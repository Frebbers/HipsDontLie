using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HipsDontLie.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;

        public AuthService(HttpClient http, CustomAuthStateProvider authProvider)
        {
            _authProvider = (CustomAuthStateProvider)authProvider;
            _http = http;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var json = JsonSerializer.Serialize(new { email, password });
            var res = await _http.PostAsync("api/auth/login", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode) return false;

            var body = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var token = doc.RootElement.GetProperty("token").GetString();

            await _authProvider.MarkUserAsAuthenticated(token);
            return true;
        }

        public sealed class RegisterResult
        {
            public bool Success { get; init; }
            public string? Message { get; init; }
        }

        public async Task<RegisterResult> RegisterAsync(string email, string username, string password)
        {
            var payload = new { Email = email, Username = username, Password = password };

            using var res = await _http.PostAsJsonAsync("api/auth/register", payload);

            var body = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                return new RegisterResult { Success = true, Message = body };
            }

            return new RegisterResult { Success = false, Message = body };
        }

        public async Task LogoutAsync() => await _authProvider.LogoutAsync();

    }
}
