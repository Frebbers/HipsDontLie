using HipsDontLie.Shared.DTO;
using System.Net.Http.Json;

namespace HipsDontLie.Client.Services
{
    public class ResourceService
    {
        private readonly HttpClient _http;

        public ResourceService(IHttpClientFactory factory) => _http = factory.CreateClient("Api");

        public async Task<ProfileDTO> GetProfile(int? userId = null, CancellationToken ct = default)
        {
            var requestUri = "api/Users/get-profile";
            if (userId.HasValue) { requestUri = $"api/Users/profile/{userId}"; }
            using var res = await _http.GetAsync(requestUri, ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<ProfileDTO>(ct) ?? new();
        }

        public async Task<List<GroupDTO>> GetUserGroups(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("api/Groups/user", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<List<GroupDTO>>(ct) ?? new();
        }
        
        public async Task<GroupDTO> GetGroupByID(int groupId, CancellationToken ct = default)
        {
            using var res = await _http.GetAsync($"api/Groups/{groupId}", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<GroupDTO>(ct) ?? new();
        }

    }
}
