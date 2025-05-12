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
    public class WebSocketEventHandler
    {
        private readonly WebSocketConnectionManager _manager;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<string, HashSet<int>> _connectionChatMap = new();

        private readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public WebSocketEventHandler(WebSocketConnectionManager manager, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _manager = manager;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleSocketAsync(HttpContext context, WebSocket socket)
        {
            var token = context.Request.Query["token"];
            var principal = ValidateToken(token!);

            if (principal == null)
            {
                Console.WriteLine("WebSocket token validation failed.");
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token", CancellationToken.None);
                return;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Missing user ID", CancellationToken.None);
                return;
            }

            var connectionId = _manager.AddSocket(socket);
            _manager.MapUserToConnection(userId, connectionId);

            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                        break;
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var doc = JsonDocument.Parse(json);
                        if (!doc.RootElement.TryGetProperty("type", out var typeProp)) continue;

                        var type = typeProp.GetString();
                        if (string.IsNullOrWhiteSpace(type)) continue;

                        //Here we parse what kind of event is that comes from parsing the Type property
                        IWebSocketMessage? parsed = type switch
                        {
                            "join" => JsonSerializer.Deserialize<JoinMessage>(json, options),
                            "leave" => JsonSerializer.Deserialize<LeaveMessage>(json, options),
                            "typing" or "stopTyping" => JsonSerializer.Deserialize<TypingMessage>(json, options),
                            "message" => JsonSerializer.Deserialize<ChatMessage>(json, options),
                            "pending.join.request" => JsonSerializer.Deserialize<PendingJoinRequestMessage>(json, options),
                            "group.accepted" => JsonSerializer.Deserialize<GroupAcceptedMessage>(json, options),
                            _ => null
                        };

                        //Here we handle the parsed events and do the communication
                        switch (parsed)
                        {
                            case JoinMessage joinMsg:
                                if (!_connectionChatMap.ContainsKey(connectionId))
                                {
                                    //HashSet solved the problem of lingering in a chat even tho you left the group
                                    //It gives the ability to have a connection joining several chatIds, but we need this for a reliable way to remove a chat.
                                    _connectionChatMap[connectionId] = new HashSet<int>(); 
                                }
                                _connectionChatMap[connectionId].Add(joinMsg.ChatId);
                                break;

                            case LeaveMessage leaveMsg:
                                if (_connectionChatMap.TryGetValue(connectionId, out var chats))
                                {
                                    chats.Remove(leaveMsg.ChatId);
                                    if (chats.Count == 0)
                                        _connectionChatMap.Remove(connectionId);
                                    Console.WriteLine($"Unmapped connection {connectionId} from chat {leaveMsg.ChatId}");
                                }
                                break;

                            case TypingMessage typingMsg:
                                if (_connectionChatMap.TryGetValue(connectionId, out var joinedChats) &&
                                    joinedChats.Contains(typingMsg.ChatId))
                                {
                                    if (type == "typing")
                                        await BroadcastTypingAsync(userId, typingMsg.ChatId);
                                    else
                                        await BroadcastStopTypingAsync(userId, typingMsg.ChatId);
                                }
                                break;

                            case ChatMessage message:
                                if (_connectionChatMap.TryGetValue(connectionId, out var chatSet) &&
                                    chatSet.Contains(message.ChatId))
                                {
                                    await BroadcastToChatAsync(message.ChatId, message);
                                }
                                break;

                            /*case GroupAcceptedMessage accepted:
                                await SendGroupAcceptedAsync(accepted.UserId, accepted.OwnerId, accepted.GroupId, accepted.GroupName);
                                break;*/
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to parse WebSocket message: {ex.Message}");
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocketException for {connectionId}: {ex.Message}");
            }
            finally
            {
                _connectionChatMap.Remove(connectionId);
                await _manager.RemoveSocket(connectionId);
            }
        }

        private async Task BroadcastTypingAsync(int userId, int chatId)
        {
            var username = await GetUsername(userId);
            var msg = new TypingMessage
            {
                ChatId = chatId,
                UserId = userId,
                Username = username
            };

            await BroadcastToChatAsync(chatId, msg, excludeUserId: userId);
        }

        private async Task BroadcastStopTypingAsync(int userId, int chatId)
        {
            var msg = new TypingMessage
            {
                Type = "stopTyping",
                ChatId = chatId,
                UserId = userId
            };

            await BroadcastToChatAsync(chatId, msg, excludeUserId: userId);
        }

        public async Task BroadcastMessageAsync(GetMessagesInChatResponseDTO dto, int chatId)
        {
            var msg = new ChatMessage
            {
                MessageId = dto.MessageId,
                SenderId = dto.SenderId,
                SenderName = dto.SenderName,
                Content = dto.Content,
                TimeStamp = dto.TimeStamp,
                ChatId = dto.ChatId
            };

            await BroadcastToChatAsync(chatId, msg);
        }

        public async Task SendGroupAcceptedAsync(int userId,int ownerId, int groupId, string groupName)
        {
            var payload = new GroupAcceptedMessage
            {
                GroupId = groupId,
                GroupName = groupName,
                UserId = userId,
                OwnerId = ownerId
            };

            await _manager.SendToUserAsync(userId, payload, options);
        }

        private async Task BroadcastToChatAsync(int chatId, object payload, int? excludeUserId = null)
        {
            var json = JsonSerializer.Serialize(payload, options);
            var buffer = Encoding.UTF8.GetBytes(json);

            var targets = _manager.GetAll()
                .Where(pair =>
                    pair.Value.State == WebSocketState.Open &&
                    _connectionChatMap.TryGetValue(pair.Key, out var chatSet) && chatSet.Contains(chatId) &&
                    (excludeUserId == null || _manager.GetConnectionIdForUser(excludeUserId.Value) != pair.Key));

            var tasks = targets.Select(async pair =>
            {
                try
                {
                    await pair.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebSocket send failed: {ex.Message}");
                    await _manager.RemoveSocket(pair.Key);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task SendPendingJoinRequestAsync(int ownerId, int groupId, int requesterId, string requesterName, string title)
        {
            var payload = new PendingJoinRequestMessage
            {
                GroupId = groupId,
                OwnerId = ownerId,
                RequestUserId = requesterId,
                RequesterName = requesterName,
                Title = title
            };

            await _manager.SendToUserAsync(ownerId, payload, options);
        }

        private async Task<string?> GetUsername(int userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await db.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUsername failed: {ex.Message}");
                return null;
            }
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
            var handler = new JwtSecurityTokenHandler();

            try
            {
                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true
                }, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
