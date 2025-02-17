using GameTogetherAPI.Models;
using GameTogetherAPI.Models.DTOs;

namespace GameTogetherAPI.Services {
    /// <summary>
    /// Service interface for handling user-related operations.
    /// </summary>
    public interface IUserService {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user object to create.</param>
        /// <returns>True if the user was created successfully; otherwise, false.</returns>
        Task<bool> CreateUserAsync(User user);

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of users.</returns>
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user object if found.</returns>
        Task<UserDTO> GetUserByIdAsync(string userId);
    }
}
