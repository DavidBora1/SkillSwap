using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkillSwap.Pages
{
    public class TestAuthModel : PageModel
    {
        private readonly ILogger<TestAuthModel> _logger;

        public TestAuthModel(ILogger<TestAuthModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
            _logger.LogInformation($"Username: {User.Identity?.Name ?? "null"}");
            _logger.LogInformation($"Cookie count: {Request.Cookies.Count}");
        }

        public IActionResult OnGetClearCookies()
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            _logger.LogInformation("Tutti i cookie sono stati eliminati");
            return RedirectToPage();
        }
    }
}