using HipsDontLie.Models;
using HipsDontLie.Repository;
using HipsDontLie.Services;
using HipsDontLie.Shared.DTO;
using HipsDontLie.Shared.Enum;
using HipsDontLie.WebSockets;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HipsDontLie.Test {
    [TestFixture]
    public class GroupServiceTests {
        private Mock<IGroupRepository> _mockGroupRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IChatRepository> _mockChatRepo;
        private GroupService _service;

        [SetUp]
        public void Setup() {
            _mockGroupRepo = new Mock<IGroupRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockChatRepo = new Mock<IChatRepository>();
            _service = new GroupService(
                _mockChatRepo.Object,
                _mockUserRepo.Object,
                _mockGroupRepo.Object,
                new FakeWebSocketEventHandler()
            );
        }

        [Test]
        public async Task CreateGroupAsync_Success_ReturnsTrue() {
            var dto = new CreateGroupRequestDTO { Title = "Test", MaxMembers = 10, Description = "desc" };
            _mockGroupRepo.Setup(r => r.CreateGroupAsync(It.IsAny<Group>()))
                .ReturnsAsync(new Group { Id = 1 });
            _mockGroupRepo.Setup(r => r.AddUserToGroupAsync(It.IsAny<UserGroup>())).ReturnsAsync(true);
            _mockChatRepo.Setup(r => r.CreateGroupChatAsync(It.IsAny<Chat>())).ReturnsAsync(true);

            var result = await _service.CreateGroupAsync(1, dto);

            Assert.IsTrue(result);
            _mockGroupRepo.Verify(r => r.CreateGroupAsync(It.IsAny<Group>()), Times.Once);
        }

        [Test]
        public async Task CreateGroupAsync_Failure_ReturnsFalse() {
            _mockGroupRepo.Setup(r => r.CreateGroupAsync(It.IsAny<Group>())).ReturnsAsync((Group)null);
            var result = await _service.CreateGroupAsync(1, new CreateGroupRequestDTO());
            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetGroupByIdAsync_ReturnsMappedDto() {
            var group = new Group {
                Id = 5,
                Title = "Group A",
                OwnerId = 1,
                Members = new List<UserGroup> { new UserGroup { UserId = 2, User = new User { Username = "Bob" }, Status = SharedEnums.UserGroupStatus.Accepted } },
                Chat = new Chat { GroupId = 5, ChatId = 99 }
            };
            _mockGroupRepo.Setup(r => r.GetGroupByIdAsync(5)).ReturnsAsync(group);

            var dto = await _service.GetGroupByIdAsync(5);

            Assert.AreEqual("Group A", dto.Title);
            Assert.AreEqual(99, dto.Chat.ChatId);
        }

        [Test]
        public async Task JoinGroupAsync_RequesterNotFound_ReturnsRequesterNotFound() {
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);
            var result = await _service.JoinGroupAsync(1, 1);
            Assert.AreEqual(JoinGroupStatus.RequesterNotFound, result);
        }

        [Test]
        public async Task JoinGroupAsync_AlreadyMember_ReturnsAlreadyMember() {
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(new User { Id = 1 });
            _mockGroupRepo.Setup(r => r.ValidateUserGroupAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            var result = await _service.JoinGroupAsync(1, 1);

            Assert.AreEqual(JoinGroupStatus.AlreadyMember, result);
        }

        [Test]
        public async Task JoinGroupAsync_GroupNotFound_ReturnsGroupNotFound() {
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(new User { Id = 1 });
            _mockGroupRepo.Setup(r => r.ValidateUserGroupAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockGroupRepo.Setup(r => r.GetGroupByIdAsync(It.IsAny<int>())).ReturnsAsync((Group)null);

            var result = await _service.JoinGroupAsync(1, 2);

            Assert.AreEqual(JoinGroupStatus.GroupNotFound, result);
        }

        [Test]
        public async Task JoinGroupAsync_Success_ReturnsSuccess() {
            var requester = new User { Id = 1, Username = "User" };
            var group = new Group { Id = 2, Title = "Test Group", OwnerId = 10 };
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(requester);
            _mockGroupRepo.Setup(r => r.ValidateUserGroupAsync(1, 2)).ReturnsAsync(true);
            _mockGroupRepo.Setup(r => r.GetGroupByIdAsync(2)).ReturnsAsync(group);
            _mockGroupRepo.Setup(r => r.AddUserToGroupAsync(It.IsAny<UserGroup>())).ReturnsAsync(true);

            var result = await _service.JoinGroupAsync(1, 2);

            Assert.AreEqual(JoinGroupStatus.Success, result);
        }

        [Test]
        public async Task LeaveGroupAsync_DelegatesToRepo() {
            _mockGroupRepo.Setup(r => r.RemoveUserFromGroupAsync(1, 2)).ReturnsAsync(true);
            var result = await _service.LeaveGroupAsync(1, 2);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task AcceptUserInGroupAsync_Valid_ReturnsTrue() {
            _mockGroupRepo.Setup(r => r.ValidateAcceptGroupAsync(2, 3, 1)).ReturnsAsync(true);
            _mockGroupRepo.Setup(r => r.GetGroupByIdAsync(3)).ReturnsAsync(new Group {
                Id = 3,
                Title = "G",
                OwnerId = 1,
                Chat = new Chat { ChatId = 5, GroupId = 3 }
            });

            var result = await _service.AcceptUserInGroupAsync(2, 3, 1);

            Assert.IsTrue(result);
            _mockChatRepo.Verify(r => r.AddUserToChatAsync(It.IsAny<UserChat>()), Times.Once);
        }

        [Test]
        public async Task RejectUserInGroupAsync_Valid_ReturnsTrue() {
            _mockGroupRepo.Setup(r => r.ValidateAcceptGroupAsync(2, 3, 1)).ReturnsAsync(true);
            var result = await _service.RejectUserInGroupAsync(2, 3, 1);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task UpdateGroupAsync_NotOwner_ReturnsFalse() {
            _mockGroupRepo.Setup(r => r.GetGroupByIdAsync(1))
                .ReturnsAsync(new Group { OwnerId = 99 });
            var result = await _service.UpdateGroupAsync(1, 1, new UpdateGroupRequestDTO());
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UpdateGroupAsync_OwnerValid_ReturnsTrue() {
            _mockGroupRepo.Setup(r => r.GetGroupByIdAsync(1))
                .ReturnsAsync(new Group { Id = 1, OwnerId = 1 });
            _mockGroupRepo.Setup(r => r.UpdateGroupAsync(1, It.IsAny<Group>())).ReturnsAsync(true);

            var result = await _service.UpdateGroupAsync(1, 1, new UpdateGroupRequestDTO { Title = "New" });
            Assert.IsTrue(result);
        }
    }
}

public class FakeWebSocketEventHandler : WebSocketEventHandler {
    public FakeWebSocketEventHandler()
        : base(new WebSocketConnectionManager(), new ConfigurationBuilder().Build(), null) { }

    public new Task SendPendingJoinRequestAsync(int ownerId, int groupId, int requesterId, string requesterName, string groupTitle) {
        return Task.CompletedTask;
    }
}
