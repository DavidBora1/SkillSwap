using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillSwap.Pages.Exchanges
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<CreateModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ExchangeViewModel Exchange { get; set; }

        public UserProfile Provider { get; set; }

        public SelectList ProviderSkills { get; set; }
        public SelectList MySkills { get; set; }

        public async Task<IActionResult> OnGetAsync(int providerId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null)
            {
                return NotFound("Profilo utente non trovato.");
            }

            Provider = await _context.UserProfiles
                .Include(u => u.User)
                .Include(u => u.Skills)
                .FirstOrDefaultAsync(u => u.Id == providerId);

            if (Provider == null)
            {
                return NotFound("Profilo del provider non trovato.");
            }

            // Non permettere di scambiare con se stessi
            if (Provider.Id == userProfile.Id)
            {
                _logger.LogWarning("Tentativo di scambio con se stessi");
                return RedirectToPage("/Profile/Index");
            }

            // Imposta le competenze disponibili per lo scambio
            ProviderSkills = new SelectList(
                Provider.Skills.Where(s => s.IsOffered).ToList(),
                "Id", "Name");

            MySkills = new SelectList(
                await _context.Skills
                    .Where(s => s.UserProfileId == userProfile.Id && s.IsOffered)
                    .ToListAsync(),
                "Id", "Name");

            // Inizializza il modello con i valori corretti
            Exchange = new ExchangeViewModel
            {
                RequestorId = userProfile.Id,
                ProviderId = providerId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Inizio creazione scambio: RequestorId={RequestorId}, ProviderId={ProviderId}, RequestedSkillId={RequestedSkillId}, OfferedSkillId={OfferedSkillId}",
                Exchange?.RequestorId, Exchange?.ProviderId, Exchange?.RequestedSkillId, Exchange?.OfferedSkillId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modello non valido: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await LoadFormData();
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Utente non trovato.");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == currentUser.Id);

            if (userProfile == null)
            {
                return NotFound("Profilo utente non trovato.");
            }

            if (userProfile.Id != Exchange.RequestorId)
            {
                _logger.LogWarning("Tentativo di creare scambio come altro utente");
                return Forbid();
            }

            // Verifica che le competenze esistano
            var requestedSkill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == Exchange.RequestedSkillId && s.UserProfileId == Exchange.ProviderId && s.IsOffered);

            var offeredSkill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == Exchange.OfferedSkillId && s.UserProfileId == Exchange.RequestorId && s.IsOffered);

            if (requestedSkill == null || offeredSkill == null)
            {
                _logger.LogWarning("Competenze non valide: RequestedSkillId={RequestedSkillId}, OfferedSkillId={OfferedSkillId}",
                    Exchange.RequestedSkillId, Exchange.OfferedSkillId);
                ModelState.AddModelError("", "Le competenze selezionate non sono valide");
                await LoadFormData();
                return Page();
            }

            try
            {
                // Crea un nuovo scambio
                var exchange = new Exchange
                {
                    RequestorId = Exchange.RequestorId,
                    ProviderId = Exchange.ProviderId,
                    RequestedSkillId = Exchange.RequestedSkillId,
                    OfferedSkillId = Exchange.OfferedSkillId,
                    Status = ExchangeStatus.Pending,
                    CreatedDate = DateTime.Now
                };

                _context.Exchanges.Add(exchange);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Scambio creato con successo: Id={Id}", exchange.Id);

                TempData["SuccessMessage"] = "Scambio proposto con successo!";

                // Reindirizza al profilo dell'utente (più sicuro)
                return RedirectToPage("/Profile/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello scambio");
                ModelState.AddModelError("", "Si è verificato un errore durante la creazione dello scambio.");
                await LoadFormData();
                return Page();
            }
        }

        // Metodo helper per caricare i dati del form
        private async Task LoadFormData()
        {
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            Provider = await _context.UserProfiles
                .Include(u => u.User)
                .Include(u => u.Skills)
                .FirstOrDefaultAsync(u => u.Id == Exchange.ProviderId);

            ProviderSkills = new SelectList(
                Provider?.Skills?.Where(s => s.IsOffered).ToList() ?? new List<Skill>(),
                "Id", "Name");

            MySkills = new SelectList(
                await _context.Skills
                    .Where(s => s.UserProfileId == userProfile.Id && s.IsOffered)
                    .ToListAsync(),
                "Id", "Name");
        }
    }

    public class ExchangeViewModel
    {
        [Required(ErrorMessage = "Il richiedente è obbligatorio")]
        public int RequestorId { get; set; }

        [Required(ErrorMessage = "Il fornitore è obbligatorio")]
        public int ProviderId { get; set; }

        [Required(ErrorMessage = "Seleziona una competenza da richiedere")]
        public int RequestedSkillId { get; set; }

        [Required(ErrorMessage = "Seleziona una competenza da offrire")]
        public int OfferedSkillId { get; set; }
    }
}