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
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                socket.Dispose();
            }
        }
    }
}
