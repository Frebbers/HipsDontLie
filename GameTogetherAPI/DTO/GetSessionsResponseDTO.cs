using GameTogetherAPI.Models;

namespace GameTogetherAPI.DTO
{
    public class GetSessionsResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int OwnerId { get; set; }
        public bool IsVisible { get; set; }
        public string AgeRange { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }

        public List<ParticipantDTO> Participants { get; set; } = new();
    }
    public class ParticipantDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
