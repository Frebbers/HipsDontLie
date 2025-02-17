using System.Collections.Generic;

namespace GameTogetherAPI.Models.DTOs {
    /// <summary>
    /// Data Transfer Object for User information.
    /// </summary>
    public class UserDTO {
        public string Id { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Description { get; set; }
        public string Region { get; set; }
        public List<GameDTO> Games { get; set; } = new List<GameDTO>();
    }
}
