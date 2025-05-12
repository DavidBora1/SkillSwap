using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System;
using System.Threading.Tasks;

namespace SkillSwap.Pages.Exchanges
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<DetailsModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public Exchange Exchange { get; set; }
        public bool CanUserRateExchange { get; set; }
        public bool IsProvider { get; set; }
        public bool IsRequestor { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
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

            Exchange = await _context.Exchanges
                .Include(e => e.Requestor)
                .Include(e => e.Provider)
                .Include(e => e.RequestedSkill)
                .Include(e => e.OfferedSkill)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Exchange == null)
            {
                return NotFound();
            }

            // Determina se l'utente è il fornitore o il richiedente dello scambio
            IsProvider = Exchange.ProviderId == userProfile.Id;
            IsRequestor = Exchange.RequestorId == userProfile.Id;

            // Verifica se l'utente può lasciare un feedback
            CanUserRateExchange = Exchange.Status == ExchangeStatus.Completed &&
                ((IsProvider && !Exchange.ProviderRating.HasValue) ||
                (IsRequestor && !Exchange.RequestorRating.HasValue));

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitFeedbackAsync(int exchangeId, int rating, string feedback)
        {
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("Rating", "La valutazione deve essere compresa tra 1 e 5");
                return RedirectToPage("./Details", new { id = exchangeId });
            }

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

            var exchange = await _context.Exchanges
                .Include(e => e.Requestor)
                .Include(e => e.Provider)
                .FirstOrDefaultAsync(e => e.Id == exchangeId);

            if (exchange == null || exchange.Status != ExchangeStatus.Completed)
            {
                return NotFound();
            }

            bool isProvider = exchange.ProviderId == userProfile.Id;
            bool isRequestor = exchange.RequestorId == userProfile.Id;

            if (!isProvider && !isRequestor)
            {
                return Forbid();
            }

            // Aggiorna il rating appropriato
            if (isProvider && !exchange.ProviderRating.HasValue)
            {
                exchange.ProviderRating = rating;
                exchange.ProviderFeedback = feedback;

                // Aggiorna la reputazione del richiedente
                await UpdateUserReputationAsync(exchange.RequestorId, rating);
            }
            else if (isRequestor && !exchange.RequestorRating.HasValue)
            {
                exchange.RequestorRating = rating;
                exchange.RequestorFeedback = feedback;

                // Aggiorna la reputazione del fornitore
                await UpdateUserReputationAsync(exchange.ProviderId, rating);
            }
            else
            {
                // L'utente ha già lasciato un feedback
                return RedirectToPage("./Details", new { id = exchangeId });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Details", new { id = exchangeId });
        }

        private async Task UpdateUserReputationAsync(int userProfileId, int rating)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == userProfileId);

            if (userProfile == null)
            {
                return;
            }

            // Calcola il nuovo rating medio
            double totalPoints = userProfile.AverageRating * userProfile.TotalRatings;
            totalPoints += rating;
            userProfile.TotalRatings++;
            userProfile.AverageRating = totalPoints / userProfile.TotalRatings;

            // Incrementa il conteggio degli scambi completati se è il primo rating per questo scambio
            userProfile.ExchangesCompleted = await _context.Exchanges
                .Where(e => (e.ProviderId == userProfileId || e.RequestorId == userProfileId)
                       && e.Status == ExchangeStatus.Completed)
                .CountAsync();

            await _context.SaveChangesAsync();
        }
    }
}