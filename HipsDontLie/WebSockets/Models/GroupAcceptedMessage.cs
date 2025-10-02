namespace HipsDontLie.WebSockets.Models
{
    public class GroupAcceptedMessage : IWebSocketMessage
    {
        public string Type => "group.accepted";
        public int GroupId {get; set;}
        public string GroupName {get; set;}
        public int UserId { get; set; }
        public int OwnerId { get; set; }
    }
}