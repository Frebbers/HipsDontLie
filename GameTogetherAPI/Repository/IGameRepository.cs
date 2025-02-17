using GameTogetherAPI.Models;

namespace GameTogetherAPI.Repository {
    /// <summary>
    /// Repository interface for managing games in the database.
    /// </summary>
    public interface IGameRepository {
        /// <summary>
        /// Adds a new game to the database.
        /// </summary>
        /// <param name="game">The game object to add.</param>
        /// <returns>True if the game was successfully added; otherwise, false.</returns>
        Task<bool> CreateGameAsync(Game game);

        /// <summary>
        /// Adds a user to a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user joining the game.</param>
        /// <returns>True if the user was successfully added; otherwise, false.</returns>
        Task<bool> JoinGameAsync(string gameId, string userId);

        /// <summary>
        /// Removes a user from a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user leaving the game.</param>
        /// <returns>True if the user was successfully removed; otherwise, false.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the game or user is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user is not in the game.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        Task<bool> LeaveGameAsync(string gameId, string userId);

        /// <summary>
        /// Retrieves all games from the database.
        /// </summary>
        /// <returns>A list of all games.</returns>
        Task<IEnumerable<Game>> GetAllGamesAsync();

        /// <summary>
        /// Retrieves a game by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve.</param>
        /// <returns>The game object if found.</returns>
        Task<Game> GetGameByIdAsync(string gameId);
    }
}
