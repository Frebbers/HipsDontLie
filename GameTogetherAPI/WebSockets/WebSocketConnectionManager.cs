using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace GameTogetherAPI.WebSockets
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public string AddSocket(WebSocket socket)
        {
            var connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, socket);
            Console.WriteLine($"Added connection: {connectionId}");
            return connectionId;
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

                socket.Dispose();
                Console.WriteLine($"Removed connection: {id}");
            }
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
