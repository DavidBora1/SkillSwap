using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System;
using System.Threading.Tasks;

namespace SkillSwap.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<ProfileController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("EditSkill")]
        public async Task<IActionResult> UpdateSkill([FromForm] SkillUpdateDto dto)
        {
            _logger.LogInformation("Tentativo di aggiornamento competenza {Id}", dto.Id);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Utente non trovato");
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (userProfile == null || userProfile.Id != dto.UserProfileId)
                {
                    return Unauthorized("Profilo utente non trovato o non autorizzato");
                }

                var existingSkill = await _context.Skills
                    .FirstOrDefaultAsync(m => m.Id == dto.Id && m.UserProfileId == userProfile.Id);

                if (existingSkill == null)
                {
                    return NotFound("Competenza non trovata");
                }

                // Aggiorna i campi
                existingSkill.Name = dto.Name;
                existingSkill.Description = dto.Description;
                existingSkill.Category = dto.Category;

                // Modifica: controlla se è una competenza offerta prima di aggiornare ProficiencyLevel
                if (existingSkill.IsOffered && dto.ProficiencyLevel.HasValue)
                {
                    // Aggiungi validazione esplicita per le competenze offerte
                    if (dto.ProficiencyLevel < 1 || dto.ProficiencyLevel > 5)
                    {
                        return BadRequest("Il livello di competenza deve essere compreso tra 1 e 5");
                    }

                    existingSkill.ProficiencyLevel = dto.ProficiencyLevel.Value;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Competenza {Id} aggiornata con successo", dto.Id);

                return Redirect("/Profile/Index?success=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della competenza {Id}", dto.Id);
                return StatusCode(500, "Si è verificato un errore durante il salvataggio. Riprova più tardi.");
            }
        }
    }
}