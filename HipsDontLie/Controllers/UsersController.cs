using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HipsDontLie.DTO;
using HipsDontLie.Models;
using HipsDontLie.Services;

namespace HipsDontLie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Handles user-related operations, including retrieving and updating user profiles.
    /// </summary>
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">The user service responsible for handling user-related operations.</param>

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves the profile of the authenticated user.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response with the user's profile.  
        /// Returns a 404 Not Found response if the user does not exist.
        /// </returns>

        [HttpGet("get-profile")]
        [Authorize]
        public async Task<IActionResult> GetProfileAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var profile = await _userService.GetProfileAsync(userId);

            if (profile == null)
                return NotFound(new { message = "User not found" });

            return Ok(profile);
        }

        /// <summary>
        /// Retrieves the profile of the authenticated user by id.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response with the user's profile.  
        /// Returns a 400 Bad request response if the user does not exist.
        /// </returns>
        [HttpGet("profile/{id}")]
        [Authorize]
        public async Task<IActionResult> GetProfileByIdAsync(int id)
        {

            var profile = await _userService.GetProfileByIdAsync(id);

            if (profile == null)
                return BadRequest(new { message = "Profile not found" });

            return Ok(profile);
        }

        /// <summary>
        /// Updates or creates the profile of the authenticated user.
        /// </summary>
        /// <param name="profile">The profile data to be updated.</param>
        /// <returns>
        /// Returns a 200 OK response if the profile is successfully updated.  
        /// Returns a 400 Bad Request response if the update fails.  
        /// Returns a 500 Internal Server Error response if an unexpected error occurs.
        /// </returns>

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDTO profile)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                UpdateProfileStatus status = await _userService.AddOrUpdateProfileAsync(userId, profile);

                if (status != UpdateProfileStatus.Success) {
                    return BadRequest(new { message = "Profile creation failed. Detected cause: " + status });
                }
                return Ok(new { message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        
        /// <summary>
        /// Retrieves a user's ID by their username.
        /// </summary>
        /// <param name="email">The username of the user.</param>
        /// <returns>
        /// Returns a 200 OK response with the user's ID.
        /// Returns a 404 Not Found response if the user does not exist.
        /// </returns>
        [HttpGet("profile/e-mail/{username}")]
        [Authorize]
        public async Task<IActionResult> GetUserIdByEmail(string username)
        {
            var userId = await _userService.GetUserIdByEmailAsync(username);
            
            if (userId == null)
                return BadRequest(new { message = "User not found" });
            
            return Ok(new { userId });
        }

    }
}
