namespace GameTogetherAPI.WebSockets.Models
{
    public class TypingMessage : IWebSocketMessage
{
    public string Type => "typing";
    public int ChatId { get; set; }
    public int UserId { get; set; }
}
}