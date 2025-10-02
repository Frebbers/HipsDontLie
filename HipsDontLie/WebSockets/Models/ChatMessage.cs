namespace HipsDontLie.WebSockets.Models
{
    public class ChatMessage : IWebSocketMessage
    {
        public string Type => "message";
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ChatId { get; set; }
    }
}