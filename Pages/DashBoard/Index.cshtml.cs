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

namespace SkillSwap.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GeminiService _geminiService;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            GeminiService geminiService)
        {
            _context = context;
            _userManager = userManager;
            _geminiService = geminiService;
        }

        public class MatchViewModel
        {
            public int Id { get; set; }
            public string UserEmail { get; set; }
            public string MatchReason { get; set; } // "offer" o "request"
            public int MatchScore { get; set; }
        }

        public class ActivityViewModel
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime Date { get; set; }
            public string Icon { get; set; }
            public string IconClass { get; set; }
        }

        public string UserName { get; set; }
        public int OfferedSkillsCount { get; set; }
        public int RequestedSkillsCount { get; set; }
        public int TotalSkills => OfferedSkillsCount + RequestedSkillsCount;
        public int SentMessagesCount { get; set; }
        public int ReceivedMessagesCount { get; set; }
        public List<MatchViewModel> TopMatches { get; set; } = new List<MatchViewModel>();
        public List<ActivityViewModel> RecentActivities { get; set; } = new List<ActivityViewModel>();
        public string AiTip { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            UserName = user.UserName;

            var userProfile = await _context.UserProfiles
                .Include(p => p.Skills)
                .Include(p => p.SentMessages)
                .Include(p => p.ReceivedMessages)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (userProfile == null)
            {
                return NotFound("Profilo utente non trovato");
            }

            // Statistiche di base
            OfferedSkillsCount = userProfile.Skills.Count(s => s.IsOffered);
            RequestedSkillsCount = userProfile.Skills.Count(s => !s.IsOffered);
            SentMessagesCount = userProfile.SentMessages.Count;
            ReceivedMessagesCount = userProfile.ReceivedMessages.Count;

            // Carica i migliori match
            await LoadTopMatchesAsync(userProfile);

            // Carica le attività recenti
            await LoadRecentActivitiesAsync(userProfile);

            // Ottieni suggerimenti dall'IA
            await GetAiTipAsync(userProfile);

            return Page();
        }

        private async Task LoadTopMatchesAsync(UserProfile userProfile)
        {
            var offeredSkills = userProfile.Skills
                .Where(s => s.IsOffered)
                .Select(s => s.Name.ToLower())
                .ToList();

            var requestedSkills = userProfile.Skills
                .Where(s => !s.IsOffered)
                .Select(s => s.Name.ToLower())
                .ToList();

            // Trova potenziali match
            var matchingProfiles = await _context.UserProfiles
                .Include(p => p.Skills)
                .Include(p => p.User)
                .Where(p => p.Id != userProfile.Id)
                .ToListAsync();

            var scoredMatches = new List<(UserProfile Profile, int Score, string Reason)>();

            foreach (var profile in matchingProfiles)
            {
                var theirOfferedSkills = profile.Skills.Where(s => s.IsOffered).Select(s => s.Name.ToLower()).ToList();
                var theirRequestedSkills = profile.Skills.Where(s => !s.IsOffered).Select(s => s.Name.ToLower()).ToList();

                // Calcola punteggio per "loro offrono ciò che io cerco"
                int offerScore = 0;
                foreach (var myRequest in requestedSkills)
                {
                    if (theirOfferedSkills.Any(s => s.Contains(myRequest) || myRequest.Contains(s)))
                    {
                        offerScore += 3;
                    }
                }

                // Calcola punteggio per "loro cercano ciò che io offro"
                int requestScore = 0;
                foreach (var myOffer in offeredSkills)
                {
                    if (theirRequestedSkills.Any(s => s.Contains(myOffer) || myOffer.Contains(s)))
                    {
                        requestScore += 2;
                    }
                }

                if (offerScore > 0 || requestScore > 0)
                {
                    string reason = offerScore > requestScore ? "offer" : "request";
                    int totalScore = offerScore + requestScore;
                    scoredMatches.Add((profile, totalScore, reason));
                }
            }

            // Ordina per punteggio e prendi i primi 3
            TopMatches = scoredMatches
                .OrderByDescending(m => m.Score)
                .Take(3)
                .Select(m => new MatchViewModel
                {
                    Id = m.Profile.Id,
                    UserEmail = m.Profile.User?.Email ?? "Utente sconosciuto",
                    MatchReason = m.Reason,
                    MatchScore = m.Score
                })
                .ToList();
        }

        private async Task LoadRecentActivitiesAsync(UserProfile userProfile)
        {
            var activities = new List<ActivityViewModel>();

            // Messaggi recenti
            var recentMessages = await _context.Messages
                .Where(m => m.SenderId == userProfile.Id || m.RecipientId == userProfile.Id)
                .OrderByDescending(m => m.SentAt)
                .Take(3)
                .ToListAsync();

            foreach (var message in recentMessages)
            {
                bool isSent = message.SenderId == userProfile.Id;
                activities.Add(new ActivityViewModel
                {
                    Title = isSent ? "Messaggio inviato" : "Messaggio ricevuto",
                    Description = isSent ? "Hai inviato un messaggio" : "Hai ricevuto un messaggio",
                    Date = message.SentAt,
                    Icon = isSent ? "bi-send" : "bi-envelope",
                    IconClass = "icon-message"
                });
            }

            // Competenze recenti (usando l'ID come approssimazione - in un'app reale avresti un timestamp)
            var recentSkills = await _context.Skills
                .Where(s => s.UserProfileId == userProfile.Id)
                .OrderByDescending(s => s.Id)
                .Take(3)
                .ToListAsync();

            foreach (var skill in recentSkills)
            {
                activities.Add(new ActivityViewModel
                {
                    Title = skill.IsOffered ? "Competenza offerta aggiunta" : "Competenza richiesta aggiunta",
                    Description = $"{skill.Name} ({skill.Category})",
                    Date = DateTime.Now.AddDays(-skill.Id % 7), // Simulazione di data - in un'app reale avresti un timestamp
                    Icon = skill.IsOffered ? "bi-lightbulb" : "bi-journal-bookmark",
                    IconClass = "icon-skill"
                });
            }

            // Aggiungi login
            activities.Add(new ActivityViewModel
            {
                Title = "Accesso effettuato",
                Description = "Hai effettuato l'accesso alla piattaforma",
                Date = DateTime.Now,
                Icon = "bi-box-arrow-in-right",
                IconClass = "icon-login"
            });

            // Ordina per data e prendi i primi 5
            RecentActivities = activities
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList();
        }

        private async Task GetAiTipAsync(UserProfile userProfile)
        {
            if (TotalSkills == 0)
            {
                AiTip = null;
                return;
            }

            try
            {
                // Prepara una descrizione delle competenze dell'utente
                var offeredSkillsStr = string.Join(", ",
                    userProfile.Skills
                        .Where(s => s.IsOffered)
                        .Select(s => s.Name));

                var requestedSkillsStr = string.Join(", ",
                    userProfile.Skills
                        .Where(s => !s.IsOffered)
                        .Select(s => s.Name));

                string skillsDescription = $"Competenze offerte: {offeredSkillsStr}. Competenze richieste: {requestedSkillsStr}.";

                // Ottieni suggerimento dall'IA
                AiTip = await _geminiService.GetSkillSuggestions(skillsDescription);
            }
            catch (Exception)
            {
                // Fallback in caso di errore
                AiTip = "Per migliorare il tuo profilo, considera di aggiungere descrizioni dettagliate alle tue competenze e bilanciare ciò che offri con ciò che cerchi di imparare.";
            }
        }
    }
}