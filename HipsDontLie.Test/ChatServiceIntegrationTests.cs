using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HipsDontLie.Database;
using HipsDontLie.Models;
using HipsDontLie.Server.Repository;
using HipsDontLie.Server.Settings;
using HipsDontLie.Services;
using HipsDontLie.Shared.DTO;
using HipsDontLie.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace HipsDontLie.Test
{
    [TestFixture]
    public class ChatServiceIntegrationTests
    {
        private MongoChatSettings? _baseMongoSettings;
        private IMongoClient? _mongoClient;
        private bool _mongoAvailable;
        private string? _mongoUnavailableReason;
        private List<string> _createdDatabases = new();

        [SetUp]
        public void SetUp()
        {
            _createdDatabases = new List<string>();

            var connectionString = Environment.GetEnvironmentVariable("MongoChat__ConnectionString");
            var databaseName = Environment.GetEnvironmentVariable("MongoChat__DatabaseName");
            var collectionName = Environment.GetEnvironmentVariable("MongoChat__MessagesCollectionName") ?? "messages";

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseName))
            {
                _mongoAvailable = false;
                _mongoUnavailableReason = "MongoChat settings not configured in environment.";
                return;
            }

            try
            {
                _mongoClient = new MongoClient(connectionString);
                var database = _mongoClient.GetDatabase(databaseName);
                database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));

                _baseMongoSettings = new MongoChatSettings
                {
                    ConnectionString = connectionString,
                    DatabaseName = databaseName,
                    MessagesCollectionName = collectionName
                };

                _mongoAvailable = true;
                _mongoUnavailableReason = null;
            }
            catch (Exception ex)
            {
                _mongoClient = null;
                _baseMongoSettings = null;
                _mongoAvailable = false;
                _mongoUnavailableReason = ex.Message;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_mongoClient != null)
            {
                foreach (var dbName in _createdDatabases)
                {
                    try
                    {
                        _mongoClient.DropDatabase(dbName);
                    }
                    catch
                    {
                        // ignore cleanup failures
                    }
                }
            }

            _mongoClient = null;
            _baseMongoSettings = null;
            _mongoAvailable = false;
            _mongoUnavailableReason = null;
            _createdDatabases.Clear();
        }

        [Test]
        public async Task PrivateChat_AllowsMessageExchangeBetweenParticipants()
        {
            SkipIfMongoUnavailable();

            using var context = CreateInMemoryDbContext();
            var userNames = await SeedUsersAsync(context,
                (1, "alice@example.com", "Alice"),
                (2, "bob@example.com", "Bob"));

            var chatService = CreateChatService(context, userNames);

            var firstSend = await chatService.SendMessageToUserAsync(1, 2, new SendMessageRequestDTO { Content = "Hello Bob!" });
            var secondSend = await chatService.SendMessageToUserAsync(2, 1, new SendMessageRequestDTO { Content = "Hey Alice!" });

            var chat = await GetChatByParticipantsAsync(context, 1, 2);
            var messages = await chatService.GetMessagesByChatIdAsync(chat.ChatId, 1);

            Assert.Multiple(() =>
            {
                Assert.That(firstSend, Is.True);
                Assert.That(secondSend, Is.True);
                Assert.That(messages, Has.Count.EqualTo(2));
                Assert.That(messages.Select(m => m.Content).ToArray(), Is.EqualTo(new[] { "Hello Bob!", "Hey Alice!" }));
                Assert.That(messages.Select(m => m.SenderId).ToArray(), Is.EqualTo(new[] { 1, 2 }));
            });
        }

        [Test]
        public async Task PrivateChat_DeniesAccessToNonParticipant()
        {
            SkipIfMongoUnavailable();

            using var context = CreateInMemoryDbContext();
            var userNames = await SeedUsersAsync(context,
                (1, "alice@example.com", "Alice"),
                (2, "bob@example.com", "Bob"),
                (3, "charlie@example.com", "Charlie"));

            var chatService = CreateChatService(context, userNames);

            await chatService.SendMessageToUserAsync(1, 2, new SendMessageRequestDTO { Content = "Secret" });
            var chat = await GetChatByParticipantsAsync(context, 1, 2);

            Assert.That(async () => await chatService.GetMessagesByChatIdAsync(chat.ChatId, 3),
                Throws.TypeOf<UnauthorizedAccessException>());
        }

        private ChatService CreateChatService(ApplicationDbContext context, Dictionary<int, string> userNames)
        {
            EnsureMongoConfigured();

            var databaseName = $"{_baseMongoSettings!.DatabaseName}_test_{Guid.NewGuid():N}";
            _createdDatabases.Add(databaseName);

            var mongoSettings = new MongoChatSettings
            {
                ConnectionString = _baseMongoSettings.ConnectionString,
                DatabaseName = databaseName,
                MessagesCollectionName = _baseMongoSettings.MessagesCollectionName
            };

            var repository = new MongoChatRepository(context, _mongoClient!, Options.Create(mongoSettings));

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "IntegrationTestSecretKey-0123456789",
                    ["JwtSettings:Issuer"] = "TestIssuer",
                    ["JwtSettings:Audience"] = "TestAudience"
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton(context)
                .BuildServiceProvider();

            var webSocketHandler = new WebSocketEventHandler(new WebSocketConnectionManager(), configuration, serviceProvider);

            var userServiceMock = new Mock<IUserService>();
            userServiceMock
                .Setup(s => s.GetProfileByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new GetProfileResponseDTO
                {
                    UserId = id,
                    Username = userNames.TryGetValue(id, out var name) ? name : $"User{id}",
                    BirthDate = DateTime.UtcNow
                });

            return new ChatService(repository, webSocketHandler, userServiceMock.Object);
        }

        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static async Task<Dictionary<int, string>> SeedUsersAsync(ApplicationDbContext context, params (int Id, string Email, string Username)[] users)
        {
            var lookup = new Dictionary<int, string>();

            foreach (var (id, email, username) in users)
            {
                var user = new User
                {
                    Id = id,
                    Email = email,
                    Username = username,
                    PasswordHash = "hashed",
                    Profile = new Profile
                    {
                        Id = id,
                        BirthDate = DateTime.UtcNow,
                        Description = "Test profile",
                        Region = "Test Region"
                    }
                };

                user.Profile.User = user;
                await context.Users.AddAsync(user);
                lookup[id] = username;
            }

            await context.SaveChangesAsync();
            return lookup;
        }

        private static async Task<Chat> GetChatByParticipantsAsync(ApplicationDbContext context, int firstUserId, int secondUserId)
        {
            var chat = await context.Chats
                .Include(c => c.UserChats)
                .FirstOrDefaultAsync(c =>
                    c.UserChats.Any(uc => uc.UserId == firstUserId) &&
                    c.UserChats.Any(uc => uc.UserId == secondUserId));

            if (chat == null)
            {
                throw new InvalidOperationException("Expected chat was not created.");
            }

            return chat;
        }

        private void EnsureMongoConfigured()
        {
            if (!_mongoAvailable || _mongoClient == null || _baseMongoSettings == null)
            {
                throw new InvalidOperationException("MongoDB integration not initialised. Ensure tests call SkipIfMongoUnavailable first.");
            }
        }

        private void SkipIfMongoUnavailable()
        {
            if (_mongoAvailable)
            {
                return;
            }

            Assert.Ignore($"MongoDB integration tests skipped: {_mongoUnavailableReason ?? "Unknown reason"}.");
        }
    }
}
