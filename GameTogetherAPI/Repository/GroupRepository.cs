using GameTogetherAPI.Database;
using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Repository {
    /// <summary>
    /// Handles database operations related to game groups.
    /// </summary>
    public class GroupRepository : IGroupRepository {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRepository"/> class.
        /// </summary>
        /// <param name="context">The database context for interacting with groups.</param>
        public GroupRepository(ApplicationDbContext context) {
            _context = context;
        }

        /// <summary>
        /// Creates a new group in the database.
        /// </summary>
        /// <param name="group">The group to be created.</param>
        /// <returns>A task that represents the asynchronous operation, returning the created group.</returns>
        public async Task<Group> CreateGroupAsync(Group group) {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            return group;
        }

        /// <summary>
        /// Retrieves a group by its unique identifier, including its participants and their profiles.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task representing the asynchronous operation, returning the group if found, otherwise null.</returns>
        public async Task<Group> GetGroupByIdAsync(int groupId) {
            return await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        /// <summary>
        /// Adds a user to a group and saves the change to the database.
        /// </summary>
        /// <param name="userGroup">The user-group relationship to be added.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the user is successfully added to the group.</returns>
        public async Task<bool> AddUserToGroupAsync(UserGroup userGroup) {
            var exists = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.UserId == userGroup.UserId && ug.GroupId == userGroup.GroupId);

            if (exists != null) {
                exists.Status = userGroup.Status;
                _context.UserGroups.Update(exists);
            }
            else {
                await _context.UserGroups.AddAsync(userGroup);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a user from a group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the user is successfully removed, otherwise false.</returns>
        public async Task<bool> RemoveUserFromGroupAsync(int userId, int groupId) {
            var userGroup = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup == null)
                return false;

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.OwnerId == userId);

            if (group != null) {
                _context.Groups.Remove(group);
            }
            else {
                _context.UserGroups.Remove(userGroup);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates whether a user is not already a participant in a group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the user is not already a participant, otherwise false.</returns>
        public async Task<bool> ValidateUserGroupAsync(int userId, int groupId) {
            return await _context.Groups
                .Where(g => g.Id == groupId)
                .Select(g => !_context.UserGroups.Any(ug => ug.UserId == userId && ug.GroupId == groupId))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Validates if the group owner can accept a pending user.
        /// </summary>
        public async Task<bool> ValidateAcceptGroupAsync(int userId, int groupId, int ownerId) {
            return await _context.Groups
                .Where(g => g.Id == groupId && g.OwnerId == ownerId)
                .Select(g => _context.UserGroups.Any(ug => ug.UserId == userId && ug.GroupId == groupId && ug.Status == UserGroupStatus.Pending))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all groups a user is participating in.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of groups the user is part of.</returns>
        public async Task<List<Group>> GetGroupsByUserIdAsync(int userId) {
            return await _context.Groups
                .Where(g => g.Members.Any(p => p.UserId == userId))
                .Include(g => g.Members)
                    .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Profile)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all available groups.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a list of all groups.</returns>
        public async Task<List<Group>> GetGroupsAsync() {
            return await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Profile)
                .ToListAsync();
        }
    }
}
