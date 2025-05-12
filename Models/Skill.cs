namespace SkillSwap.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int ProficiencyLevel { get; set; } // 1-5
        public bool IsOffered { get; set; } // true = offerto, false = richiesto

        // Relazioni
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
    }
}