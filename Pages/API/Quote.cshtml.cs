using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillSwap.Services;

namespace SkillSwap.Pages.Api
{
    public class QuoteModel : PageModel
    {
        private readonly QuoteService _quoteService;
        private readonly ILogger<QuoteModel> _logger;

        public QuoteModel(QuoteService quoteService, ILogger<QuoteModel> logger)
        {
            _quoteService = quoteService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetRefreshAsync()
        {
            try
            {
                _logger.LogInformation("API: Richiesta refresh citazione");
                var (quote, author) = await _quoteService.GetDailyQuoteAsync(true);
                return new JsonResult(new { quote, author });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Errore refresh citazione");
                return new JsonResult(new
                {
                    quote = "La condivisione è la chiave per crescere insieme.",
                    author = "SkillSwap"
                });
            }
        }
    }
}