namespace HipsDontLie.WebSockets.Models
{
    public class TypingMessage : IWebSocketMessage
    {
        public string Type { get; set; } = "typing";
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
    }
}