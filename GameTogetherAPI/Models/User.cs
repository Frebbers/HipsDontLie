using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? ProfilePicture { get; set; }
        public string? Description { get; set; }
        public string? Region { get; set; }

        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
