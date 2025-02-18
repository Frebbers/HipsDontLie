using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTogetherAPI.Models {
    public class User {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? ProfilePicture { get; set; }
        public string? Description { get; set; }
        public string? Region { get; set; }

        public List<string> GameIDs { get; set; } = new List<string>();

    }
}
