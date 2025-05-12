using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System;
using System.Threading.Tasks;

namespace SkillSwap.Pages.Profile
{
    [Authorize]
    public class AddSkillModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AddSkillModel> _logger;

        public AddSkillModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<AddSkillModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public Skill Skill { get; set; } = new Skill();

        [BindProperty(SupportsGet = true)]
        public bool IsOffered { get; set; }

        public void OnGet(bool isOffered)
        {
            IsOffered = isOffered;
            Skill.IsOffered = isOffered;

            // Imposta un livello predefinito per le competenze offerte
            if (isOffered)
            {
                Skill.ProficiencyLevel = 3;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation($"Tentativo aggiunta skill: {Skill.Name}, IsOffered: {IsOffered}");

            // Assicuriamoci che Skill.IsOffered sia impostato correttamente dal form
            Skill.IsOffered = IsOffered;

            ModelState.Remove("Skill.UserProfile"); // Rimuovi validazione proprietà di navigazione

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState non valido: {Errors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return Page();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Utente non trovato");
                    return NotFound("Utente non trovato.");
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (userProfile == null)
                {
                    _logger.LogWarning("Profilo utente non trovato");
                    return NotFound("Profilo utente non trovato.");
                }

                // Assegna la competenza al profilo utente
                Skill.UserProfileId = userProfile.Id;

                // Se è una competenza richiesta, reset ProficiencyLevel a 0
                if (!Skill.IsOffered)
                {
                    Skill.ProficiencyLevel = 0;
                }
                // Altrimenti, assicuriamoci che abbia un valore valido
                else if (Skill.ProficiencyLevel < 1 || Skill.ProficiencyLevel > 5)
                {
                    Skill.ProficiencyLevel = 3;
                }

                _context.Skills.Add(Skill);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Skill aggiunta con successo: {Id}", Skill.Id);
                TempData["SuccessMessage"] = "Competenza aggiunta con successo!";

                return RedirectToPage("/Profile/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio della skill");
                ModelState.AddModelError("", $"Errore durante il salvataggio: {ex.Message}");
                return Page();
            }
        }
    }
}