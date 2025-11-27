namespace HipsDontLie.WebSockets.Models 
{
    public class PendingJoinRequestMessage : IWebSocketMessage
    {
        public string Type => "pending.join.request";
        public int GroupId { get; set; }
        public int RequestUserId { get; set; }
        public int OwnerId { get; set; }
        public string Title {get; set;}
        public string RequesterName { get; set; }
    }
}