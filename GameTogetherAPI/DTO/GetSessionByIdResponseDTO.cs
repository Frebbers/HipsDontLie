using GameTogetherAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.DTO
{
    public class GetSessionByIdResponseDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int OwnerId { get; set; }

        public bool IsVisible { get; set; }
        public string AgeRange { get; set; }
        public string Description { get; set; }

        public List<ParticipantDTO> Participants { get; set; } = new();

        public List<string> Tags { get; set; } = new();
    }
}
