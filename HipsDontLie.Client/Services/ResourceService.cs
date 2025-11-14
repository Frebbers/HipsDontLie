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
            var requestUri = userId.HasValue
                ? $"api/Users/profile/{userId.Value}"
                : "api/Users/get-profile";

            return await _http.GetFromJsonAsync<ProfileDTO>(requestUri, ct)
                   ?? new ProfileDTO();
        }

        public async Task<ApiResponseMessage> UpdateProfile(UpdateProfileRequestDTO profile, CancellationToken ct = default)
        {
            using var res = await _http.PostAsJsonAsync("api/Users/update-profile", profile, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from UpdateProfile");
        }
        #endregion

        #region Group
        public async Task<ApiResponseMessage> CreateGroup(CreateGroupRequestDTO group, CancellationToken ct = default)
        {
            using var res = await _http.PostAsJsonAsync("api/Groups/create", group, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from CreateGroup");
        }

        public async Task<ApiResponseMessage> JoinGroup(int groupId, CancellationToken ct = default)
        {
            using var res = await _http.PostAsync($"api/Groups/{groupId}/join", null, ct);

            return await res.Content.ReadFromJsonAsync<ApiResponseMessage>(ct)
                   ?? new ApiResponseMessage("Unknown response from JoinGroup");
        }

        public async Task<ApiResponseMessage> AcceptUserIntoGroup(int groupId, int userId, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<ApiResponseMessage>($"api/Groups/{groupId}/{userId}/accept", ct)
                   ?? new ApiResponseMessage("Unknown response from AcceptUserIntoGroup");
        }

        public async Task<ApiResponseMessage> RejectUserFromGroup(int groupId, int userId, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<ApiResponseMessage>($"api/Groups/{groupId}/{userId}/reject", ct)
                   ?? new ApiResponseMessage("Unknown response from RejectUserFromGroup");
        }

        public async Task<List<GroupDTO>> GetUserGroups(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<List<GroupDTO>>("api/Groups/user", ct)
                   ?? new List<GroupDTO>();
        }

        public async Task<List<GroupDTO>> GetAllGroups(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<List<GroupDTO>>("api/Groups", ct)
                   ?? new List<GroupDTO>();
        }

        public async Task<GroupDTO> GetGroupByID(int groupId, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<GroupDTO>($"api/Groups/{groupId}", ct)
                   ?? new GroupDTO();
        }

        #endregion

        #region Chat
        // TO-DO
        #endregion
    }
}
