using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services
{
    public class ChatService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;

        public ChatService(IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
        }
        

    }
}
