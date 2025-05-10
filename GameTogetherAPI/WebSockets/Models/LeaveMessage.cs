namespace GameTogetherAPI.WebSockets.Models
{
    public class LeaveMessage : IWebSocketMessage
    {
        public string Type => "leave";
        public int ChatId { get; set; }
    }
}
