using GameTogetherAPI.Models;

namespace GameTogetherAPI.Repository {
    /// <summary>
    /// Repository interface for managing users in the database.
    /// </summary>
    public interface IUserRepository {
        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        /// <param name="user">The user object to add.</param>
        /// <returns>True if the user was successfully added; otherwise, false.</returns>
        Task<bool> CreateUserAsync(User user);

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A list of all users.</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user object if found.</returns>
        Task<User> GetUserByIdAsync(string userId);

    }
}
