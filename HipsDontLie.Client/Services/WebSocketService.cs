using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HipsDontLie.WebSockets.Models;   // 👈 shared modeller
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

        // Dine gamle events
        public event Action? OnConnected;
        public event Action<int, string>? OnDisconnected;
        public event Action<string>? OnRawMessage;

        // 🔹 Typed events baseret på shared modeller
        public event Action<ChatMessage>? ChatMessageReceived;
        public event Action<TypingMessage>? TypingStarted;
        public event Action<TypingMessage>? TypingStopped;
        public event Action<PendingJoinRequestMessage>? PendingJoinRequestReceived;
        public event Action<GroupAcceptedMessage>? GroupAcceptedReceived;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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

        // 🔹 Brug dine shared modeller når du sender

        public Task JoinChatAsync(int chatId)
            => SendAsync(new JoinMessage { ChatId = chatId });

        public Task LeaveChatAsync(int chatId)
            => SendAsync(new LeaveMessage { ChatId = chatId });

        public Task SendTypingAsync(int chatId, int userId, string? username)
            => SendAsync(new TypingMessage
            {
                ChatId = chatId,
                UserId = userId,
                Username = username,
                Type = "typing"
            });

        public Task SendStopTypingAsync(int chatId, int userId, string? username)
            => SendAsync(new TypingMessage
            {
                ChatId = chatId,
                UserId = userId,
                Username = username,
                Type = "stopTyping"
            });

        private async Task SendAsync(IWebSocketMessage payload)
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

            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeProp))
                    return;

                var type = typeProp.GetString();

                switch (type)
                {
                    case "message":
                        {
                            var msg = JsonSerializer.Deserialize<ChatMessage>(rawJson, _jsonOptions);
                            if (msg is not null)
                                ChatMessageReceived?.Invoke(msg);
                            break;
                        }

                    case "typing":
                        {
                            var msg = JsonSerializer.Deserialize<TypingMessage>(rawJson, _jsonOptions);
                            if (msg is not null)
                                TypingStarted?.Invoke(msg);
                            break;
                        }

                    case "stopTyping":
                        {
                            var msg = JsonSerializer.Deserialize<TypingMessage>(rawJson, _jsonOptions);
                            if (msg is not null)
                                TypingStopped?.Invoke(msg);
                            break;
                        }

                    case "pending.join.request":
                        {
                            var msg = JsonSerializer.Deserialize<PendingJoinRequestMessage>(rawJson, _jsonOptions);
                            if (msg is not null)
                                PendingJoinRequestReceived?.Invoke(msg);
                            break;
                        }

                    case "group.accepted":
                        {
                            var msg = JsonSerializer.Deserialize<GroupAcceptedMessage>(rawJson, _jsonOptions);
                            if (msg is not null)
                                GroupAcceptedReceived?.Invoke(msg);
                            break;
                        }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse WS message: {ex.Message}");
            }
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
