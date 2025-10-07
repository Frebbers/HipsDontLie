using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HipsDontLie.WebSockets
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly ConcurrentDictionary<int, string> _userConnections = new();

        public string AddSocket(WebSocket socket)
        {
            var connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, socket);
            Console.WriteLine($"Added connection: {connectionId}");
            return connectionId;
        }
        public void MapUserToConnection(int userId, string connectionId)
        {
            _userConnections[userId] = connectionId;
        }

        public string? GetConnectionIdForUser(int userId)
        {
            return _userConnections.TryGetValue(userId, out var connId) ? connId : null;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll() => _sockets;

        public WebSocket? GetSocketById(string id) =>
            _sockets.TryGetValue(id, out var socket) ? socket : null;

        public async Task RemoveSocket(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
                _userConnections
                .Where(pair => pair.Value == id)
                .Select(pair => pair.Key)
                .ToList()
                .ForEach(userId => _userConnections.TryRemove(userId, out _));

                socket.Dispose();
                Console.WriteLine($"Removed connection: {id}");
            }
        }

        public async Task SendToUserAsync(int userId, object payload, JsonSerializerOptions? options = null)
        {
            var connectionId = GetConnectionIdForUser(userId);
            if (connectionId == null) return;

            var socket = GetSocketById(connectionId);
            if (socket?.State != WebSocketState.Open) return;

            var json = JsonSerializer.Serialize(payload, options ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var buffer = Encoding.UTF8.GetBytes(json);

            await socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        public void CleanupClosedSockets()
        {
            int removedCount = 0;
            foreach (var (id, socket) in _sockets)
            {
                if (socket.State != WebSocketState.Open)
                {
                    _sockets.TryRemove(id, out _);
                    socket.Dispose();
                    removedCount++;
                    Console.WriteLine($"Cleaned up useless connection: {id}");
                }
            }

            if (removedCount > 0)
            {
                Console.WriteLine($"Cleanup removed {removedCount} useless connections");
            }
        }
    }
}
