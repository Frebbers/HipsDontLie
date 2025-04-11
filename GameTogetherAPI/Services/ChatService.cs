using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<List<GetMessagesInChatResponseDTO>> GetMessagesByChatIdAsync(int chatId, int userId)
        {
            var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId, userId);


            var result = messages.Select(message => new GetMessagesInChatResponseDTO
            {
                MessageId = message.Id,
                SenderName = message.Sender.Profile.Name,
                SenderId = message.SenderId ?? 0,
                Content = message.Content,
                TimeStamp = message.TimeStamp
            }).ToList();

            return result;
        }

        public async Task<List<GetUserInboxResponseDTO>> GetUserInboxAsync(int userId)
        {
            var inbox = await _chatRepository.GetUserInboxAsync(userId);

            var result = new List<GetUserInboxResponseDTO>();

            foreach (var chat in inbox)
            {
                var participants = chat.UserChats
                    .Where(uc => uc.UserId != userId)
                    .Select(uc => new ChatParticipantDTO
                    {
                        UserId = uc.UserId,
                        Name = uc.User?.Profile?.Name ?? "John Doe",//when testing some users do not have a profile
                    })
                    .ToList();

                result.Add(new GetUserInboxResponseDTO()
                {
                    ChatId = chat.ChatId,
                    SessionId = chat.GroupId,
                    SessionTitle = chat.Group?.Title,
                    Participants = participants
                });
            }

            return result;
        }

        public async Task<bool> SendMessageToSessionAsync(int sessionId, int userId, SendMessageRequestDTO messageDto)
        {
            var chat = await _chatRepository.GetChatByGroupId(sessionId);
            if (chat == null)
                return false;

            var message = new Message()
            {
                ChatId = chat.ChatId,
                SenderId = userId,
                Content = messageDto.Content,
                TimeStamp = DateTime.Now,
            };

            await _chatRepository.SendMessageToSessionAsync(message);

            return true;
        }

        public async Task<bool> SendMessageToUserAsync(int senderId, int receiverId, SendMessageRequestDTO messageDto)
        {
            var chat = await _chatRepository.GetPrivateChatBetweenUsersAsync(senderId, receiverId);
            if (chat == null)
            {
                chat = new Chat()
                {
                    UserChats = new List<UserChat>{
                        new UserChat { UserId = senderId },
                        new UserChat { UserId = receiverId }
                    }
                };
                var createdChat = await _chatRepository.CreatePrivateChatAsync(chat);
                if (!createdChat)
                    return false;
            }

            var message = new Message
            {
                ChatId = chat.ChatId,
                SenderId = senderId,
                Content = messageDto.Content,
                TimeStamp = DateTime.UtcNow
            };

            await _chatRepository.SendMessageToUserAsync(message);
            return true;

        }
    }
}
