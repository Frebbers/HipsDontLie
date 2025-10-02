using HipsDontLie.Shared.DTO;
using HipsDontLie.Models;

namespace HipsDontLie.Services {
    /// <summary>
    /// Defines the contract for group management services.
    /// </summary>
    public interface IGroupService {
        /// <summary>
        /// Creates a new group for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user creating the group.</param>
        /// <param name="group">The group details provided in the request.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the group is successfully created.</returns>
        Task<bool> CreateGroupAsync(int userId, CreateGroupRequestDTO group);

        /// <summary>
        /// Retrieves a group by its unique identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task representing the asynchronous operation, returning the group details if found.</returns>
        Task<GetGroupByIdResponseDTO> GetGroupByIdAsync(int groupId);

        /// <summary>
        /// Retrieves all available groups or groups associated with a specific user.
        /// </summary>
        /// <param name="userId">The optional user ID to filter groups by user participation. If null, all groups are retrieved.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of available groups.</returns>
        Task<List<GetGroupResponseDTO>> GetGroupsAsync(int? userId = null);

        /// <summary>
        /// Retrieves all groups that a specific user is participating in.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of groups the user is part of.</returns>
        Task<List<GetGroupResponseDTO>> GetGroupsByUserIdAsync(int userId);


        /// <summary>
        /// Allows a user to join a specified group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user joining the group.</param>
        /// <param name="groupId">The unique identifier of the group to join.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the user successfully joins the group.</returns>
        Task<JoinGroupStatus> JoinGroupAsync(int userId, int groupId);

        /// <summary>
        /// Allows a user to leave a specified group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user leaving the group.</param>
        /// <param name="groupId">The unique identifier of the group to leave.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the user successfully leaves the group.</returns>
        Task<bool> LeaveGroupAsync(int userId, int groupId);

        /// <summary>
        /// Accepts a pending user into a group if the requester is the group owner.
        /// </summary>
        Task<bool> AcceptUserInGroupAsync(int userId, int groupId, int ownerId);

        /// <summary>
        /// Rejects a pending user from a group if the requester is the group owner.
        /// </summary>
        Task<bool> RejectUserInGroupAsync(int userId, int groupId, int ownerId);

        /// <summary>
        /// Updates the details of a group if the user is the group owner.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group to update.</param>
        /// <param name="userId">The unique identifier of the user performing the update.</param>
        /// <param name="dto">The updated group data.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the update is successful.</returns>
        Task<bool> UpdateGroupAsync(int groupId, int userId, UpdateGroupRequestDTO dto);
    }
}
