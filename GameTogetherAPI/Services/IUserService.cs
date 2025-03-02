using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;

namespace GameTogetherAPI.Services
{
    public interface IUserService
    {
        Task<GetProfileResponseDTO> GetProfileAsync(int userId);
        Task<bool> AddOrUpdateProfileAsync(int userId, UpdateProfileRequestDTO profileDto);
    }
}