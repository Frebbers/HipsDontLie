using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;

namespace GameTogetherAPI.Repository
{
    public interface IChatRepository
    {
        Task<bool> AddUserToChatAsync(UserChat userChat);
        Task<bool> CreatePrivateChatAsync(Chat chat);
        Task<bool> CreateSessionChatAsync(Chat chat);
        Task<Chat> GetChatBySessionId(int sessionId);
        Task<List<Message>> GetMessagesByChatIdAsync(int chatId, int userId);
        Task<Chat?> GetPrivateChatBetweenUsersAsync(int senderId, int receiverId);
        Task<List<Chat>> GetUserInboxAsync(int userId);
        Task<bool> SendMessageToSessionAsync(Message message);
        Task<bool> SendMessageToUserAsync(Message message);
    }
}
