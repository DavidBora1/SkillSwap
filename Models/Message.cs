using System;

namespace SkillSwap.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        // Relazioni con chiavi esterne esplicite
        public int SenderId { get; set; }
        public UserProfile Sender { get; set; }

        public int RecipientId { get; set; }
        public UserProfile Recipient { get; set; }
    }
}