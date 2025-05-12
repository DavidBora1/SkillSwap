using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class UserProfileUpdateDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime JoinDate { get; set; }

        public string Bio { get; set; } = string.Empty;

        // Modifica: consenti valori nulli o vuoti per CurrentImageUrl
        public string? CurrentImageUrl { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public bool RemoveExistingImage { get; set; }
    }
}