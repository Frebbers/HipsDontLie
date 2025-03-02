using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionService(IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<bool> CreateSessionAsync(int userId, CreateSessionRequestDTO sessionDto)
        {
            var session = new Session()
            {
                Title = sessionDto.Title,
                AgeRange = sessionDto.AgeRange,
                Description = sessionDto.Description,
                IsVisible = sessionDto.IsVisible,
                OwnerId = userId,
                Tags = sessionDto.Tags,

            };

            bool isSuccess = await _sessionRepository.CreateSessionAsync(session);

            if (!isSuccess)
                return false;

            var userSession = new UserSession
            {
                UserId = userId,
                SessionId = session.Id
            };

            await _sessionRepository.AddUserToSessionAsync(userSession);

            return true;

        }


        public async Task<List<GetSessionsResponseDTO>> GetSessionsAsync(int? userId = null)
        {
        
            var sessions = userId == null
                ? await _sessionRepository.GetSessionsAsync() 
                : await _sessionRepository.GetSessionsByUserIdAsync((int)userId);

            if (sessions == null)
                return null;

            var results = new List<GetSessionsResponseDTO>();

            foreach (var session in sessions)
            {
                results.Add(new GetSessionsResponseDTO
                {
                    Id = session.Id,
                    Title = session.Title,
                    OwnerId = session.OwnerId,
                    IsVisible = session.IsVisible,
                    AgeRange = session.AgeRange,
                    Description = session.Description,
                    Tags = session.Tags,
                    Participants = session.Participants
                    .Select(p => new ParticipantDTO
                    {
                        UserId = p.UserId,
                        Name = p.User.Profile.Name,
                    })
                    .ToList()
                });
            }
            return results; 
        }

    }
}
