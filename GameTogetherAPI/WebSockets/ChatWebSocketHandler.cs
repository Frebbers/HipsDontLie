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
                    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

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

                        IWebSocketMessage? parsed = type switch
                        {
                            "typing" => JsonSerializer.Deserialize<TypingMessage>(json),
                            _ => null
                        };

                        if (parsed is TypingMessage)
                        {
                            await BroadcastTypingAsync(userId, chatId);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to parse WebSocket message: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("WebSocket timed out");
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

        private async Task BroadcastTypingAsync(int userId, int chatId)
        {
            var senderConnId = _userSocketMap.TryGetValue(userId.ToString(), out var connId) ? connId : null;

            var typingPayload = new TypingMessage
            {
                ChatId = chatId,
                UserId = userId
            };

            await BroadcastToChatAsync(chatId, typingPayload, excludeConnectionId: senderConnId);
        }

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
                    _connectionChatMap.TryGetValue(pair.Key, out var mappedChatId) &&
                    mappedChatId == chatId &&
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
