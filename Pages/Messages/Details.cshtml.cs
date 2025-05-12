using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Pages.Messages
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<DetailsModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public class MessageDetailViewModel
        {
            public int Id { get; set; }
            public string SenderName { get; set; }
            public int SenderId { get; set; }
            public string Content { get; set; }
            public DateTime SentAt { get; set; }
        }

        public MessageDetailViewModel Message { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            _logger.LogInformation("Visualizzazione dettaglio messaggio ID: {Id}", id);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Sender)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.RecipientId == userProfile.Id);

            if (message == null)
            {
                _logger.LogWarning("Messaggio ID: {Id} non trovato o non autorizzato", id);
                return NotFound();
            }

            var senderUser = await _userManager.FindByIdAsync(message.Sender.UserId);

            // Imposta il messaggio come letto se non lo è già
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Messaggio ID: {Id} segnato come letto", id);
            }

            Message = new MessageDetailViewModel
            {
                Id = message.Id,
                SenderName = senderUser?.Email ?? "Utente sconosciuto",
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt
            };

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            _logger.LogInformation("Richiesta eliminazione messaggio ID: {Id}", id);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.RecipientId == userProfile.Id);

            if (message == null)
            {
                _logger.LogWarning("Messaggio ID: {Id} non trovato o non autorizzato per eliminazione", id);
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Messaggio ID: {Id} eliminato con successo", id);
            TempData["SuccessMessage"] = "Messaggio eliminato con successo!";

            return RedirectToPage("./Index");
        }
    }
}