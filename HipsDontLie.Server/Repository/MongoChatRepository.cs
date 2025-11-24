using HipsDontLie.Database;
using HipsDontLie.Models;
using HipsDontLie.Repository;
using HipsDontLie.Server.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HipsDontLie.Server.Repository
{
    public class MongoChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMongoCollection<MessageDocument> _messages;

        public MongoChatRepository(
            ApplicationDbContext context,
            IMongoClient mongoClient,
            IOptions<MongoChatSettings> mongoOptions)
        {
            _context = context;

            var mongoSettings = mongoOptions.Value;
            var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);
            _messages = database.GetCollection<MessageDocument>(mongoSettings.MessagesCollectionName);

            var indexKeys = Builders<MessageDocument>.IndexKeys
                .Ascending(m => m.ChatId)
                .Ascending(m => m.TimeStamp);

            _messages.Indexes.CreateOne(new CreateIndexModel<MessageDocument>(indexKeys));
        }
        private class MessageDocument
        {
            public string? Id { get; set; }
            public int ChatId { get; set; }
            public int SenderId { get; set; }
            public string Content { get; set; } = string.Empty;
            public DateTime TimeStamp { get; set; }
        }

        public async Task<bool> CreateGroupChatAsync(Chat chat)
        {
            try
            {
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Chat?> GetChatByGroupId(int sessionId)
        {
            return await _context.Chats.FirstOrDefaultAsync(c => c.GroupId == sessionId);
        }

        public async Task<bool> SendMessageToSessionAsync(Message message)
        {
            var msgId = Guid.NewGuid();
            var doc = new MessageDocument
            {
                Id = msgId.ToString(),
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                TimeStamp = message.TimeStamp
            };

            await _messages.InsertOneAsync(doc);
            return true;
        }

        public async Task<Chat?> GetPrivateChatBetweenUsersAsync(int senderId, int receiverId)
        {
            return await _context.Chats
                .Where(c => c.GroupId == null)
                .Where(c => c.UserChats.Any(uc => uc.UserId == senderId) &&
                            c.UserChats.Any(uc => uc.UserId == receiverId))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreatePrivateChatAsync(Chat chat)
        {
            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendMessageToUserAsync(Message message)
        {
            var msgId = Guid.NewGuid();
            var doc = new MessageDocument
            {
                Id = msgId.ToString(),
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                TimeStamp = message.TimeStamp
            };

            await _messages.InsertOneAsync(doc);
            return true;
        }

        public async Task<List<Chat>> GetUserInboxAsync(int userId)
        {
            return await _context.Chats
                .Where(c => c.UserChats.Any(uc => uc.UserId == userId))
                .Include(c => c.UserChats)
                    .ThenInclude(uc => uc.User)
                        .ThenInclude(u => u.Profile)
                .Include(c => c.Group)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByChatIdAsync(int chatId, int userId)
        {
            var isParticipant = await _context.UserChats.AnyAsync(uc => uc.ChatId == chatId && uc.UserId == userId);
            if (!isParticipant)
                throw new UnauthorizedAccessException("User is not part of this chat");

            var filter = Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId);
            var docs = await _messages.Find(filter)
                                      .SortBy(m => m.TimeStamp)
                                      .ToListAsync();

            var senderIds = docs.Select(d => d.SenderId).Distinct().ToList();
            var senders = await _context.Users
                .Where(u => senderIds.Contains(u.Id))
                .Include(u => u.Profile)
                .ToListAsync();
            var senderMap = senders.ToDictionary(s => s.Id);

            var result = new List<Message>(docs.Count);
            foreach (var doc in docs)
            {
                var message = new Message
                {
                    ChatId = doc.ChatId,
                    SenderId = doc.SenderId,
                    Content = doc.Content,
                    TimeStamp = doc.TimeStamp
                };

                if (senderMap.TryGetValue(doc.SenderId, out var sender))
                {
                    message.Sender = sender;
                }

                result.Add(message);
            }

            return result;
        }

        public async Task<bool> AddUserToChatAsync(UserChat userChat)
        {
            var exists = await _context.UserChats
                .AnyAsync(uc => uc.UserId == userChat.UserId && uc.ChatId == userChat.ChatId);

            if (!exists)
            {
                await _context.UserChats.AddAsync(userChat);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }


    }
}