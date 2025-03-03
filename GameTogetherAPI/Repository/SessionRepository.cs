using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Repository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ApplicationDbContext _context;

        public SessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateSessionAsync(Session session)
        {
            try
            {
                await _context.Sessions.AddAsync(session);
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
        public async Task<bool> AddUserToSessionAsync(UserSession userSession)
        {
            await _context.UserSessions.AddAsync(userSession);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateUserSessionAsync(int userId, int sessionId)
        {

            bool sessionExists = await _context.Sessions.AnyAsync(s => s.Id == sessionId);
            if (!sessionExists)
                return false;

            bool isParticipant = await _context.UserSessions
                .AnyAsync(us => us.UserId == userId && us.SessionId == sessionId);

            return !isParticipant;
        }

        public async Task<List<Session>> GetSessionsByUserIdAsync(int userId)
        {
            return await _context.Sessions
                .Where(s => s.Participants.Any(p => p.UserId == userId))
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Profile)
                .ToListAsync();
        }
        public async Task<List<Session>> GetSessionsAsync()
        {
            return await _context.Sessions
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Profile).ToListAsync();
        }

    }
}
