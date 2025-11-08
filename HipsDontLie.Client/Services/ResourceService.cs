using HipsDontLie.DTO;
using HipsDontLie.Shared.DTO;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HipsDontLie.Client.Services
{
    public class ResourceService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _jsRuntime;
        private readonly CustomAuthStateProvider _authProvider;

        public ResourceService(IJSRuntime jsRuntime,  HttpClient http, CustomAuthStateProvider authProvider)
        {
            _jsRuntime = jsRuntime;
            _authProvider = (CustomAuthStateProvider)authProvider;
            _http = http;
        }

        public async Task<GetProfileResponseDTO> GetProfileAsync()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            if (authState.User?.Identity?.IsAuthenticated != true)
                return new GetProfileResponseDTO();

            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/Users/get-profile");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var res = await _http.SendAsync(request);
            if (!res.IsSuccessStatusCode)
                return new GetProfileResponseDTO();

            var profile = await res.Content.ReadFromJsonAsync<GetProfileResponseDTO>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            });

            return profile ?? new GetProfileResponseDTO();
        }

    }
}
