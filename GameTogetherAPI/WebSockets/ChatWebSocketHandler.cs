using GameTogetherAPI.Database;
using GameTogetherAPI.DTO;
using GameTogetherAPI.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace GameTogetherAPI.WebSockets
{
    public class ChatWebSocketHandler
    {
        private readonly WebSocketConnectionManager _manager;
        private readonly IConfiguration _configuration;

        // userId -> connectionId
        private readonly Dictionary<string, string> _userSocketMap = new();

        // connectionId -> chatId
        private readonly Dictionary<string, int> _connectionChatMap = new();

        public ChatWebSocketHandler(WebSocketConnectionManager manager, IConfiguration configuration)
        {
            _manager = manager;
            _configuration = configuration;
        }

        public async Task HandleSocketAsync(HttpContext context, WebSocket socket)
        {
            var token = context.Request.Query["token"].ToString();
            var chatIdStr = context.Request.Query["chatId"].ToString();

            if (!int.TryParse(chatIdStr, out int chatId))
            {
                context.Response.StatusCode = 400;
                return;
            }

            var principal = ValidateToken(token);
            if (principal == null)
            {
                Console.WriteLine("WebSocket token validation failed.");
                context.Response.StatusCode = 401;
                return;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Response.StatusCode = 403;
                return;
            }

            // Check user has access to this chat
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            var isInChat = await dbContext.UserChats.AnyAsync(uc => uc.UserId == userId && uc.ChatId == chatId);
            if (!isInChat)
            {
                context.Response.StatusCode = 403;
                return;
            }

            var connectionId = _manager.AddSocket(socket);
            _userSocketMap[userIdClaim] = connectionId;
            _connectionChatMap[connectionId] = chatId;

            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    // Optional: handle any incoming client messages here
                }
            }
            finally
            {
                await _manager.RemoveSocket(connectionId);
                _userSocketMap.Remove(userIdClaim);
                _connectionChatMap.Remove(connectionId);
            }
        }

        public async Task BroadcastMessageAsync(GetMessagesInChatResponseDTO dto, int chatId)
        {
            var json = JsonSerializer.Serialize(dto);
            var buffer = Encoding.UTF8.GetBytes(json);

            var openSocketsInSameChat = _manager.GetAll()
                .Where(pair =>
                    pair.Value.State == WebSocketState.Open &&
                    _connectionChatMap.TryGetValue(pair.Key, out var mappedChatId) &&
                    mappedChatId == chatId
                );

            var tasks = openSocketsInSameChat.Select(pair =>
                pair.Value.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                ));

            await Task.WhenAll(tasks);
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
