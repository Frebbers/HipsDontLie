using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
        }

        public Task<ProfileResponseDTO> GetProfileAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddOrUpdateProfileAsync(int userId, ProfileCreateDTO profileDto)
        {
            var profile = new Profile
            {
                Id = userId,
                Name = profileDto.Name,
                ProfilePicture = profileDto.ProfilePicture,
                Description = profileDto.Description,
                Region = profileDto.Region,
                Tags = profileDto.Tags
            };

            bool isSuccess = await _userRepository.AddOrUpdateProfileAsync(profile);

            if (!isSuccess)
            {
                return false;
            }
            return true;
        }
    }
}
