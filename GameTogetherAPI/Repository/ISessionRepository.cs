using GameTogetherAPI.Models;

namespace GameTogetherAPI.Repository
{
    public interface ISessionRepository
    {
        Task<bool> CreateSessionAsync(Session session);
        Task<bool> AddUserToSessionAsync(UserSession userSession);
        Task<List<Session>> GetSessionsAsync();
        Task<List<Session>> GetSessionsByUserIdAsync(int userId);
    }
}
