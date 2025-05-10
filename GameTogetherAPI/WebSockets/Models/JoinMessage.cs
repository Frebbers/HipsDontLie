namespace GameTogetherAPI.WebSockets.Models
{
    public class JoinMessage : IWebSocketMessage
    {
        public string Type => "join";
        public int ChatId { get; set; }
    }
}