using System.Collections.Generic;

namespace GameTogetherAPI.Models.DTOs {
    /// <summary>
    /// Data Transfer Object for Game information.
    /// </summary>
    public class GameDTO {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public List<UserDTO> Users { get; set; } = new List<UserDTO>();
    }
}
