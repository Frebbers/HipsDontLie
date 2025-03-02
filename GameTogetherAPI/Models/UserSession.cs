namespace GameTogetherAPI.Models
{
    public class UserSession
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }
    }
}
