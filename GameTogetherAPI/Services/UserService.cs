using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> AddOrUpdateProfileAsync(int userId, UpdateProfileRequestDTO profileDto)
        {
            var profile = new Profile
            {
                Id = userId,
                Name = profileDto.Name,
                Age = profileDto.Age,
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

        public async Task<GetProfileResponseDTO> GetProfileAsync(int userId)
        {
            var profile = await _userRepository.GetProfileAsync(userId);
            return new GetProfileResponseDTO
            {
                Age = profile.Age,
                Description = profile.Description,
                Region = profile.Region,
                Tags = profile.Tags,
                Name = profile.Name,
                ProfilePicture = profile.ProfilePicture
            };
        }
    }
}
