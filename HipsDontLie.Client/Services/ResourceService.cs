using HipsDontLie.Shared.DTO;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace HipsDontLie.Client.Services
{
    public class ResourceService
    {
        private readonly HttpClient _http;
        private CustomAuthStateProvider _authStateProvider;
        public record ApiResponseMessage(string Message);

        public ResourceService(IHttpClientFactory factory, CustomAuthStateProvider authStateProvider) {
            _http = factory.CreateClient("Api");
            _authStateProvider = authStateProvider;
        }

        #region Profile
        public async Task<ProfileDTO> GetProfile(int? userId = null, CancellationToken ct = default)
        {
            var requestUri = userId.HasValue
                ? $"Users/profile/{userId.Value}"
                : "Users/get-profile";

            return await _http.GetFromJsonAsync<ProfileDTO>(requestUri, ct)
                   ?? new ProfileDTO();
        }

        public async Task<ApiResponseMessage> UpdateProfile(UpdateProfileRequestDTO profile, CancellationToken ct = default)
        {
            using var res = await _http.PutAsJsonAsync("Users/update-profile", profile, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from UpdateProfile");
        }

        public async Task<ApiResponseMessage> DeleteUser(CancellationToken ct = default) {
            using var res = await _http.DeleteAsync("auth/remove-user", ct);
            await _authStateProvider.LogoutAsync();

            var raw = await res.Content.ReadAsStringAsync(ct);

            if (!string.IsNullOrWhiteSpace(raw) && raw.Length >= 2 &&
                raw[0] == '"' && raw[^1] == '"') {
                raw = raw[1..^1];
            }
            
            return new ApiResponseMessage(string.IsNullOrWhiteSpace(raw)
                ? "User deleted successfully."
                : raw);
        }


        #endregion

        #region Group
        public async Task<ApiResponseMessage> CreateGroup(CreateGroupRequestDTO group, CancellationToken ct = default)
        {
            using var res = await _http.PostAsJsonAsync("Groups/create", group, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from CreateGroup");
        }

        public async Task<ApiResponseMessage> UpdateGroup(int groupId, GroupDTO group, CancellationToken ct = default) {
            using var res = await _http.PutAsJsonAsync($"Groups/{groupId}", group, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from UpdateGroup");
        }

        public async Task<ApiResponseMessage> DeleteGroup(int groupId, CancellationToken ct = default) {
            using var res = await _http.DeleteAsync($"Groups/{groupId}", ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from DeleteGroup");
        }

        public async Task<ApiResponseMessage> JoinGroup(int groupId, CancellationToken ct = default)
        {
            using var res = await _http.PostAsync($"Groups/{groupId}/join", null, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from JoinGroup");
        }

        public async Task<ApiResponseMessage> LeaveGroup(int groupId, CancellationToken ct = default) {
            using var res = await _http.DeleteAsync($"Groups/{groupId}/leave", ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from LeaveGroup");
        }

        public async Task<ApiResponseMessage> AcceptUserIntoGroup(int groupId, int userId, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<ApiResponseMessage>($"Groups/{groupId}/{userId}/accept", ct)
                   ?? new ApiResponseMessage("Unknown response from AcceptUserIntoGroup");
        }

        public async Task<ApiResponseMessage> RejectUserFromGroup(int groupId, int userId, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<ApiResponseMessage>($"Groups/{groupId}/{userId}/reject", ct)
                   ?? new ApiResponseMessage("Unknown response from RejectUserFromGroup");
        }

        public async Task<List<GroupDTO>> GetUserGroups(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<List<GroupDTO>>("Groups/user", ct)
                   ?? new List<GroupDTO>();
        }

        public async Task<List<GroupDTO>> GetAllGroups(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<List<GroupDTO>>("Groups", ct)
                   ?? new List<GroupDTO>();
        }

        public async Task<GroupDTO> GetGroupByID(int groupId, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<GroupDTO>($"Groups/{groupId}", ct)
                   ?? new GroupDTO();
        }

        #endregion

        #region Chat
        // TO-DO
        #endregion
    }
}
