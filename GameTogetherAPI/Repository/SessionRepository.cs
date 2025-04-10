using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Repository {
    /// <summary>
    /// Handles database operations related to game sessions.
    /// </summary>
    public class SessionRepository : ISessionRepository {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionRepository"/> class.
        /// </summary>
        /// <param name="context">The database context for interacting with sessions.</param>
        public SessionRepository(ApplicationDbContext context) {
            _context = context;
        }

        /// <summary>
        /// Creates a new session in the database.
        /// </summary>
        /// <param name="session">The session to be created.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if successful, otherwise false.</returns>
        public async Task<Session> CreateSessionAsync(Session session)
        {
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        /// <summary>
        /// Retrieves a session by its unique identifier, including its participants and their profiles.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning the session if found, otherwise null.
        /// </returns>
        public async Task<Session> GetSessionByIdAsync(int sessionId) {
            return await _context.Sessions
                .Include(s => s.Members)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Profile)
                .Include(c => c.Chat)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

        }

        /// <summary>
        /// Adds a user to a session and saves the change to the database.
        /// </summary>
        /// <param name="userSession">The user-session relationship to be added.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning true if the user is successfully added to the session.
        /// </returns>
        public async Task<bool> AddUserToSessionAsync(UserSession userSession)
        {
            var exists = await _context.UserSessions.FirstOrDefaultAsync(us => us.UserId == userSession.UserId && us.SessionId == userSession.SessionId);

            if (exists != null) {
                exists.Status = userSession.Status;
                _context.UserSessions.Update(exists);
            }
            else {
                await _context.UserSessions.AddAsync(userSession);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a user from a session.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the user is successfully removed, otherwise false.</returns>
        public async Task<bool> RemoveUserFromSessionAsync(int userId, int sessionId) {
            var userSession = await _context.UserSessions.FirstOrDefaultAsync
                (us => us.UserId == userId && us.SessionId == sessionId);

            if (userSession == null)
                return false;

            //A check to see if the user is the owner of the session
            var session = await _context.Sessions.FirstOrDefaultAsync
                (s => s.Id == sessionId && s.OwnerId == userId);

            if (session != null)
            {
                _context.Sessions.Remove(session);
            }
            else
            {
                _context.UserSessions.Remove(userSession);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates whether a user is not already a participant in a session.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the user is not already a participant, otherwise false.</returns>
        public async Task<bool> ValidateUserSessionAsync(int userId, int sessionId) {
            return await _context.Sessions
                            .Where(s => s.Id == sessionId)
                            .Select(s => !_context.UserSessions.Any(us => us.UserId == userId && us.SessionId == sessionId))
                            .FirstOrDefaultAsync();
        }

        public async Task<bool> ValidateAcceptSessionAsync(int userId, int sessionId,int ownerId)
        {
            return await _context.Sessions
                            .Where(s => s.Id == sessionId && s.OwnerId == ownerId)
                            .Select(s => _context.UserSessions.Any(us => us.UserId == userId && us.SessionId == sessionId && us.Status == UserSessionStatus.Pending))
                            .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all sessions a user is participating in.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that represents the asynchronous operation, returning a list of sessions the user is part of.</returns>
        public async Task<List<Session>> GetSessionsByUserIdAsync(int userId) {
            return await _context.Sessions
                            .Where(s => s.Members.Any(p => p.UserId == userId))
                            .Include(s => s.Members)
                                .ThenInclude(p => p.User)
                                .ThenInclude(u => u.Profile)
                            .Include(c => c.Chat)
                            .ToListAsync();
        }

        /// <summary>
        /// Retrieves all available sessions.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, returning a list of all sessions.</returns>
        public async Task<List<Session>> GetSessionsAsync() {
            return await _context.Sessions
                .Include(s => s.Members)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Profile)
                .Include(c => c.Chat)
                .ToListAsync();
        }
    }
}
