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
            if (model == null || model.Username == null || model.Username == null || model.Password == null)
            {
                return BadRequest("Username, email and password are required.");
            }
            AuthStatus status = await _authService.RegisterUserAsync(model.Email, model.Username, model.Password);
            if (status == AuthStatus.UserExists)
            {
                return BadRequest("Email already taken.");
            }

            if(status == AuthStatus.WeakPassword)
                return BadRequest("Password must be atleast 8 characters long, contain at least one uppercase letter, one lowercase letter, one number.");

            if (status == AuthStatus.UserCreated)
            {
                bool emailSent = await _authService.SendEmailVerificationAsync(model.Email);


                if (!emailSent) //email sending fails
                {
                    try //Clean up the user to make registration transactional
                    {
                        
                        _authService.DeleteUserAsync(0, model.Email);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error parsing user ID: {ex.Message}");
                        // We should consider handling cases where this parsing fails. Could the user still exist?
                    }
                    return StatusCode(StatusCodes.Status500InternalServerError, "Could not send verification email.");
                }
            }

            return Ok("User registered successfully!");
        }

        /// <summary>
        /// Verifies the user's email by validating the provided token.
        /// Redirects the user to the frontend with a success or failure status.
        /// </summary>
        /// <param name="token">The email verification JWT token.</param>
        /// <returns>Redirects to the frontend URL with a verification status in the query string.</returns>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token) {
            bool success = await _authService.ConfirmEmailAsync(token);

            var redirectUrl = _configuration["FRONTEND_BASE_URL"];

            if (!success) {
                return Redirect($"{redirectUrl}/?verification=failed");
            }

            return Redirect($"{redirectUrl}/?verification=success");
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
        public async Task<IActionResult> Login([FromBody] LoginModel model) {
            var token = await _authService.AuthenticateUserAsync(model.Email, model.Password);
            if (token == null)
                return Unauthorized("Invalid credentials or your user has not been verified.");

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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _authService.DeleteUserAsync(userId);

            if (!success)
                return BadRequest(new { message = "Unable to delete user." });

            return Ok();
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
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok(new { Email = userEmail });
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