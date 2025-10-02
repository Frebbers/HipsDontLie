using HipsDontLie.DTO;

namespace HipsDontLie.Services
{
    public interface IChatService
    {
        Task<List<GetMessagesInChatResponseDTO>> GetMessagesByChatIdAsync(int chatId, int userId);
        Task<List<GetUserInboxResponseDTO>> GetUserInboxAsync(int userId);
        Task<bool> SendMessageToSessionAsync(int sessionId, int userId, SendMessageRequestDTO messageDto);
        Task<bool> SendMessageToUserAsync(int senderId, int receiverId, SendMessageRequestDTO messageDto);
    }
}