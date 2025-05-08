using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GameTogetherAPI.WebSockets
{
    public class WebSocketCleanupService : IHostedService, IDisposable
    {
        private readonly WebSocketConnectionManager _manager;
        private Timer? _timer;

        public WebSocketCleanupService(WebSocketConnectionManager manager)
        {
            _manager = manager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run every 30 seconds
            _timer = new Timer(CleanupSockets, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }

        private void CleanupSockets(object? state)
        {
            _manager.CleanupClosedSockets();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
