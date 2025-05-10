using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;
using GameTogetherAPI.WebSockets;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly WebSocketEventHandler _webSocketEventHandler;
        private readonly IUserService _userService;

        public ChatService(
            IChatRepository chatRepository,
            WebSocketEventHandler webSocketEventHandler,
            IUserService userService)
        {
            _chatRepository = chatRepository;
            _webSocketEventHandler = webSocketEventHandler;
            _userService = userService;
        }

        public async Task<List<GetMessagesInChatResponseDTO>> GetMessagesByChatIdAsync(int chatId, int userId)
        {
            var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId, userId);

            return messages.Select(m => new GetMessagesInChatResponseDTO
            {
                MessageId = m.Id,
                SenderName = m.Sender?.Username ?? "Unknown",
                SenderId = m.SenderId ?? 0,
                Content = m.Content,
                TimeStamp = m.TimeStamp,
                ChatId = m.ChatId
            }).ToList();
        }

        public async Task<List<GetUserInboxResponseDTO>> GetUserInboxAsync(int userId)
        {
            var inbox = await _chatRepository.GetUserInboxAsync(userId);

            return inbox.Select(chat => new GetUserInboxResponseDTO
            {
                ChatId = chat.ChatId,
                SessionId = chat.GroupId,
                SessionTitle = chat.Group?.Title,
                Participants = chat.UserChats
                    .Where(uc => uc.UserId != userId)
                    .Select(uc => new ChatParticipantDTO
                    {
                        UserId = uc.UserId,
                        Name = uc.User?.Username ?? "No Username"
                    }).ToList()
            }).ToList();
        }

        public async Task<bool> SendMessageToSessionAsync(int sessionId, int userId, SendMessageRequestDTO messageDto)
        {
            var chat = await _chatRepository.GetChatByGroupId(sessionId);
            if (chat == null) return false;

            var message = new Message
            {
                ChatId = chat.ChatId,
                SenderId = userId,
                Content = messageDto.Content,
                TimeStamp = DateTime.UtcNow
            };

            await _chatRepository.SendMessageToSessionAsync(message);

            var user = await _userService.GetProfileByIdAsync(userId);
            var senderName = user?.Username ?? "Unknown";

            var dto = new GetMessagesInChatResponseDTO
            {
                MessageId = message.Id,
                SenderId = userId,
                SenderName = senderName,
                Content = message.Content,
                TimeStamp = message.TimeStamp,
                ChatId = chat.ChatId
            };

            await _webSocketEventHandler.BroadcastMessageAsync(dto, chat.ChatId);
            return true;
        }

        public async Task<bool> SendMessageToUserAsync(int senderId, int receiverId, SendMessageRequestDTO messageDto)
        {
            var chat = await _chatRepository.GetPrivateChatBetweenUsersAsync(senderId, receiverId);

            if (chat == null)
            {
                chat = new Chat
                {
                    UserChats = new List<UserChat>
                    {
                        new UserChat { UserId = senderId },
                        new UserChat { UserId = receiverId }
                    }
                };

                var created = await _chatRepository.CreatePrivateChatAsync(chat);
                if (!created) return false;
            }

            var message = new Message
            {
                ChatId = chat.ChatId,
                SenderId = senderId,
                Content = messageDto.Content,
                TimeStamp = DateTime.UtcNow
            };

            await _chatRepository.SendMessageToUserAsync(message);

            var user = await _userService.GetProfileByIdAsync(senderId);
            var senderName = user?.Username ?? "Unknown";

            var dto = new GetMessagesInChatResponseDTO
            {
                MessageId = message.Id,
                SenderId = senderId,
                SenderName = senderName,
                Content = message.Content,
                TimeStamp = message.TimeStamp,
                ChatId = chat.ChatId
            };

            await _webSocketEventHandler.BroadcastMessageAsync(dto, chat.ChatId);
            return true;
        }
    }
}
