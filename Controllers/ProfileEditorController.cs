using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SkillSwap.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileEditorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProfileEditorController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileEditorController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<ProfileEditorController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserProfileUpdateDto dto)
        {
            _logger.LogInformation("Tentativo di aggiornamento profilo");

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Utente non trovato");
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (userProfile == null || userProfile.Id != dto.Id)
                {
                    return Unauthorized("Profilo utente non trovato o non autorizzato");
                }

                // Aggiorna la biografia
                userProfile.Bio = dto.Bio;

                // Gestisci l'immagine
                if (dto.RemoveExistingImage)
                {
                    // Rimuovi l'immagine esistente se richiesto
                    if (!string.IsNullOrEmpty(userProfile.ProfileImageUrl))
                    {
                        DeleteProfileImage(userProfile.ProfileImageUrl);
                        userProfile.ProfileImageUrl = "";
                    }
                }
                else if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
                {
                    // Se è stata caricata una nuova immagine
                    if (!string.IsNullOrEmpty(userProfile.ProfileImageUrl))
                    {
                        DeleteProfileImage(userProfile.ProfileImageUrl);
                    }

                    userProfile.ProfileImageUrl = await SaveProfileImage(dto.ProfileImage);
                }
                // Nota: se non è stata fatta alcuna azione sull'immagine, mantieni quella esistente

                await _context.SaveChangesAsync();
                _logger.LogInformation("Profilo aggiornato con successo");

                return Redirect("/Profile/Index?success=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del profilo: {Message}", ex.Message);
                return StatusCode(500, $"Si è verificato un errore durante il salvataggio: {ex.Message}");
            }
        }

        private async Task<string> SaveProfileImage(IFormFile image)
        {
            if (image.Length <= 0) return string.Empty;

            // Verifica se l'immagine è troppo grande (2MB)
            if (image.Length > 2 * 1024 * 1024)
            {
                throw new InvalidOperationException("L'immagine deve essere inferiore a 2MB");
            }

            // Verifica il tipo di file
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                throw new InvalidOperationException("Solo i formati JPG e PNG sono supportati");
            }

            // Crea un nome file univoco
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");

            // Assicurati che la directory esista
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Restituisci l'URL relativo dell'immagine
            return $"/images/profiles/{fileName}";
        }

        private void DeleteProfileImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                // Estrai il percorso relativo dall'URL
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation("Eliminata immagine profilo: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'immagine profilo");
            }
        }
    }
}