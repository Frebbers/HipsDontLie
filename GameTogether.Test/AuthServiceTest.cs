using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;
using GameTogetherAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace GameTogether.Test
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private IConfiguration _configuration;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            // Create configuration with in-memory values for testing
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:SecretKey", "TestSecretKeyWithAtLeast32Characters!!"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"ASPNETCORE_ENVIRONMENT", "Development"},
                {"EmailSettings:SmtpServer", "smtp.test.com"},
                {"EmailSettings:Port", "587"},
                {"EmailSettings:SenderEmail", "test@example.com"},
                {"EmailSettings:SenderPassword", "password"},
                {"EmailSettings:VerificationUrl", "http://test.com/verify"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Create mock repository
            _mockUserRepository = new Mock<IUserRepository>();

            // Create auth service with real configuration but mock repository
            _authService = new AuthService(_mockUserRepository.Object, _configuration);
        }

        #region RegisterUserAsync Tests

        [Test]
        public async Task RegisterUserAsync_NewUser_ReturnsUserCreated()
        {
            // Arrange
            string email = "user@example.com";
            string username = "newuser";
            string password = "Password123";

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync((User)null);
            _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUserAsync(email, username, password);

            // Assert
            Assert.That(result, Is.EqualTo(AuthStatus.TestUserCreated));
            _mockUserRepository.Verify(r => r.AddUserAsync(It.Is<User>(u => 
                u.Email == email && 
                u.Username == username && 
                u.PasswordHash != password)), Times.Once);
        }

        [Test]
        public async Task RegisterUserAsync_ExistingUser_ReturnsUserExists()
        {
            // Arrange
            string email = "user@example.com";
            string username = "existing";
            string password = "Password123";

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync(new User { Email = email });

            // Act
            var result = await _authService.RegisterUserAsync(email, username, password);

            // Assert
            Assert.That(result, Is.EqualTo(AuthStatus.UserExists));
            _mockUserRepository.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task RegisterUserAsync_WeakPassword_ReturnsWeakPassword()
        {
            // Arrange
            string email = "user@example.com";
            string username = "user";
            string password = "weak"; // Too short, missing requirements

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync((User)null);

            // Act
            var result = await _authService.RegisterUserAsync(email, username, password);

            // Assert
            Assert.That(result, Is.EqualTo(AuthStatus.WeakPassword));
            _mockUserRepository.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task RegisterUserAsync_TestEmail_SetsEmailVerifiedTrue()
        {
            // Arrange
            string email = "user@example.com"; // Test email from _testEmails array
            string username = "testuser";
            string password = "Password123";

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync((User)null);
            _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUserAsync(email, username, password);

            // Assert
            Assert.That(result, Is.EqualTo(AuthStatus.TestUserCreated));
            _mockUserRepository.Verify(r => r.AddUserAsync(It.Is<User>(u => u.IsEmailVerified == true)), Times.Once);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Test]
        public async Task DeleteUserAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            _mockUserRepository.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _authService.DeleteUserAsync(userId);

            // Assert
            Assert.IsTrue(result);
            _mockUserRepository.Verify(r => r.DeleteUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task DeleteUserAsync_InvalidId_ReturnsFalse()
        {
            // Arrange
            int userId = 999;
            _mockUserRepository.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _authService.DeleteUserAsync(userId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteUserAsync_ValidEmail_FindsUserAndDeletes()
        {
            // Arrange
            string email = "user@example.com";
            int userId = 1;
            
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync(new User { Id = userId, Email = email });
            _mockUserRepository.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _authService.DeleteUserAsync(0, email);

            // Assert
            Assert.IsTrue(result);
            _mockUserRepository.Verify(r => r.DeleteUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task DeleteUserAsync_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@example.com";
            
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.DeleteUserAsync(0, email);

            // Assert
            Assert.IsFalse(result);
            _mockUserRepository.Verify(r => r.DeleteUserAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region SendEmailVerificationAsync Tests

        [Test]
        public async Task SendEmailVerificationAsync_UserNotFound_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@example.com";
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.SendEmailVerificationAsync(email);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region ConfirmEmailAsync Tests

        [Test]
        public async Task ConfirmEmailAsync_ValidToken_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:SecretKey").Value);
            if (key == null)
            {
                throw new Exception("SecretKey is not set in the configuration.");
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim("email_verification", "true")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var validToken = tokenHandler.WriteToken(securityToken);

            _mockUserRepository.Setup(r => r.ConfirmEmailAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _authService.ConfirmEmailAsync(validToken);

            // Assert
            Assert.IsTrue(result);
            _mockUserRepository.Verify(r => r.ConfirmEmailAsync(userId), Times.Once);
        }

        [Test]
        public async Task ConfirmEmailAsync_InvalidToken_ReturnsFalse()
        {
            // Arrange
            string invalidToken = "invalid.token.string";

            // Act
            var result = await _authService.ConfirmEmailAsync(invalidToken);

            // Assert
            Assert.IsFalse(result);
            _mockUserRepository.Verify(r => r.ConfirmEmailAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region AuthenticateUserAsync Tests

        [Test]
        public async Task AuthenticateUserAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            string email = "user@example.com";
            string password = "Password123";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync(new User 
                { 
                    Id = 1, 
                    Email = email, 
                    PasswordHash = hashedPassword,
                    IsEmailVerified = true 
                });

            // Act
            var token = await _authService.AuthenticateUserAsync(email, password);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsNotEmpty(token);
        }

        [Test]
        public async Task AuthenticateUserAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            string email = "nonexistent@example.com";
            string password = "Password123";

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var token = await _authService.AuthenticateUserAsync(email, password);

            // Assert
            Assert.IsNull(token);
        }

        [Test]
        public async Task AuthenticateUserAsync_IncorrectPassword_ReturnsNull()
        {
            // Arrange
            string email = "user@example.com";
            string correctPassword = "Password123";
            string wrongPassword = "WrongPassword123";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync(new User 
                { 
                    Id = 1, 
                    Email = email, 
                    PasswordHash = hashedPassword,
                    IsEmailVerified = true 
                });

            // Act
            var token = await _authService.AuthenticateUserAsync(email, wrongPassword);

            // Assert
            Assert.IsNull(token);
        }

        [Test]
        public async Task AuthenticateUserAsync_EmailNotVerified_ReturnsNull()
        {
            // Arrange
            string email = "user@example.com";
            string password = "Password123";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(email))
                .ReturnsAsync(new User 
                { 
                    Id = 1, 
                    Email = email, 
                    PasswordHash = hashedPassword,
                    IsEmailVerified = false // Not verified
                });

            // Act
            var token = await _authService.AuthenticateUserAsync(email, password);

            // Assert
            Assert.IsNull(token);
        }

        #endregion
    }
}