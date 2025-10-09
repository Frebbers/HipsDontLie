using HipsDontLie.Models;
using HipsDontLie.Repository;
using HipsDontLie.Services;
using HipsDontLie.Shared.DTO;
using Moq;

namespace HipsDontLie.Test {
    [TestFixture]
    public class UserServiceTests {
        private Mock<IUserRepository> _mockRepo;
        private UserService _service;

        [SetUp]
        public void Setup() {
            _mockRepo = new Mock<IUserRepository>();
            _service = new UserService(_mockRepo.Object);
        }

        [Test]
        public async Task AddOrUpdateProfileAsync_InvalidBirthDate_ReturnsInvalidBirthDate() {
            var dto = new UpdateProfileRequestDTO { BirthDate = DateTime.UtcNow.AddYears(-5) };
            var result = await _service.AddOrUpdateProfileAsync(1, dto);
            Assert.That(result, Is.EqualTo(UpdateProfileStatus.InvalidBirthDate));
        }

        [Test]
        public async Task AddOrUpdateProfileAsync_InvalidDescription_ReturnsInvalidDescription() {
            var dto = new UpdateProfileRequestDTO {
                BirthDate = DateTime.UtcNow.AddYears(-20),
                Description = "visit http://malicious.link"
            };
            var result = await _service.AddOrUpdateProfileAsync(1, dto);
            Assert.That(result, Is.EqualTo(UpdateProfileStatus.InvalidDescription));
        }

        [Test]
        public async Task AddOrUpdateProfileAsync_ValidProfile_ReturnsSuccess() {
            var dto = new UpdateProfileRequestDTO {
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Description = "Valid description",
                Region = "North"
            };
            _mockRepo.Setup(r => r.AddOrUpdateProfileAsync(It.IsAny<Profile>())).ReturnsAsync(true);

            var result = await _service.AddOrUpdateProfileAsync(1, dto);

            Assert.That(result, Is.EqualTo(UpdateProfileStatus.Success));
            _mockRepo.Verify(r => r.AddOrUpdateProfileAsync(It.Is<Profile>(p => p.Id == 1)), Times.Once);
        }

        [Test]
        public async Task AddOrUpdateProfileAsync_FailsInRepo_ReturnsUnknownFailure() {
            var dto = new UpdateProfileRequestDTO {
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Description = "ok"
            };
            _mockRepo.Setup(r => r.AddOrUpdateProfileAsync(It.IsAny<Profile>())).ReturnsAsync(false);

            var result = await _service.AddOrUpdateProfileAsync(1, dto);

            Assert.That(result, Is.EqualTo(UpdateProfileStatus.UnknownFailure));
        }

        [Test]
        public async Task GetProfileAsync_ReturnsMappedDto() {
            var user = new User { Id = 2, Username = "John" };
            var profile = new Profile {
                User = user,
                BirthDate = new DateTime(2000, 1, 1),
                Description = "Cool",
                Region = "West",
                ProfilePicture = "pic.png"
            };
            _mockRepo.Setup(r => r.GetProfileAsync(2)).ReturnsAsync(profile);

            var result = await _service.GetProfileAsync(2);

            Assert.That(result.Username, Is.EqualTo("John"));
            Assert.That(result.Region, Is.EqualTo("West"));
        }

        [Test]
        public async Task GetUserIdByEmailAsync_ReturnsUserId() {
            _mockRepo.Setup(r => r.GetUserByEmailAsync("test@example.com"))
                .ReturnsAsync(new User { Id = 42 });

            var result = await _service.GetUserIdByEmailAsync("test@example.com");

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public async Task GetUserIdByEmailAsync_UserNotFound_ReturnsNull() {
            _mockRepo.Setup(r => r.GetUserByEmailAsync("missing@example.com")).ReturnsAsync((User)null);

            var result = await _service.GetUserIdByEmailAsync("missing@example.com");

            Assert.That(result, Is.Null);
        }
    }
}
