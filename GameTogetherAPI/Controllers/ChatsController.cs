using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace GameTogetherAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetUserInboxAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var inbox = await _chatService.GetUserInboxAsync(userId);

            if (inbox == null || inbox.Count == 0)
                return NotFound(new { message = "Inbox is empty!" });

            return Ok(inbox);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessagesByChatIdAsync(int chatId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var messages = await _chatService.GetMessagesByChatIdAsync(chatId, userId);
            if (messages == null || messages.Count == 0)
                return NotFound(new { message = "No messages in this chat!" });

            return Ok(messages);
        }


        [HttpPost("session/{sessionId}/send")]
        public async Task<IActionResult> SendMessageToSessionAsync(int sessionId,[FromBody]SendMessageRequestDTO messageDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _chatService.SendMessageToSessionAsync(sessionId, userId, messageDto);
            if (!success)
                return BadRequest(new { message = "Failed to send message to session." });

            return Created(string.Empty, new { message = "Message sent successfully!" });

        }

        [HttpPost("user/{userId}/send")]
        public async Task<IActionResult> SendMessageToUserAsync(int userId, [FromBody]SendMessageRequestDTO messageDto)
        {
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _chatService.SendMessageToUserAsync(senderId, userId, messageDto);
            if (!success)
                return BadRequest(new { message = "Failed to send message to user/chat." });

            return Created(string.Empty, new { message = "Message sent successfully!" });
        }


    }
}
