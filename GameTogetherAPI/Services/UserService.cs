using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services
{
    /// <summary>
    /// Provides user management services, including profile retrieval and updates.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">The repository for user-related database operations.</param>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Adds or updates the profile of a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="profileDto">The profile data to be added or updated.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the operation is successful.</returns>
        public async Task<bool> AddOrUpdateProfileAsync(int userId, UpdateProfileRequestDTO profileDto)
        {
            var profile = new Profile
            {
                Id = userId,
                BirthDate = profileDto.BirthDate,
                ProfilePicture = profileDto.ProfilePicture,
                Description = profileDto.Description,
                Region = profileDto.Region,
            };

            bool isSuccess = await _userRepository.AddOrUpdateProfileAsync(profile);

            if (!isSuccess)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves the profile information of the calling user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, returning the user's profile details.</returns>
        public async Task<GetProfileResponseDTO> GetProfileAsync(int userId)
        {
            var profile = await _userRepository.GetProfileAsync(userId);
            return new GetProfileResponseDTO
            {
                Username = profile.User.Username,
                BirthDate = profile.BirthDate,
                Description = profile.Description,
                Region = profile.Region,
                ProfilePicture = profile.ProfilePicture
            };
        }

        /// <summary>
        /// Retrieves the profile information of a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, returning the user's profile details.</returns>
        public async Task<GetProfileResponseDTO> GetProfileByIdAsync(int userId)
        {
            var profile = await _userRepository.GetProfileAsync(userId);
            return new GetProfileResponseDTO
            {
                BirthDate = profile.BirthDate,
                Description = profile.Description,
                Region = profile.Region,
                ProfilePicture = profile.ProfilePicture
            };
        }
    }
}
