namespace HipsDontLie.Shared.DTO
{
    public class GetUserInboxResponseDTO
    {
        public int ChatId { get; set; }
        public int? SessionId { get; set; }
        public string? SessionTitle { get; set; }

        public List<ChatParticipantDTO> Participants { get; set; } = new();

    }
}
