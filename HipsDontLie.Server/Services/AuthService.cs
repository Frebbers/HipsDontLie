using HipsDontLie.Models;
using HipsDontLie.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace HipsDontLie.Services
{
    /// <summary>
    /// Provides authentication and user management services, including registration, login, and token generation.
    /// </summary>
    public class AuthService : IAuthService
    {

        // private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly string[] _testEmails;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userRepository">The repository for user-related database operations.</param>
        /// <param name="configuration">The configuration settings for authentication.</param>
        public AuthService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;

            var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
            _testEmails = env.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? new[] { "user@example.com", "user1@example.com", "user2@example.com" }
                : Array.Empty<string>();
        }

        /// <summary>
        /// Registers a new user using ASP.NET Core Identity.
        /// </summary>
        public async Task<AuthStatus> RegisterUserAsync(string email, string username, string password)
        {
            // Check for existing user
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return AuthStatus.UserExists;

            
            if (!IsPasswordValid(password))
                return AuthStatus.WeakPassword;

            // Determine if this is a "test" user
            bool isTestEmail = _testEmails.Contains(email, StringComparer.OrdinalIgnoreCase);

            // Create new Identity user
            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = isTestEmail // auto-confirm for test users
            };

            // Let Identity handle password hashing and user creation
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                // If password or validation failed, you can inspect errors if needed
                return AuthStatus.WeakPassword;
            }

            if (isTestEmail)
                return AuthStatus.TestUserCreated;

            // If not test user, they need to confirm email
            return AuthStatus.UserCreated;
        }

        private bool IsPasswordValid(string password)
        {
            // optional: your own quick checks before Identity validation
            return password.Length >= 6;
        }

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to be deleted.</param>
        /// <param name="email">The email of the user to be deleted. Used only if we do not know their userID</param>
        /// <returns>A task representing the asynchronous operation, returning true if the user is successfully deleted.</returns>
        public async Task<bool> DeleteUserAsync(int userId, string? email = null)
        {
            User? user = null;
            if (userId == 0 && email != null)
                user = await _userManager.FindByEmailAsync(email);
            else
                user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// Sends a verification email to the specified user.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <returns>True if the email is sent successfully; false if the user is not found.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the email template is missing.</exception>
        public async Task<bool> SendEmailVerificationAsync(string email) {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var smtpSettings = _configuration.GetSection("EmailSettings");
            var verificationUrl = smtpSettings["VerificationUrl"] + $"?userId={user.Id}&token={encodedToken}";
            var smtpServer = smtpSettings["SmtpServer"];
            var port = int.Parse(smtpSettings["Port"]);
            var senderEmail = smtpSettings["SenderEmail"];
            var senderPassword = smtpSettings["SenderPassword"];

            // **Correct the file path to match the Models folder**
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "email-template.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Email template not found at {templatePath}");

            string emailBody = await File.ReadAllTextAsync(templatePath);
            emailBody = emailBody.Replace("{VERIFICATION_URL}", verificationUrl);

            using (var client = new SmtpClient(smtpServer)) {
                client.Port = port;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage {
                    From = new MailAddress(senderEmail),
                    Subject = "Verify Your Email",
                    Body = emailBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);
            }

            return true;
        }

        /// <summary>
        /// Verifies the email using a JWT token.
        /// </summary>
        public async Task<bool> ConfirmEmailAsync(int userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        /// <summary>
        /// Authenticates a user by verifying their credentials and returning a JWT token if valid.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The plaintext password provided for authentication.</param>
        /// <returns>A task representing the asynchronous operation, returning a JWT token if authentication is successful, otherwise null.</returns>
        public async Task<string?> AuthenticateUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var validPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!validPassword)
                return null;

            if (!user.EmailConfirmed)
                return null;

            return GenerateJwtToken(user);
        }

        /// <summary>
        /// Generates a JWT token for an authenticated user.
        /// </summary>
        /// <param name="user">The authenticated user for whom the token is generated.</param>
        /// <returns>A string representing the generated JWT token.</returns>
        private string GenerateJwtToken(User user)
        {

            //TO-DO: Roles
            //var roles = await _userManager.GetRolesAsync(user);
            //foreach (var role in roles)
                //claims.Add(new Claim(ClaimTypes.Role, role));

            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

}
