using GameTogetherAPI.Models;

namespace GameTogetherAPI.Services
{
    public interface IAuthService
    {

        Task<bool> RegisterUserAsync(string email, string password);
        Task<string> AuthenticateUserAsync(string email, string password);
        Task<bool> DeleteUserAsync(int userId);
    }
}
