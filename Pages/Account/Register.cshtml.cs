using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Log di debug
            _logger.LogInformation($"Tentativo registrazione per {Email}");

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Tutti i campi sono obbligatori");
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Le password non coincidono");
                return Page();
            }

            // Tentativo di creazione utente semplificato
            var user = new IdentityUser { UserName = Email, Email = Email };
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Utente {Email} creato con successo");

                // Crea anche il profilo utente
                var userProfile = new UserProfile
                {
                    UserId = user.Id,
                    JoinDate = DateTime.Now
                };
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/Index");
            }

            // Log dettagliato degli errori
            foreach (var error in result.Errors)
            {
                _logger.LogWarning($"Errore registrazione: {error.Code} - {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}