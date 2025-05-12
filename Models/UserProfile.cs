using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSwap.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public string Bio { get; set; } = ""; // Imposta un valore predefinito vuoto
        public string ProfileImageUrl { get; set; } = ""; // Anche questo potrebbe causare problemi
        public DateTime JoinDate { get; set; } = DateTime.Now;

        // Relazioni
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        // Nuovi campi per il sistema di reputazione
        public double AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public int ExchangesCompleted { get; set; } = 0;

        // Calcolo automatico della reputazione
        public int ReputationScore =>
            (int)(AverageRating * 10 * Math.Log10(TotalRatings + 1) * Math.Log10(ExchangesCompleted + 1));

        // Livello di reputazione
        public string ReputationLevel => ReputationScore switch
        {
            < 10 => "Principiante",
            < 50 => "Esperto in formazione",
            < 100 => "Esperto",
            < 200 => "Esperto rinomato",
            _ => "Maestro"
        };

        [NotMapped]
        public List<Skill> TopSkills { get; set; } = new List<Skill>();
    }
}