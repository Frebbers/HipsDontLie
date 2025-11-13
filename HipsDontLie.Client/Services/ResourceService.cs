using HipsDontLie.Shared.DTO;
using System.Net.Http.Json;

namespace HipsDontLie.Client.Services
{
    public class ResourceService
    {
        private readonly HttpClient _http;
        public record ApiResponseMessage(string Message);

        public ResourceService(IHttpClientFactory factory) => _http = factory.CreateClient("Api");


        #region Profile
        public async Task<ProfileDTO> GetProfile(int? userId = null, CancellationToken ct = default)
        {
            var requestUri = "api/Users/get-profile";
            if (userId.HasValue) { requestUri = $"api/Users/profile/{userId}"; }
            using var res = await _http.GetAsync(requestUri, ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<ProfileDTO>(ct) ?? new();
        }

        public async Task<ApiResponseMessage?> UpdateProfile(UpdateProfileRequestDTO profile, CancellationToken ct = default)
        {
            using var res = await _http.PostAsJsonAsync("api/Users/update-profile", profile, ct);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage?>(ct);
        }
        #endregion

        #region Group
        public async Task<ApiResponseMessage?> CreateGroup(CreateGroupRequestDTO group, CancellationToken ct = default)
        {
            using var res = await _http.PostAsJsonAsync("api/Groups/create", group, ct);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct);
        }

        public async Task<ApiResponseMessage?> JoinGroup(int groupId, CancellationToken ct = default)
        {
            using var res = await _http.PostAsync($"api/Groups/{groupId}/join", null, ct);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct);
        }
        public async Task<ApiResponseMessage?> AcceptUserIntoGroup(int groupId, int userId, CancellationToken ct = default)
        {
            using var res = await _http.GetAsync($"api/Groups/{groupId}/{userId}/accept", ct);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct);
        }

        public async Task<ApiResponseMessage?> RejectUserFromGroup(int groupId, int userId, CancellationToken ct = default)
        {
            using var res = await _http.GetAsync($"api/Groups/{groupId}/{userId}/reject", ct);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct);
        }

        public async Task<List<GroupDTO>> GetUserGroups(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("api/Groups/user", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<List<GroupDTO>>(ct) ?? new();
        }

        public async Task<List<GroupDTO>> GetAllGroups(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("api/Groups", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<List<GroupDTO>>(ct) ?? new();
        }

        public async Task<GroupDTO> GetGroupByID(int groupId, CancellationToken ct = default)
        {
            using var res = await _http.GetAsync($"api/Groups/{groupId}", ct);
            if (!res.IsSuccessStatusCode) return new();

            return await res.Content.ReadFromJsonAsync<GroupDTO>(ct) ?? new();
        }
        #endregion

        #region Chat
        //TO-DO
        #endregion
    }
}
