using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Pages.Profile
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager,
                        IWebHostEnvironment environment, ILogger<EditModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        [BindProperty]
        public UserProfile UserProfile { get; set; } = new UserProfile();

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        [BindProperty]
        public bool RemoveExistingImage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossibile trovare l'utente con ID '{_userManager.GetUserId(User)}'.");
            }

            UserProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (UserProfile == null)
            {
                return NotFound($"Profilo utente non trovato.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossibile trovare l'utente con ID '{_userManager.GetUserId(User)}'.");
            }

            var existingProfile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == UserProfile.Id && m.UserId == user.Id);

            if (existingProfile == null)
            {
                return NotFound($"Profilo utente non trovato.");
            }

            // Gestione immagine profilo
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Validazione formato
                var extension = Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) ||
                    (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif"))
                {
                    ModelState.AddModelError("ProfileImage", "Formato file non supportato. Usa jpg, png o gif.");
                    return Page();
                }

                // Validazione dimensione (2MB max)
                if (ProfileImage.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfileImage", "L'immagine non può superare i 2MB.");
                    return Page();
                }

                try
                {
                    // Elimina immagine precedente se esiste
                    if (!string.IsNullOrEmpty(existingProfile.ProfileImageUrl))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath,
                            existingProfile.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Crea directory se non esiste
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Salva la nuova immagine
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(fileStream);
                    }

                    UserProfile.ProfileImageUrl = $"/uploads/profiles/{fileName}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il caricamento dell'immagine profilo");
                    ModelState.AddModelError("", "Si è verificato un errore durante il caricamento dell'immagine.");
                    return Page();
                }
            }
            else if (RemoveExistingImage && !string.IsNullOrEmpty(existingProfile.ProfileImageUrl))
            {
                // Elimina immagine esistente senza caricarne una nuova
                try
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath,
                        existingProfile.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    UserProfile.ProfileImageUrl = "";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la rimozione dell'immagine profilo");
                }
            }
            else
            {
                // Mantieni l'immagine esistente
                UserProfile.ProfileImageUrl = existingProfile.ProfileImageUrl;
            }

            _context.Attach(UserProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profilo aggiornato con successo!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del profilo");

                if (!UserProfileExists(UserProfile.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Si è verificato un errore durante l'aggiornamento del profilo. Riprova.");
                    return Page();
                }
            }
        }

        private bool UserProfileExists(int id)
        {
            return _context.UserProfiles.Any(e => e.Id == id);
        }
    }
}