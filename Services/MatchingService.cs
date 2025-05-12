using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Services
{
    public class MatchingService
    {
        private readonly ApplicationDbContext _context;

        public MatchingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserProfile>> FindMatchesForUserAsync(int userProfileId)
        {
            // Ottieni le competenze dell'utente
            var userSkills = await _context.Skills
                .Where(s => s.UserProfileId == userProfileId)
                .ToListAsync();

            var offeredSkillNames = userSkills.Where(s => s.IsOffered).Select(s => s.Name.ToLower());
            var requestedSkillNames = userSkills.Where(s => !s.IsOffered).Select(s => s.Name.ToLower());

            // Trova gli utenti con competenze compatibili
            var matchingProfileIds = await _context.Skills
                .Where(s =>
                    // Altri offrono ciò che io cerco
                    (s.IsOffered && requestedSkillNames.Contains(s.Name.ToLower())) ||
                    // Altri cercano ciò che io offro
                    (!s.IsOffered && offeredSkillNames.Contains(s.Name.ToLower())))
                .Select(s => s.UserProfileId)
                .Distinct()
                .Where(id => id != userProfileId)
                .ToListAsync();

            // Carica i profili completi degli utenti con le loro competenze
            var matchingProfiles = await _context.UserProfiles
                .Include(p => p.Skills)
                .Where(p => matchingProfileIds.Contains(p.Id))
                .ToListAsync();

            return matchingProfiles.OrderByDescending(p =>
                // Punteggio basato sul numero di match
                p.Skills.Count(s => s.IsOffered && requestedSkillNames.Contains(s.Name.ToLower())) +
                p.Skills.Count(s => !s.IsOffered && offeredSkillNames.Contains(s.Name.ToLower()))
            ).ToList();
        }
    }
}