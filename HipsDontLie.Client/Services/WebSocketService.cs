using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace HipsDontLie.Client.Services
{
    public class WebSocketService : IAsyncDisposable
    {
        private readonly IJSRuntime _js;
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authState; 

        private IJSObjectReference? _module;
        private DotNetObjectReference<WebSocketService>? _dotNetRef;

        private bool _isConnecting;
        public bool IsConnected { get; private set; }

        public event Action? OnConnected;
        public event Action<int, string>? OnDisconnected;
        public event Action<string>? OnRawMessage;

        public WebSocketService(
            IJSRuntime js,
            HttpClient http,
            CustomAuthStateProvider authState)
        {
            _js = js;
            _http = http;
            _authState = authState;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected || _isConnecting)
                return;

            _isConnecting = true;
            try
            {
                var token = await _authState.GetAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                    return;

                if (_http.BaseAddress is null)
                    throw new InvalidOperationException("HttpClient.BaseAddress er ikke sat.");

                var apiBase = _http.BaseAddress;
                var scheme = apiBase.Scheme == "https" ? "wss" : "ws";
                var host = apiBase.IsDefaultPort ? apiBase.Host : $"{apiBase.Host}:{apiBase.Port}";
                var wsUrl = $"{scheme}://{host}/ws/events?token={Uri.EscapeDataString(token)}";

                _module ??= await _js.InvokeAsync<IJSObjectReference>(
                    "import", "./js/websocketClient.js");

                _dotNetRef ??= DotNetObjectReference.Create(this);

                await _module.InvokeVoidAsync("connect", wsUrl, _dotNetRef);
            }
            finally
            {
                _isConnecting = false;
            }
        }

        public async Task CloseAsync()
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("close");
                IsConnected = false;
            }
        }

        public Task JoinChatAsync(int chatId)
            => SendAsync(new { type = "join", chatId });

        public Task LeaveChatAsync(int chatId)
            => SendAsync(new { type = "leave", chatId });

        public Task SendChatMessageAsync(int chatId, string content)
            => SendAsync(new
            {
                type = "message",
                chatId,
                content
            });

        public Task SendTypingAsync(int chatId)
            => SendAsync(new { type = "typing", chatId });

        public Task SendStopTypingAsync(int chatId)
            => SendAsync(new { type = "stopTyping", chatId });

        private async Task SendAsync(object payload)
        {
            if (_module is null)
                throw new InvalidOperationException("WebSocket modulet er ikke loadet. Kald ConnectAsync først.");

            await _module.InvokeVoidAsync("send", payload);
        }

        [JSInvokable]
        public void OnJsConnected()
        {
            IsConnected = true;
            OnConnected?.Invoke();
        }

        [JSInvokable]
        public void OnJsDisconnected(int code, string reason)
        {
            IsConnected = false;
            OnDisconnected?.Invoke(code, reason);
        }

        [JSInvokable]
        public void OnJsMessageReceived(string rawJson)
        {
            OnRawMessage?.Invoke(rawJson);
        }

        public async ValueTask DisposeAsync()
        {
            if (_module is not null)
            {
                await _module.DisposeAsync();
            }
            _dotNetRef?.Dispose();
        }
    }
}
