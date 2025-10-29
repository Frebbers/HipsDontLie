using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HipsDontLie.Models;
using HipsDontLie.Services;

namespace HipsDontLie.Controllers {
    
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class which handles user authentication,
        /// including registration, login, and user account management.
        /// </summary>
        /// <param name="authService">The authentication service used to handle user authentication and management.</param>
        public AuthController(IAuthService authService, IConfiguration configuration) {
            _authService = authService;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user with the provided email and password and sends a verification email.
        /// </summary>
        /// <param name="model">The registration model containing the user's email and password.</param>
        /// <returns>
        /// Returns a 200 OK response if registration is successful and the verification email is sent.
        /// Returns a 400 Bad Request response if the email is already taken or if the verification email could not be sent.
        /// </returns>
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Username, email, and password are required.");


            var status = await _authService.RegisterUserAsync(model.Email, model.Username, model.Password);

            return status switch
            {
                AuthStatus.UserExists => BadRequest("Email already taken."),
                AuthStatus.WeakPassword => BadRequest("Password is too weak. It must be at least 6 characters long."),
                AuthStatus.UserCreated => await HandleEmailVerification(model.Email),
                AuthStatus.TestUserCreated => Ok("Test user created successfully."),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown registration error.")
            };
        }

        private async Task<IActionResult> HandleEmailVerification(string email)
        {
            var sent = await _authService.SendEmailVerificationAsync(email);
            if (!sent)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to send verification email.");
            return Ok("User registered successfully. Please check your email for a verification link.");
        }

        /// <summary>
        /// Verifies the user's email by validating the provided token.
        /// Redirects the user to the frontend with a success or failure status.
        /// </summary>
        /// <param name="token">The email verification JWT token.</param>
        /// <returns>Redirects to the frontend URL with a verification status in the query string.</returns>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] int userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token) || userId <= 0)
                return BadRequest("Invalid verification link.");

            bool success = await _authService.ConfirmEmailAsync(userId, token);

            string redirectUrl = _configuration["FRONTEND_BASE_URL"] ?? "https://localhost:7057";
            string status = success ? "success" : "failed";

            return Redirect($"{redirectUrl}/verify?status={(success ? "success" : "failed")}");
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="model">The login model containing the user's email and password.</param>
        /// <returns>
        /// Returns a 200 OK response with a JWT token if authentication is successful.
        /// Returns a 401 Unauthorized response if credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Email and password are required.");


            var token = await _authService.AuthenticateUserAsync(model.Email, model.Password);
            if (token == null)
                return Unauthorized("Invalid credentials or email not verified.");

            return Ok(new { Token = token });
        }

        /// <summary>
        /// Deletes the currently authenticated user's account.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response if the account is successfully deleted.
        /// Returns a 400 Bad Request response if the deletion fails.
        /// </returns>
        [HttpDelete("remove-user/")]
        [Authorize]
        public async Task<IActionResult> DeleteUser() {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("Invalid user ID.");

            bool success = await _authService.DeleteUserAsync(userId);
            if (!success)
                return BadRequest("Unable to delete user.");

            return Ok("User deleted successfully.");
        }

        /// <summary>
        /// Retrieves the currently authenticated user's email.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response with the user's email.
        /// </returns>
        [HttpGet("me")]
        [Authorize]
        //public async Task<IActionResult> GetCurrentUser([FromBody] string token)
        public IActionResult GetCurrentUser() {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(new { Id = id, Email = email });
        }

        /// <summary>
        /// Validates the current JWT token and returns 200 OK if valid.
        /// </summary>
        /// <returns>A 200 OK response if the token is valid, otherwise 401 Unauthorized.</returns>
        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken() {
            // If the request reaches here, the token is valid
            return Ok(new { message = "Token is valid." });
        }
    }
}