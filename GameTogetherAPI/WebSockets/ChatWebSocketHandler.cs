using GameTogetherAPI.Database;
using GameTogetherAPI.DTO;
using GameTogetherAPI.WebSockets.Models;
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
        private readonly IServiceProvider _serviceProvider;

        // userId -> connectionId
        private readonly Dictionary<string, string> _userSocketMap = new();

        // connectionId -> chatId
        private readonly Dictionary<string, int> _connectionChatMap = new();

        public ChatWebSocketHandler(WebSocketConnectionManager manager, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _manager = manager;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }
    public async Task HandleSocketAsync(HttpContext context, WebSocket socket)
    {
        var token = context.Request.Query["token"].ToString();
        var principal = ValidateToken(token);

        if (principal == null)
        {
            Console.WriteLine("WebSocket token validation failed.");
            await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token", CancellationToken.None);
            return;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Missing user ID", CancellationToken.None);
            return;
        }

        var connectionId = _manager.AddSocket(socket);
        _userSocketMap[userIdClaim] = connectionId;

        var buffer = new byte[1024 * 4];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("type", out var typeProp))
                        continue;

                    var type = typeProp.GetString();
                    if (string.IsNullOrWhiteSpace(type))
                        continue;

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    IWebSocketMessage? parsed = type switch
                    {
                        "typing" or "stopTyping" => JsonSerializer.Deserialize<TypingMessage>(json,options),
                        "message" => JsonSerializer.Deserialize<ChatMessage>(json,options),
                        // add other message types here
                        _ => null
                    };

                    if (parsed is TypingMessage typingMsg)
                    {
                        if (type == "typing")
                        {
                            await BroadcastTypingAsync(userId, typingMsg.ChatId);
                        }
                        else if (type == "stopTyping")
                        {
                            await BroadcastStopTypingAsync(userId, typingMsg.ChatId);
                        }
                    }

                    // Handle other types here

                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to parse WebSocket message: {ex.Message}");
                }
            }
        }
        finally
        {
            await _manager.RemoveSocket(connectionId);
            _userSocketMap.Remove(userIdClaim);
        }
    }

        //Broadcast for messages to a chat in a group
        public async Task BroadcastMessageAsync(GetMessagesInChatResponseDTO dto, int chatId)
        {
            var messagePayload = new ChatMessage
            {
                MessageId = dto.MessageId,
                SenderId = dto.SenderId,
                SenderName = dto.SenderName,
                Content = dto.Content,
                TimeStamp = dto.TimeStamp,
                ChatId = dto.ChatId
            };

            await BroadcastToChatAsync(chatId, messagePayload);
        }

        //Broadcast for a user that is currently typing in a chat
        private async Task BroadcastTypingAsync(int userId, int chatId)
        {
            var senderConnId = _userSocketMap.TryGetValue(userId.ToString(), out var connId) ? connId : null;

            var username = await GetUsername(userId);

            var typingPayload = new TypingMessage
            {
                ChatId = chatId,
                UserId = userId,
                Username = username
            };

            await BroadcastToChatAsync(chatId, typingPayload, excludeConnectionId: senderConnId);
        }

         //Broadcast for a user that stopped typing in a chat
        private async Task BroadcastStopTypingAsync(int userId, int chatId)
        {
            var senderConnId = _userSocketMap.TryGetValue(userId.ToString(), out var connId) ? connId : null;

            var typingPayload = new TypingMessage
            {
                Type = "stopTyping",
                ChatId = chatId,
                UserId = userId
            };

            await BroadcastToChatAsync(chatId, typingPayload, excludeConnectionId: senderConnId);
        }

        //Handles the broadcasts and sends them along to the connections
        private async Task BroadcastToChatAsync(int chatId, object payload, string? excludeConnectionId = null)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var buffer = Encoding.UTF8.GetBytes(json);

            var recipients = _manager.GetAll()
                .Where(pair =>
                    pair.Value.State == WebSocketState.Open &&
                    (excludeConnectionId == null || pair.Key != excludeConnectionId)
            );

            var tasks = recipients.Select(pair =>
                pair.Value.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                ));

            await Task.WhenAll(tasks);
        }

        private async Task<string?> GetUsername(int userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                return await dbContext.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.Username)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get username: {ex.Message}");
                return null;
            }
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
