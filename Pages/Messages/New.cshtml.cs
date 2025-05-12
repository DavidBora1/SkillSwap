using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Pages.Messages
{
    [Authorize]
    public class NewModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<NewModel> _logger;

        public NewModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<NewModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public class MessageInputModel
        {
            [Required(ErrorMessage = "Seleziona un destinatario")]
            [Display(Name = "Destinatario")]
            public int RecipientId { get; set; }

            [Required(ErrorMessage = "Il messaggio non può essere vuoto")]
            [Display(Name = "Messaggio")]
            [MinLength(5, ErrorMessage = "Il messaggio deve contenere almeno 5 caratteri")]
            public string Content { get; set; }
        }

        [BindProperty]
        public MessageInputModel Message { get; set; } = new MessageInputModel();

        public SelectList Recipients { get; set; }
        public string RecipientName { get; set; }
        public bool IsReply { get; set; }

        public async Task<IActionResult> OnGetAsync(int? recipientId)
        {
            _logger.LogInformation("OnGet con recipientId: {RecipientId}", recipientId);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Utente corrente non trovato");
                return NotFound("Utente non trovato");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (userProfile == null)
            {
                _logger.LogWarning("Profilo utente non trovato");
                return NotFound("Profilo utente non trovato");
            }

            // Se è specificato un destinatario, è una risposta
            if (recipientId.HasValue)
            {
                IsReply = true;
                Message.RecipientId = recipientId.Value;

                // Ottieni il nome del destinatario
                var recipient = await _context.UserProfiles
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == recipientId);

                if (recipient == null)
                {
                    _logger.LogWarning("Destinatario non trovato: {RecipientId}", recipientId);
                    return NotFound("Destinatario non trovato");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipient.UserId);
                RecipientName = recipientUser?.Email ?? "Utente sconosciuto";
                _logger.LogInformation("Risposta a: {RecipientName}", RecipientName);
            }
            else
            {
                IsReply = false;
                // Prepara la lista di destinatari potenziali
                var potentialRecipients = await _context.UserProfiles
                    .Include(p => p.User)
                    .Where(p => p.UserId != user.Id)
                    .ToListAsync();

                var recipientItems = new List<SelectListItem>();

                foreach (var recipient in potentialRecipients)
                {
                    var recipientUser = await _userManager.FindByIdAsync(recipient.UserId);
                    recipientItems.Add(new SelectListItem
                    {
                        Value = recipient.Id.ToString(),
                        Text = recipientUser?.Email ?? "Utente sconosciuto"
                    });
                }

                Recipients = new SelectList(recipientItems, "Value", "Text");
                _logger.LogInformation("Preparati {Count} destinatari potenziali", recipientItems.Count);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPost con RecipientId: {RecipientId}", Message.RecipientId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState non valido: {Errors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));

                // Ricarica la lista di destinatari per il rendering della pagina
                await ReloadRecipientsAsync();
                return Page();
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var senderProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

                if (senderProfile == null)
                {
                    _logger.LogWarning("Profilo mittente non trovato");
                    return NotFound("Profilo mittente non trovato");
                }

                var recipientProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.Id == Message.RecipientId);

                if (recipientProfile == null)
                {
                    _logger.LogWarning("Destinatario non trovato: {RecipientId}", Message.RecipientId);
                    ModelState.AddModelError("Message.RecipientId", "Destinatario non valido");
                    await ReloadRecipientsAsync();
                    return Page();
                }

                // Crea il messaggio
                var newMessage = new Message
                {
                    Content = Message.Content,
                    SenderId = senderProfile.Id,
                    RecipientId = recipientProfile.Id,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Messaggio inviato con successo: {Id}", newMessage.Id);
                TempData["SuccessMessage"] = "Messaggio inviato con successo!";

                // Redirect alla pagina dei messaggi inviati
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio del messaggio");
                ModelState.AddModelError("", $"Errore durante l'invio: {ex.Message}");
                await ReloadRecipientsAsync();
                return Page();
            }
        }

        private async Task ReloadRecipientsAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (Message.RecipientId > 0)
            {
                IsReply = true;
                var recipient = await _context.UserProfiles
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == Message.RecipientId);

                if (recipient != null)
                {
                    var recipientUser = await _userManager.FindByIdAsync(recipient.UserId);
                    RecipientName = recipientUser?.Email ?? "Utente sconosciuto";
                }
            }
            else
            {
                IsReply = false;
                var potentialRecipients = await _context.UserProfiles
                    .Include(p => p.User)
                    .Where(p => p.UserId != user.Id)
                    .ToListAsync();

                var recipientItems = new List<SelectListItem>();

                foreach (var recipient in potentialRecipients)
                {
                    var recipientUser = await _userManager.FindByIdAsync(recipient.UserId);
                    recipientItems.Add(new SelectListItem
                    {
                        Value = recipient.Id.ToString(),
                        Text = recipientUser?.Email ?? "Utente sconosciuto"
                    });
                }

                Recipients = new SelectList(recipientItems, "Value", "Text");
            }
        }
    }
}