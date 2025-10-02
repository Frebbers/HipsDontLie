using HipsDontLie.Shared.DTO;
using HipsDontLie.Database;
using HipsDontLie.Models;
using Microsoft.EntityFrameworkCore;

namespace HipsDontLie.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateGroupChatAsync(Chat chat)
        {
            try
            {
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (DbUpdateException ex)
            {
                return false;
            }
            catch (Exception ex)
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
           await _context.Messages.AddAsync(message);
           await _context.SaveChangesAsync();
           return true;
        }

        public async Task<Chat?> GetPrivateChatBetweenUsersAsync(int senderId, int receiverId)
        {
            return await _context.Chats
                            .Where(c => c.GroupId == null)
                            .Where(c => c.UserChats.Any(uc => uc.UserId == senderId) && c.UserChats.Any(uc => uc.UserId == receiverId))
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
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
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
                .Include(c => c.Messages)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByChatIdAsync(int chatId, int userId)
        {
            var isParticipant = await _context.UserChats.AnyAsync(uc => uc.ChatId == chatId && uc.UserId == userId);

            if (!isParticipant)
                throw new UnauthorizedAccessException("User is not part of this chat");

            return await _context.Messages
                            .Where(c => c.ChatId == chatId)
                            .Include(s => s.Sender)
                                .ThenInclude(p => p.Profile)
                            .OrderBy(t => t.TimeStamp)
                            .ToListAsync();
        }

        public async Task<bool> AddUserToChatAsync(UserChat userChat)
        {
            // Avoid duplicate entries
            var exists = await _context.UserChats.AnyAsync
                (uc => uc.UserId == userChat.UserId && uc.ChatId == userChat.ChatId);

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
