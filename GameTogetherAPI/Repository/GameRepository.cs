using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Repository {
    public class GameRepository : IGameRepository {
        private readonly ApplicationDbContext _context;

        public GameRepository(ApplicationDbContext context) {
            _context = context;
        }

        /// <summary>
        /// Creates a new game and adds it to the database.
        /// </summary>
        /// <param name="game">The game object to add.</param>
        /// <returns>True if the game was successfully added; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the owner does not exist.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<bool> CreateGameAsync(Game game) {
            try {
                // Ensure the owner exists before adding the game
                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == game.OwnerId);
                if (owner == null)
                    throw new InvalidOperationException($"Cannot create game. Owner with ID '{game.OwnerId}' does not exist.");

                // Add the owner to the game
                game.UserIDs.Add(owner.Id);

                // Add the game to the owner's game list
                owner.GameIDs.Add(game.Id);

                _context.Games.Add(game);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (InvalidOperationException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while adding the game.", ex);
            }
        }

        /// <summary>
        /// Adds a user to a game by updating the game's player list.
        /// </summary>
        /// <param name="gameId">The ID of the game the user wants to join.</param>
        /// <param name="userId">The ID of the user joining the game.</param>
        /// <returns>True if the user was successfully added; otherwise, false.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the game or user is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user is already in the game.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<bool> JoinGameAsync(string gameId, string userId) {
            try {
                // Find the game
                var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

                if (game == null)
                    throw new KeyNotFoundException($"Game with ID '{gameId}' not found.");

                // Find the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID '{userId}' not found.");

                // Check if the user is already in the game
                if (game.UserIDs.Any(p => p == userId))
                    throw new InvalidOperationException("User is already in the game.");

                // Add the user to the game's player list
                game.UserIDs.Add(user.Id);

                // Add the game to the user's game list
                user.GameIDs.Add(game.Id);

                // Save changes
                return await _context.SaveChangesAsync() > 0;
            }
            catch (KeyNotFoundException) {
                throw;
            }
            catch (InvalidOperationException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while joining the game.", ex);
            }
        }

        /// <summary>
        /// Removes a user from a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user leaving the game.</param>
        /// <returns>True if the user was successfully removed; otherwise, false.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the game or user is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user is not in the game.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<bool> LeaveGameAsync(string gameId, string userId) {
            try {
                // Find the game
                var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

                if (game == null)
                    throw new KeyNotFoundException($"Game with ID '{gameId}' not found.");

                // Find the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID '{userId}' not found.");

                // Check if the user is part of the game
                if (!game.UserIDs.Any(p => p == userId))
                    throw new InvalidOperationException("User is not in the game.");

                // Remove the user from the game's player list
                game.UserIDs.Remove(user.Id);

                // Remove the game from the user's game list
                user.GameIDs.Remove(game.Id);

                // Save changes
                return await _context.SaveChangesAsync() > 0;
            }
            catch (KeyNotFoundException) {
                throw;
            }
            catch (InvalidOperationException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while leaving the game.", ex);
            }
        }

        /// <summary>
        /// Retrieves all games from the database.
        /// </summary>
        /// <returns>A list of all games.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while fetching games.</exception>
        public async Task<IEnumerable<Game>> GetAllGamesAsync() {
            try {
                return await _context.Games.ToListAsync();
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while retrieving games.", ex);
            }
        }

        /// <summary>
        /// Retrieves a game by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve.</param>
        /// <returns>The game object if found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no game with the given ID is found.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        public async Task<Game> GetGameByIdAsync(string gameId) {
            try {
                var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);

                if (game == null)
                    throw new KeyNotFoundException($"Game with ID '{gameId}' not found.");

                return game;
            }
            catch (KeyNotFoundException) {
                throw;
            }
            catch (Exception ex) {
                throw new Exception($"An error occurred while retrieving the game with ID '{gameId}'.", ex);
            }
        }
    }
}
