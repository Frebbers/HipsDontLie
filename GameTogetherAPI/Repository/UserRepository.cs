using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Repository {
    public class UserRepository : IUserRepository {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) {
            _context = context;
        }

        /// <summary>
        /// Adds a new user to the database if the email does not already exist.
        /// </summary>
        /// <param name="user">The user object to add.</param>
        /// <returns>True if the user was successfully added; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a user with the same email already exists.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<bool> CreateUserAsync(User user) {
            try {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                    throw new InvalidOperationException($"A user with email '{user.Email}' already exists.");

                _context.Users.Add(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (InvalidOperationException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while adding the user.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users from the database, including their associated games.
        /// </summary>
        /// <returns>A list of users with their games.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while fetching users.</exception>
        public async Task<IEnumerable<User>> GetAllUsersAsync() {
            try {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while retrieving users.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their ID, including their associated games.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user object if found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no user with the given ID is found.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<User> GetUserByIdAsync(string userId) {
            try {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    throw new KeyNotFoundException($"User with ID '{userId}' not found.");

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
