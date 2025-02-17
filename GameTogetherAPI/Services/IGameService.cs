using GameTogetherAPI.Models;
using GameTogetherAPI.Models.DTOs;

namespace GameTogetherAPI.Services {
    /// <summary>
    /// Service interface for handling game-related operations.
    /// </summary>
    public interface IGameService {
        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="game">The game object to create.</param>
        /// <returns>True if the game was created successfully; otherwise, false.</returns>
        Task<bool> CreateGameAsync(Game game);

        /// <summary>
        /// Adds a user to a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user joining the game.</param>
        /// <returns>True if the user was successfully added; otherwise, false.</returns>
        Task<bool> JoinGameAsync(string gameId, string userId);

        /// <summary>
        /// Allows a user to leave a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user leaving the game.</param>
        /// <returns>True if the user successfully left the game; otherwise, false.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the game or user is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user is not in the game.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
        Task<bool> LeaveGameAsync(string gameId, string userId);

        /// <summary>
        /// Retrieves all games.
        /// </summary>
        /// <returns>A list of all games.</returns>
        Task<IEnumerable<GameDTO>> GetAllGamesAsync();

        /// <summary>
        /// Retrieves a game by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve.</param>
        /// <returns>The game object if found.</returns>
        Task<GameDTO> GetGameByIdAsync(string gameId);

    }
}
