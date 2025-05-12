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
        /// <returns>A task that represents the asynchronous operation, returning the saved group.</returns>
        public async Task<Group> CreateGroupAsync(Group group) {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            return group;
        }

        /// <summary>
        /// Retrieves a group by its unique identifier, including its participants and their profiles.
        /// </summary>
        public async Task<Group> GetGroupByIdAsync(int? groupId) {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(p => p.User)
                .Include(g => g.Chat)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        /// <summary>
        /// Adds a user to a group and saves the change to the database.
        /// </summary>
        public async Task<bool> AddUserToGroupAsync(UserGroup userGroup) {
            var exists = await _context.UserGroups.FirstOrDefaultAsync(
                ug => ug.UserId == userGroup.UserId && ug.GroupId == userGroup.GroupId
            );

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
        public async Task<bool> RemoveUserFromGroupAsync(int userId, int groupId) {
            var userGroup = await _context.UserGroups.FirstOrDefaultAsync(
                ug => ug.UserId == userId && ug.GroupId == groupId
            );

            if (userGroup == null)
                return false;

            // Check if the user is the owner of the group
            var group = await _context.Groups.FirstOrDefaultAsync(
                g => g.Id == groupId && g.OwnerId == userId
            );

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
        public async Task<bool> ValidateUserGroupAsync(int userId, int groupId) {
            return await _context.Groups
                .Where(g => g.Id == groupId)
                .Select(g => !_context.UserGroups.Any(ug => ug.UserId == userId && ug.GroupId == groupId))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Validates whether a group owner can accept a pending user.
        /// </summary>
        public async Task<bool> ValidateAcceptGroupAsync(int userId, int groupId, int ownerId) {
            return await _context.Groups
                .Where(g => g.Id == groupId && g.OwnerId == ownerId)
                .Select(g => _context.UserGroups.Any(
                    ug => ug.UserId == userId && ug.GroupId == groupId && ug.Status == UserGroupStatus.Pending
                ))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all groups that a user is participating in.
        /// </summary>
        public async Task<List<Group>> GetGroupsByUserIdAsync(int userId) {
            return await _context.Groups
                .Where(g => g.Members.Any(p => p.UserId == userId))
                .Include(g => g.Members)
                    .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Include(g => g.Chat)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all available groups.
        /// </summary>
        public async Task<List<Group>> GetGroupsAsync() {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Include(g => g.Chat)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing group with new values.
        /// </summary>
        /// <param name="groupId">The ID of the group to update.</param>
        /// <param name="updatedGroup">The updated group values.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateGroupAsync(int groupId, Group updatedGroup) {
            var existingGroup = await _context.Groups.FindAsync(groupId);
            if (existingGroup == null)
                return false;

            existingGroup.Title = updatedGroup.Title;
            existingGroup.Description = updatedGroup.Description;
            existingGroup.IsVisible = updatedGroup.IsVisible;
            existingGroup.MaxMembers = updatedGroup.MaxMembers;
            existingGroup.AgeRange = updatedGroup.AgeRange;
            existingGroup.Tags = updatedGroup.Tags;
            existingGroup.NonUserMembers = updatedGroup.NonUserMembers;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
