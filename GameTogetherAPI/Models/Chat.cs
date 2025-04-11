using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.Models
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public List<Message> Messages { get; set; }
        public List<UserChat> UserChats { get; set; }
    }
}