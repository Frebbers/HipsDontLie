using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using GameTogetherAPI.Models;
using GameTogetherAPI.Services;
using System.Security.Claims;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        bool success = await _authService.RegisterUserAsync(model.Email, model.Password);
        if (!success)
            return BadRequest("Email already taken.");

        return Ok("User registered successfully!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var token = await _authService.AuthenticateUserAsync(model.Email, model.Password);
        if (token == null)
            return Unauthorized("Invalid credentials.");

        return Ok(new { Token = token });
    }

    [HttpDelete("remove-user/")]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        bool success = await _authService.DeleteUserAsync(userId);

        if (!success)
            return BadRequest( new { message = "Unable to delete user."});

        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { Email = userEmail });
    }
}
