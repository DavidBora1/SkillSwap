using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillSwap.Pages.Exchanges
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public UserProfile CurrentUserProfile { get; set; }
        public IList<Exchange> PendingExchanges { get; set; } = new List<Exchange>();
        public IList<Exchange> ActiveExchanges { get; set; } = new List<Exchange>();
        public IList<Exchange> CompletedExchanges { get; set; } = new List<Exchange>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Utente non trovato durante il caricamento della pagina degli scambi");
                    return RedirectToPage("/Account/Login");
                }

                CurrentUserProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (CurrentUserProfile == null)
                {
                    _logger.LogWarning("Profilo utente non trovato per l'utente {UserId}", user.Id);
                    TempData["ErrorMessage"] = "Il tuo profilo non è stato trovato. Contatta l'amministratore.";
                    return RedirectToPage("/Index");
                }

                // Carica tutti gli scambi relativi all'utente corrente
                var allExchanges = await _context.Exchanges
                    .Include(e => e.Requestor).ThenInclude(r => r.User)
                    .Include(e => e.Provider).ThenInclude(p => p.User)
                    .Include(e => e.RequestedSkill)
                    .Include(e => e.OfferedSkill)
                    .Where(e => e.ProviderId == CurrentUserProfile.Id || e.RequestorId == CurrentUserProfile.Id)
                    .ToListAsync();

                // Filtra gli scambi in base allo stato
                PendingExchanges = allExchanges.Where(e => e.Status == ExchangeStatus.Pending).ToList();
                ActiveExchanges = allExchanges.Where(e => e.Status == ExchangeStatus.Accepted).ToList();
                CompletedExchanges = allExchanges.Where(e => e.Status == ExchangeStatus.Completed).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della pagina degli scambi");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento degli scambi. Riprova più tardi.";
                return RedirectToPage("/Index");
            }
        }

        public bool IsExchangeInitiator(Exchange exchange)
        {
            return exchange.RequestorId == CurrentUserProfile.Id;
        }

        public async Task<IActionResult> OnPostAcceptAsync(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null || exchange.ProviderId != userProfile.Id)
            {
                return Forbid();
            }

            exchange.Status = ExchangeStatus.Accepted;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null || exchange.ProviderId != userProfile.Id)
            {
                return Forbid();
            }

            exchange.Status = ExchangeStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (userProfile == null || (exchange.ProviderId != userProfile.Id && exchange.RequestorId != userProfile.Id))
            {
                return Forbid();
            }

            // Solo gli scambi accettati possono essere completati
            if (exchange.Status != ExchangeStatus.Accepted)
            {
                return BadRequest("Solo gli scambi accettati possono essere completati");
            }

            exchange.Status = ExchangeStatus.Completed;
            exchange.CompletedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}