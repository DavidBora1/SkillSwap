using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Pages.Messages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class MessageViewModel
        {
            public int Id { get; set; }
            public string SenderName { get; set; }
            public string Content { get; set; }
            public DateTime SentAt { get; set; }
            public bool IsRead { get; set; }
        }

        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return;
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (userProfile == null)
            {
                return;
            }

            // Recupera i messaggi ricevuti
            var messages = await _context.Messages
                .Where(m => m.RecipientId == userProfile.Id)
                .Include(m => m.Sender)
                .ThenInclude(s => s.User)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            Messages = messages.Select(msg => new MessageViewModel
            {
                Id = msg.Id,
                SenderName = msg.Sender.User.Email,
                Content = msg.Content,
                SentAt = msg.SentAt,
                IsRead = msg.IsRead
            }).ToList();
        }
    }
}