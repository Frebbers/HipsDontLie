using GameTogetherAPI.Models;

namespace GameTogetherAPI.Services
{
    public interface IUserService
    {
        Task<ProfileResponseDTO> GetProfileAsync(int userId);
        Task<bool> AddOrUpdateProfileAsync(int userId, ProfileCreateDTO profileDto);
    }
}