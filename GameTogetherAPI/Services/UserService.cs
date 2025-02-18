using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services {
    public class UserService : IUserService {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user object to add.</param>
        /// <returns>True if the user was successfully added; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a user with the same email already exists.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<bool> CreateUserAsync(User user) {
            try {
                return await _userRepository.CreateUserAsync(user);
            }
            catch (InvalidOperationException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while creating the user.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of users with limited information.</returns>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<IEnumerable<User>> GetAllUsersAsync() {
            try {
                return await _userRepository.GetAllUsersAsync();
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while retrieving users.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user DTO if found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<User> GetUserByIdAsync(string userId) {
            try {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null) throw new KeyNotFoundException($"User with ID '{userId}' not found.");

                return user;
            }
            catch (KeyNotFoundException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception($"An error occurred while retrieving the user with ID '{userId}'.", ex);
            }
        }
    }
}
