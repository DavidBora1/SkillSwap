using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkillSwap.Pages.Profile
{
    [Authorize]
    public class TestEditSkillModel : PageModel
    {
        private readonly ILogger<TestEditSkillModel> _logger;

        public TestEditSkillModel(ILogger<TestEditSkillModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Accesso alla pagina di test per EditSkill");
        }
    }
}