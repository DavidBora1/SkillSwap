using System;
using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class Exchange
    {
        public int Id { get; set; }

        public int RequestorId { get; set; }
        public UserProfile Requestor { get; set; }

        public int ProviderId { get; set; }
        public UserProfile Provider { get; set; }

        public int RequestedSkillId { get; set; }
        public Skill RequestedSkill { get; set; }

        public int OfferedSkillId { get; set; }
        public Skill OfferedSkill { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

        // Valutazioni
        public int? RequestorRating { get; set; }
        public string RequestorFeedback { get; set; } = string.Empty;

        public int? ProviderRating { get; set; }
        public string ProviderFeedback { get; set; } = string.Empty;
    }

    public enum ExchangeStatus
    {
        Pending,    // In attesa di accettazione
        Accepted,   // Accettato ma non completato
        Completed,  // Completato
        Rejected,   // Rifiutato
        Canceled    // Annullato
    }
}