using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateSessionChatAsync(Chat chat)
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
    }
}
