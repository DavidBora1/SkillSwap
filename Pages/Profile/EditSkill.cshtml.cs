using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Pages.Profile
{
    [Authorize]
    public class EditSkillModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<EditSkillModel> _logger;

        public EditSkillModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<EditSkillModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public Skill Skill { get; set; } = new Skill();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            _logger.LogInformation("OnGetAsync chiamato con ID: {Id}", id);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Utente non trovato per modifica competenza");
                    return NotFound("Utente non trovato.");
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (userProfile == null)
                {
                    _logger.LogWarning("Profilo utente non trovato");
                    return NotFound("Profilo utente non trovato.");
                }

                Skill = await _context.Skills
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserProfileId == userProfile.Id);

                if (Skill == null)
                {
                    _logger.LogWarning("Competenza {Id} non trovata", id);
                    return NotFound("Competenza non trovata.");
                }

                _logger.LogInformation("Competenza {SkillId} caricata con successo: {SkillName}", id, Skill.Name);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della competenza {Id}", id);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Tentativo di aggiornamento competenza {Id}", Skill.Id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState non valido durante modifica competenza");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null || userProfile.Id != Skill.UserProfileId)
            {
                return NotFound("Profilo utente non trovato o non autorizzato.");
            }

            var existingSkill = await _context.Skills
                .FirstOrDefaultAsync(m => m.Id == Skill.Id && m.UserProfileId == userProfile.Id);

            if (existingSkill == null)
            {
                return NotFound("Competenza non trovata.");
            }

            // Aggiorna i campi
            existingSkill.Name = Skill.Name;
            existingSkill.Description = Skill.Description;
            existingSkill.Category = Skill.Category;

            if (existingSkill.IsOffered)
            {
                existingSkill.ProficiencyLevel = Skill.ProficiencyLevel;
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Competenza {Id} aggiornata con successo", Skill.Id);
                TempData["SuccessMessage"] = "Competenza aggiornata con successo!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Errore concorrenza durante l'aggiornamento della competenza {Id}", Skill.Id);
                ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio. Riprova.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generico durante l'aggiornamento della competenza {Id}", Skill.Id);
                ModelState.AddModelError("", "Si è verificato un errore imprevisto. Riprova più tardi.");
                return Page();
            }
        }
    }
}