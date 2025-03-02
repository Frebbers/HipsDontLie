using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public bool IsVisible { get; set; }
        public string AgeRange { get; set; }
        public string Description { get; set; }

        public List<UserSession> Participants { get; set; } = new();

        public List<string> Tags { get; set; } = new();
    }
}
