using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;

namespace GameTogetherAPI.Services
{
    public interface ISessionService
    {
        Task<bool> CreateSessionAsync(int userId , CreateSessionRequestDTO session);
        Task<GetSessionByIdResponseDTO> GetSessionByIdAsync(int sessionId);
        Task<List<GetSessionsResponseDTO>> GetSessionsAsync(int? userId = null);
        Task<bool> JoinSessionAsync(int userId, int sessionId);
        Task<bool> LeaveSessionAsync(int userId, int sessionId);
    }
}
