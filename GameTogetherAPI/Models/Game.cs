using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.Models
{
    public class Game
    {
        // Foreign key
        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
