using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class SkillUpdateDto
    {
        public int Id { get; set; }

        public int UserProfileId { get; set; }

        [Required(ErrorMessage = "Il nome della competenza è obbligatorio")]
        [StringLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoria è obbligatoria")]
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Modificato: campo nullable per gestire competenze richieste
        public int? ProficiencyLevel { get; set; }

        public bool IsOffered { get; set; }
    }
}