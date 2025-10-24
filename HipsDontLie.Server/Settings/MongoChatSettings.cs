namespace HipsDontLie.Server.Settings
{
    public class MongoChatSettings
    {
        public string ConnectionString { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string MessagesCollectionName { get; set; } = "messages";
    }
}