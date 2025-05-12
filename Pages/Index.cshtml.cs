using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using SkillSwap.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillSwap.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly QuoteService _quoteService;

        public IndexModel(
            ILogger<IndexModel> logger,
            ApplicationDbContext context,
            QuoteService quoteService = null)
        {
            _logger = logger;
            _context = context;
            _quoteService = quoteService;
        }

        public string DailyQuote { get; private set; } = "L'educazione è l'arma più potente che puoi utilizzare per cambiare il mondo.";
        public string QuoteAuthor { get; private set; } = "Nelson Mandela";
        public int StatsTotalUsers { get; private set; }
        public int StatsOfferedSkills { get; private set; }
        public int StatsRequestedSkills { get; private set; }
        public int StatsSuccessfulMatches { get; private set; }

        public class SkillStatViewModel
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public int Count { get; set; }
        }

        public List<SkillStatViewModel> TopRequestedSkills { get; private set; } = new List<SkillStatViewModel>();

        public async Task OnGetAsync()
        {
            try
            {
                if (_quoteService != null)
                {
                    try
                    {
                        (string quote, string author) = await _quoteService.GetDailyQuoteAsync();
                        DailyQuote = quote;
                        QuoteAuthor = author;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Errore nel recuperare la citazione del giorno");
                    }
                }


                // Recupera le statistiche
                StatsTotalUsers = await _context.UserProfiles.CountAsync();
                StatsOfferedSkills = await _context.Skills.CountAsync(s => s.IsOffered);
                StatsRequestedSkills = await _context.Skills.CountAsync(s => !s.IsOffered);

                // Il numero di match potrebbe essere calcolato dal numero di conversazioni iniziate
                // Per ora usiamo un numero approssimativo
                StatsSuccessfulMatches = await _context.Messages
                    .Select(m => new { m.SenderId, m.RecipientId })
                    .Distinct()
                    .CountAsync();

                // Recupera le competenze più richieste
                TopRequestedSkills = await _context.Skills
                    .Where(s => !s.IsOffered)
                    .GroupBy(s => new { s.Name, s.Category })
                    .Select(g => new SkillStatViewModel
                    {
                        Name = g.Key.Name,
                        Category = g.Key.Category,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(8)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati per la home page");
            }
        }
    }
}