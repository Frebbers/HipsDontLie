using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameTogetherAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
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

        public async Task<bool> AddOrUpdateProfileAsync(Profile profile)
        {
            try
            {
                var existingProfile = await _context.Profiles.FindAsync(profile.Id);

                if (existingProfile == null)
                {
                    await _context.Profiles.AddAsync(profile);
                }
                else
                {
                    _context.Entry(existingProfile).CurrentValues.SetValues(profile);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (existingUser != null)
            {
                _context.Users.Remove(existingUser);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
