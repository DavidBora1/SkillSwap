using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using SkillSwap.Services;

namespace SkillSwap.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<IndexModel> _logger;
        private readonly GeminiService _geminiService;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<IndexModel> logger,
            GeminiService geminiService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _geminiService = geminiService;
        }

        public UserProfile UserProfile { get; set; }
        public List<Skill> OfferedSkills { get; set; }
        public List<Skill> RequestedSkills { get; set; }
        public bool IsOtherUserProfile { get; set; }

        public async Task<IActionResult> OnGetAsync(int? userId = null, bool success = false)
        {
            if (success)
            {
                TempData["SuccessMessage"] = "Competenza aggiornata con successo!";
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound($"Non è stato possibile caricare l'utente con ID '{_userManager.GetUserId(User)}'.");
            }

            // Se userId è specificato, carica il profilo di quell'utente, altrimenti carica il profilo dell'utente corrente
            UserProfile userProfile;
            if (userId.HasValue)
            {
                userProfile = await _context.UserProfiles
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(m => m.Id == userId.Value);

                if (userProfile == null)
                {
                    return NotFound("Profilo utente non trovato.");
                }

                IsOtherUserProfile = userProfile.UserId != currentUser.Id;
            }
            else
            {
                // Carica il profilo utente corrente
                userProfile = await _context.UserProfiles
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(m => m.UserId == currentUser.Id);

                IsOtherUserProfile = false;

                if (userProfile == null)
                {
                    // Se non esiste un profilo, lo crea
                    userProfile = new UserProfile
                    {
                        UserId = currentUser.Id,
                        User = currentUser,
                        JoinDate = DateTime.Now,
                        Bio = "",
                        ProfileImageUrl = ""
                    };
                    _context.UserProfiles.Add(userProfile);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Creato nuovo profilo per l'utente {UserId}", currentUser.Id);
                }
            }

            UserProfile = userProfile;

            // Carica le competenze
            OfferedSkills = await _context.Skills
                .Where(s => s.UserProfileId == UserProfile.Id && s.IsOffered)
                .ToListAsync();

            RequestedSkills = await _context.Skills
                .Where(s => s.UserProfileId == UserProfile.Id && !s.IsOffered)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSkillAsync(int id)
        {
            _logger.LogInformation("Richiesta eliminazione competenza ID: {Id}", id);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id && s.UserProfileId == userProfile.Id);

            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Competenza {Id} eliminata con successo", id);
                TempData["SuccessMessage"] = "Competenza eliminata con successo!";
            }
            else
            {
                _logger.LogWarning("Competenza {Id} non trovata o non autorizzata", id);
                TempData["ErrorMessage"] = "Competenza non trovata o non autorizzata.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostImproveSkillAsync(int id)
        {
            _logger.LogInformation("Richiesta miglioramento competenza ID: {Id}", id);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id && s.UserProfileId == userProfile.Id);

            if (skill != null && _geminiService != null)
            {
                try
                {
                    var improvedDescription = await _geminiService.ImproveSkillDescription(
                        skill.Name, skill.Description);

                    if (!string.IsNullOrEmpty(improvedDescription))
                    {
                        skill.Description = improvedDescription;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Descrizione competenza {Id} migliorata con successo", id);
                        TempData["SuccessMessage"] = "La descrizione della competenza è stata migliorata!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Non è stato possibile migliorare la descrizione.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il miglioramento della descrizione della competenza {Id}", id);
                    TempData["ErrorMessage"] = "Si è verificato un errore durante il miglioramento della descrizione.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Competenza non trovata o servizio AI non disponibile.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostNavigateToEditSkillAsync(int id)
        {
            _logger.LogInformation("Navigazione a EditSkill con ID: {Id}", id);
            return RedirectToPage("/Profile/EditSkill", new { id });
        }

        public IActionResult OnPostNavigateToEditProfileAsync()
        {
            _logger.LogInformation("Navigazione a EditProfile");
            return RedirectToPage("/Profile/Edit");
        }
    }
}