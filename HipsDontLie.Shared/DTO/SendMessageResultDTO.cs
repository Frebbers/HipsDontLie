namespace HipsDontLie.Shared.DTO
{
    public class SendMessageResultDTO
    {
        public int ChatId { get; set; }
        public GetMessagesInChatResponseDTO Message { get; set; } = null!;
    }
}

