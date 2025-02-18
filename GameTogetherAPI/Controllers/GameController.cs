using GameTogetherAPI.Models;
using GameTogetherAPI.Models.DTOs;
using GameTogetherAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameTogetherAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;

        public GameController(IGameService gameService, IUserService userService) {
            _gameService = gameService;
            _userService = userService;
        }

        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="game">The game object to create.</param>
        /// <returns>An HTTP response indicating success or failure.</returns>
        /// <response code="200">Game created successfully.</response>
        /// <response code="400">Invalid request or owner does not exist.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] Game game) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                // Check if the owner exists before creating the game
                var ownerExists = await _userService.GetUserByIdAsync(game.OwnerId);
                if (ownerExists == null)
                    return BadRequest(new { error = "Owner does not exist." });
            
                var createResult = await _gameService.CreateGameAsync(game);
                if (!createResult) return BadRequest(new { error = "Could not create game." });

                var joinResult = await _gameService.JoinGameAsync(game.Id, game.OwnerId);
                if (!joinResult)
                    return BadRequest(new { error = "Owner could not join  game." });

                return Ok(new { message = "Game created successfully" });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An internal server error occurred.", details = ex.Message });
            }
        }

        /// <summary>
        /// Allows a user to join a game by adding them to the game's player list.
        /// </summary>
        /// <param name="gameId">The ID of the game the user wants to join.</param>
        /// <param name="userId">The ID of the user joining the game.</param>
        /// <returns>
        /// - 200 OK: If the user successfully joins the game.  
        /// - 400 Bad Request: If the user is already in the game or the request fails.  
        /// - 404 Not Found: If the game or user does not exist.  
        /// - 500 Internal Server Error: If an unexpected error occurs.
        /// </returns>
        /// <response code="200">User successfully joined the game.</response>
        /// <response code="400">User is already in the game or an invalid request.</response>
        /// <response code="404">Game or user not found.</response>
        /// <response code="500">An unexpected error occurred.</response>
        [HttpPost("{gameId}/join/{userId}")]
        public async Task<IActionResult> JoinGame(string gameId, string userId) {
            try {
                var result = await _gameService.JoinGameAsync(gameId, userId);
                if (!result)
                    return BadRequest(new { error = "Could not join game." });

                return Ok(new { message = "User successfully joined the game." });
            }
            catch (KeyNotFoundException ex) {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while joining the game.", details = ex.Message });
            }
        }

        [HttpPost("{gameId}/leave/{userId}")]
        /// <summary>
        /// Allows a user to leave a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user leaving the game.</param>
        /// <returns>A success message if the user successfully left the game.</returns>
        /// <response code="200">User successfully left the game.</response>
        /// <response code="400">If the user is not in the game.</response>
        /// <response code="404">If the game or user does not exist.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        public async Task<IActionResult> LeaveGame(string gameId, string userId) {
            try {
                var result = await _gameService.LeaveGameAsync(gameId, userId);
                if (!result)
                    return BadRequest(new { error = "Could not leave game." });

                return Ok(new { message = "User successfully left the game." });
            }
            catch (KeyNotFoundException ex) {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while leaving the game.", details = ex.Message });
            }
        }

        [HttpGet("all")]
        /// <summary>
        /// Retrieves all games.
        /// </summary>
        /// <returns>A list of game DTOs.</returns>
        /// <response code="200">Returns the list of games.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetAllGames() {
            try {
                var games = await _gameService.GetAllGamesAsync();
                return Ok(games);
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while retrieving games.", details = ex.Message });
            }
        }

        [HttpGet("{gameId}")]
        /// <summary>
        /// Retrieves a game by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <returns>The game DTO if found.</returns>
        /// <response code="200">Returns the game.</response>
        /// <response code="404">If the game is not found.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        public async Task<ActionResult<GameDTO>> GetGameById(string gameId) {
            try {
                var game = await _gameService.GetGameByIdAsync(gameId);
                if (game == null) return NotFound(new { error = "Game not found." });

                return Ok(game);
            }
            catch (KeyNotFoundException ex) {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while retrieving the game.", details = ex.Message });
            }
        }
    }
}
