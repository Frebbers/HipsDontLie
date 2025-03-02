using GameTogetherAPI.DTO;
using GameTogetherAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameTogetherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

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

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDTO profile)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                bool success = await _userService.AddOrUpdateProfileAsync(userId, profile);

                if (!success)
                    return BadRequest(new { message = "Profile creation failed due to a bad request" });

                return Ok(new { message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }
}
