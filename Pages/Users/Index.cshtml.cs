using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillSwap.Pages.Users
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<UserProfile> Users { get; set; } = new List<UserProfile>();
        public string SearchTerm { get; set; }
        public string SkillCategory { get; set; }
        public string SkillType { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 9;

        public async Task OnGetAsync(string searchTerm, string skillCategory, string skillType, int page = 1)
        {
            SearchTerm = searchTerm;
            SkillCategory = skillCategory;
            SkillType = skillType;
            CurrentPage = Math.Max(1, page);

            // Carica le categorie disponibili
            Categories = await _context.Skills
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Ottieni l'utente corrente
            var currentUser = await _userManager.GetUserAsync(User);

            // Prepara la query per gli utenti
            var query = _context.UserProfiles
                .Include(u => u.User)
                .Include(u => u.Skills)
                .Where(u => u.UserId != currentUser.Id)
                .AsQueryable();

            // Applica filtri
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(u =>
                    u.User.UserName.Contains(SearchTerm) ||
                    u.Bio.Contains(SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(SkillCategory))
            {
                query = query.Where(u => u.Skills.Any(s => s.Category == SkillCategory));
            }

            if (!string.IsNullOrWhiteSpace(SkillType))
            {
                bool isOffered = SkillType == "offered";
                query = query.Where(u => u.Skills.Any(s => s.IsOffered == isOffered));
            }

            // Calcola totale pagine
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // Carica gli utenti per la pagina corrente
            Users = await query
                .OrderByDescending(u => u.AverageRating)
                .ThenBy(u => u.User.UserName)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Limita il numero di competenze visualizzate per ciascun utente
            foreach (var user in Users)
            {
                // Mostra solo le prime 5 competenze di ciascun utente
                user.TopSkills = user.Skills
                    .OrderByDescending(s => s.IsOffered)
                    .ThenByDescending(s => s.ProficiencyLevel)
                    .Take(5)
                    .ToList();
            }
        }
    }
}