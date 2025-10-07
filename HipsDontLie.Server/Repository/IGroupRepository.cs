using HipsDontLie.Models;

namespace HipsDontLie.Repository {
    /// <summary>
    /// Defines the contract for group-related database operations.
    /// </summary>
    public interface IGroupRepository {
        /// <summary>
        /// Creates a new group asynchronously.
        /// </summary>
        /// <param name="group">The group to create.</param>
        /// <returns>A task that represents the asynchronous operation, returning the created group.</returns>
        Task<Group> CreateGroupAsync(Group group);

        /// <summary>
        /// Adds a user to a group asynchronously.
        /// </summary>
        /// <param name="userGroup">The user-group relationship to add.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the user is successfully added to the group.</returns>
        Task<bool> AddUserToGroupAsync(UserGroup userGroup);

        /// <summary>
        /// Retrieves all available groups asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, returning a list of all groups.</returns>
        Task<List<Group>> GetGroupsAsync();

        /// <summary>
        /// Retrieves all groups that a specific user is participating in asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that represents the asynchronous operation, returning a list of groups the user is part of.</returns>
        Task<List<Group>> GetGroupsByUserIdAsync(int userId);

        /// <summary>
        /// Validates whether a user is part of a specific group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the user is part of the group.</returns>
        Task<bool> ValidateUserGroupAsync(int userId, int groupId);

        /// <summary>
        /// Removes a user from a group asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the user is successfully removed from the group.</returns>
        Task<bool> RemoveUserFromGroupAsync(int userId, int groupId);

        /// <summary>
        /// Retrieves a group by its unique identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task representing the asynchronous operation, returning the group if found, otherwise null.</returns>
        Task<Group> GetGroupByIdAsync(int? groupId);

        /// <summary>
        /// Validates whether the group owner can accept a user into the group.
        /// </summary>
        /// <param name="userId">The ID of the user to be accepted.</param>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="ownerId">The ID of the group owner.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the owner can accept the user.</returns>
        Task<bool> ValidateAcceptGroupAsync(int userId, int groupId, int ownerId);

        /// <summary>
        /// Updates the details of a group in the database.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group to update.</param>
        /// <param name="updatedGroup">The updated group entity with new values.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the update is successful.</returns>
        Task<bool> UpdateGroupAsync(int groupId, Group updatedGroup);
    }
}
