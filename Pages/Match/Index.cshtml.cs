using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using SkillSwap.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillSwap.Pages.Match
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GeminiService _geminiService;
        private readonly MatchingService _matchingService;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            GeminiService geminiService,
            MatchingService matchingService)
        {
            _context = context;
            _userManager = userManager;
            _geminiService = geminiService;
            _matchingService = matchingService;
        }

        public class MatchViewModel
        {
            public int Id { get; set; }
            public string UserEmail { get; set; }
            public string Bio { get; set; }
            public string ProfileImageUrl { get; set; }
            public IEnumerable<Skill> OfferedSkills { get; set; }
            public IEnumerable<Skill> RequestedSkills { get; set; }
            public string ConversationStarters { get; set; }
        }

        public IEnumerable<MatchViewModel> Matches { get; set; } = new List<MatchViewModel>();
        public bool HasSkills { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .Include(u => u.Skills)
                .FirstOrDefaultAsync(u => u.UserId == user.Id);

            if (userProfile == null || !userProfile.Skills.Any())
            {
                HasSkills = false;
                return Page();
            }

            HasSkills = true;

            var matchingProfiles = await FindMatchingProfilesAsync(user.Id, userProfile.Id);
            var matchViewModels = new List<MatchViewModel>();

            foreach (var profile in matchingProfiles)
            {
                // Ottieni l'email dell'utente
                var matchedUser = await _userManager.FindByIdAsync(profile.UserId);
                if (matchedUser == null) continue;

                var offeredSkills = profile.Skills.Where(s => s.IsOffered).ToList();
                var requestedSkills = profile.Skills.Where(s => !s.IsOffered).ToList();

                // Prepara suggerimenti per la conversazione
                string conversationStarters = null;
                try
                {
                    if (_geminiService != null && offeredSkills.Any() && userProfile.Skills.Any(s => s.IsOffered))
                    {
                        // Scegli una competenza a caso da entrambi
                        var userOfferedSkill = userProfile.Skills.FirstOrDefault(s => s.IsOffered)?.Name;
                        var matchOfferedSkill = offeredSkills.FirstOrDefault()?.Name;

                        if (userOfferedSkill != null && matchOfferedSkill != null)
                        {
                            conversationStarters = await _geminiService.GetConversationStarters(userOfferedSkill, matchOfferedSkill);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log dell'errore
                    Console.WriteLine($"Errore nell'ottenere suggerimenti: {ex.Message}");
                }

                matchViewModels.Add(new MatchViewModel
                {
                    Id = profile.Id,
                    UserEmail = matchedUser.Email,
                    Bio = profile.Bio,
                    ProfileImageUrl = profile.ProfileImageUrl,
                    OfferedSkills = offeredSkills,
                    RequestedSkills = requestedSkills,
                    ConversationStarters = conversationStarters
                });
            }

            Matches = matchViewModels;
            return Page();
        }

        private async Task<List<UserProfile>> FindMatchingProfilesAsync(string userId, int userProfileId)
        {
            // Ottieni le competenze dell'utente corrente
            var userSkills = await _context.Skills
                .Where(s => s.UserProfileId == userProfileId)
                .ToListAsync();

            var offeredSkillNames = userSkills.Where(s => s.IsOffered).Select(s => s.Name.ToLower());
            var requestedSkillNames = userSkills.Where(s => !s.IsOffered).Select(s => s.Name.ToLower());

            // Trova gli utenti i cui profili hanno compatibilità
            var matchingProfiles = await _context.UserProfiles
                .Where(up => up.UserId != userId) // Escludi l'utente corrente
                .Include(up => up.Skills) // Carica le competenze
                .ToListAsync();

            // Filtra i profili che hanno compatibilità con le competenze dell'utente
            return matchingProfiles
                .Where(up =>
                    // Altri offrono ciò che io cerco
                    up.Skills.Any(s => s.IsOffered && requestedSkillNames.Contains(s.Name.ToLower())) ||
                    // Altri cercano ciò che io offro
                    up.Skills.Any(s => !s.IsOffered && offeredSkillNames.Contains(s.Name.ToLower())))
                .OrderByDescending(up =>
                    // Punteggio di compatibilità
                    up.Skills.Count(s => s.IsOffered && requestedSkillNames.Contains(s.Name.ToLower())) +
                    up.Skills.Count(s => !s.IsOffered && offeredSkillNames.Contains(s.Name.ToLower())))
                .ToList();
        }
    }
}