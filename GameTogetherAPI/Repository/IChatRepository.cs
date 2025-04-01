using GameTogetherAPI.Models;

namespace GameTogetherAPI.Repository
{
    public interface IChatRepository
    {
        Task<bool> CreateSessionChatAsync(Chat chat);
    }
}
