using HipsDontLie.DTO;
using HipsDontLie.Shared.DTO;
using System.Net.Http.Json;

namespace HipsDontLie.Client.Services
{
    public class ResourceService
    {
        private readonly HttpClient _http;

        public ResourceService(HttpClient http) => _http = http;
        
        public async Task<GetProfileResponseDTO> GetProfileAsync(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("api/Users/get-profile", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<GetProfileResponseDTO>(ct) ?? new();
        }

        public async Task<List<GetGroupResponseDTO>> GetUserGroupsAsync(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("api/Groups/user", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<List<GetGroupResponseDTO>>(ct) ?? new();
        }

    }
}
