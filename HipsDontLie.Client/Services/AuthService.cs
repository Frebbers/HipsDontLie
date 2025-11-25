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

        public AuthService(IHttpClientFactory factory, CustomAuthStateProvider authProvider)
        {
            _authProvider = authProvider;
            _http = factory.CreateClient("Auth");
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var res = await _http.PostAsJsonAsync("api/auth/login", payload);
            if (!res.IsSuccessStatusCode) return false;

            var doc = await res.Content.ReadFromJsonAsync<JsonElement>();
            var token = doc.GetProperty("token").GetString();
            await _authProvider.MarkUserAsAuthenticated(token!);
            return true;
        }

        public sealed class RegisterResult
        {
            public bool Success { get; init; }
            public string? Message { get; init; }
        }

        public async Task<RegisterResult> RegisterAsync(string email, string userName, string displayName, string password)
        {
            var payload = new { Email = email, Username = userName, Displayname = displayName, Password = password };

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
