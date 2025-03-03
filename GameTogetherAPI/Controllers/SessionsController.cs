using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameTogetherAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        public SessionsController(ISessionService sessionservice)
        {
            _sessionService = sessionservice; 
        }

        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequestDTO sessionDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _sessionService.CreateSessionAsync(userId, sessionDto);

            if (!success)
                return BadRequest(new { message = "Failed to create session." });

            return Created(string.Empty, new { message = "Session created successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionsAsync()
        {
            var sessions = await _sessionService.GetSessionsAsync();

            if (sessions == null)
                return NotFound(new { message = "Sessions not found" });

            return Ok(sessions);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMySessionsAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var sessions = await _sessionService.GetSessionsAsync(userId);

            if (sessions == null)
                return NotFound(new { message = "Sessions not found" });

            return Ok(sessions);
        }

        [HttpPost("{sessionId}/join")]
        public async Task<IActionResult> JoinSession(int sessionId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _sessionService.JoinSessionAsync(userId, sessionId);

            if (!success)
                return BadRequest(new { message = "Failed to join session. Either it does not exist or you're already a participant." });

            return Ok(new { message = "Successfully joined the session!" });
        }

        [HttpDelete("{sessionId}/leave")]
        public async Task<IActionResult> LeaveSession(int sessionId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _sessionService.LeaveSessionAsync(userId, sessionId);

            if (!success)
                return BadRequest(new { message = "Failed to leave session. Either it does not exist or you've already left." });

            return Ok(new { message = "Successfully left the session!" });
        }

    }
}
