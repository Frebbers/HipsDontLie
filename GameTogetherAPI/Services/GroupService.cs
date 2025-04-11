using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Repository;

namespace GameTogetherAPI.Services {
    /// <summary>
    /// Provides group management services, including group creation, retrieval, joining, and leaving.
    /// </summary>
    public class GroupService : IGroupService {
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;

        /// <summary>
        /// Provides group management services, including group creation, retrieval, joining, and leaving.
        /// </summary>
        public GroupService(IChatRepository chatRepository, IUserRepository userRepository, IGroupRepository groupRepository) {
            _groupRepository = groupRepository;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        /// <summary>
        /// Creates a new group and assigns the user as its owner.
        /// </summary>
        /// <param name="userId">The unique identifier of the user creating the group.</param>
        /// <param name="groupDto">The group details provided in the request.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the group is successfully created.</returns>
        public async Task<bool> CreateGroupAsync(int userId, CreateGroupRequestDTO groupDto) {
            var group = new Group() {
                Title = groupDto.Title,
                AgeRange = groupDto.AgeRange,
                Description = groupDto.Description,
                IsVisible = groupDto.IsVisible,
                OwnerId = userId,
                MaxMembers = groupDto.MaxMembers,
                Tags = groupDto.Tags,
            };

            var savedGroup = await _groupRepository.CreateGroupAsync(group);

            if (savedGroup == null)
                return false;

            var userGroup = new UserGroup {
                UserId = userId,
                GroupId = savedGroup.Id,
                Status = UserGroupStatus.Accepted
            };
            await _groupRepository.AddUserToGroupAsync(userGroup);

            var chat = new Chat {
                GroupId = savedGroup.Id,
                UserChats = new List<UserChat> { new UserChat { UserId = userId } }
            };
            await _chatRepository.CreateGroupChatAsync(chat);

            return true;
        }

        /// <summary>
        /// Retrieves a group by its unique identifier and maps it to a response DTO.
        /// </summary>
        public async Task<GetGroupByIdResponseDTO> GetGroupByIdAsync(int groupId) {
            var group = await _groupRepository.GetGroupByIdAsync(groupId);

            return new GetGroupByIdResponseDTO() {
                Title = group.Title,
                AgeRange = group.AgeRange,
                Description = group.Description,
                IsVisible = group.IsVisible,
                OwnerId = groupId,
                Tags = group.Tags,
                Id = groupId,
                MaxMembers = group.MaxMembers,
                Members = group.Members
                    .Select(p => new MemberDTO {
                        UserId = p.UserId,
                        Name = p.User.Profile.Name,
                        GroupStatus = p.Status
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Retrieves all available groups or groups associated with a specific user.
        /// </summary>
        public async Task<List<GetGroupResponseDTO>> GetGroupsAsync(int? userId = null) {
            var groups = userId == null
                ? await _groupRepository.GetGroupsAsync()
                : await _groupRepository.GetGroupsByUserIdAsync((int)userId);

            if (groups == null)
                return null;

            var results = new List<GetGroupResponseDTO>();

            foreach (var group in groups) {
                results.Add(new GetGroupResponseDTO {
                    Id = group.Id,
                    Title = group.Title,
                    OwnerId = group.OwnerId,
                    IsVisible = group.IsVisible,
                    AgeRange = group.AgeRange,
                    Description = group.Description,
                    MaxMembers = group.MaxMembers,
                    Tags = group.Tags,
                    Members = group.Members
                        .Select(p => new MemberDTO {
                            UserId = p.UserId,
                            Name = p.User.Profile?.Name ?? "No profile",
                            GroupStatus = p.Status
                        })
                        .ToList(),
                    Chat = group.Chat != null ? new ChatDTO {
                        ChatId = group.Chat.ChatId,
                        SessionId = group.Chat.GroupId
                    } : null
                });
            }

            return results;
        }

        /// <summary>
        /// Allows a user to join a specified group if they are not already a participant.
        /// </summary>
        public async Task<bool> JoinGroupAsync(int userId, int groupId) {
            if (!await _groupRepository.ValidateUserGroupAsync(userId, groupId))
                return false;

            var userGroup = new UserGroup {
                UserId = userId,
                GroupId = groupId,
                Status = UserGroupStatus.Pending
            };

            await _groupRepository.AddUserToGroupAsync(userGroup);

            return true;
        }

        /// <summary>
        /// Allows a user to leave a specified group.
        /// </summary>
        public async Task<bool> LeaveGroupAsync(int userId, int groupId) {
            return await _groupRepository.RemoveUserFromGroupAsync(userId, groupId);
        }

        /// <summary>
        /// Accepts a pending user into a group if the requesting user is the owner of the group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to be accepted.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <param name="ownerId">The unique identifier of the group owner making the request.</param>
        /// <returns>
        /// A task representing the asynchronous operation.  
        /// Returns <c>true</c> if the user is successfully accepted and optionally added to the group's chat; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> AcceptUserInGroupAsync(int userId, int groupId, int ownerId) {
            if (!await _groupRepository.ValidateAcceptGroupAsync(userId, groupId, ownerId))
                return false;

            var userGroup = new UserGroup {
                UserId = userId,
                GroupId = groupId,
                Status = UserGroupStatus.Accepted
            };

            await _groupRepository.AddUserToGroupAsync(userGroup);

            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            if (group?.Chat != null) {
                var userChat = new UserChat {
                    UserId = userId,
                    ChatId = group.Chat.ChatId
                };

                await _chatRepository.AddUserToChatAsync(userChat);
            }

            return true;
        }

        /// <summary>
        /// Rejects a pending user from a group if the requesting user is the owner of the group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to be rejected.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <param name="ownerId">The unique identifier of the group owner making the request.</param>
        /// <returns>
        /// A task representing the asynchronous operation.  
        /// Returns <c>true</c> if the user is successfully rejected; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> RejectUserInGroupAsync(int userId, int groupId, int ownerId) {
            if (!await _groupRepository.ValidateAcceptGroupAsync(userId, groupId, ownerId))
                return false;

            var userGroup = new UserGroup {
                UserId = userId,
                GroupId = groupId,
                Status = UserGroupStatus.Rejected
            };

            await _groupRepository.AddUserToGroupAsync(userGroup);

            return true;
        }

    }
}
