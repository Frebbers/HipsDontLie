using GameTogetherAPI.Models;
using GameTogetherAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameTogetherAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase {
        private readonly IUserService _userService;

        public UserController(IUserService userService) {
            _userService = userService;
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user object to create.</param>
        /// <returns>An HTTP response indicating success or failure.</returns>
        /// <response code="200">User created successfully.</response>
        /// <response code="400">Invalid request or user creation failed.</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] User user) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                var result = await _userService.CreateUserAsync(user);
                if (!result) return BadRequest("Could not create user.");

                return Ok(new { message = "User created successfully" });
            }
            catch (InvalidOperationException ex) {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An internal server error occurred.", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <response code="200">Users retrieved successfully.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers() {
            try {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while retrieving users.", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user object if found.</returns>
        /// <response code="200">User found.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserById(string userId) {
            try {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null) return NotFound(new { error = "User not found." });

                return Ok(user);
            }
            catch (KeyNotFoundException ex) {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while retrieving the user.", details = ex.Message });
            }
        }
    }
}
