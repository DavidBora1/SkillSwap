using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SkillSwap.Controllers
{
    [Authorize]
    public class RedirectController : Controller
    {
        private readonly ILogger<RedirectController> _logger;

        public RedirectController(ILogger<RedirectController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/Redirect/EditSkill/{id}")]
        public IActionResult EditSkill(int id)
        {
            _logger.LogInformation("Controller: Redirezione a EditSkill con ID: {Id}", id);
            return RedirectToPage("/Profile/EditSkill", new { id });
        }

        [HttpGet]
        [Route("/Redirect/EditProfile")]
        public IActionResult EditProfile()
        {
            _logger.LogInformation("Controller: Redirezione a Edit Profile");
            return RedirectToPage("/Profile/Edit");
        }
    }
}