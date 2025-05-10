using GameTogetherAPI.DTO;
using GameTogetherAPI.Models;
using GameTogetherAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameTogetherAPI.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Manages game groups, including group creation, retrieval, joining, and leaving.
    /// </summary>
    public class GroupsController : ControllerBase {
        private readonly IGroupService _groupService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsController"/> class.
        /// </summary>
        /// <param name="groupService">The group service responsible for handling group-related operations.</param>
        public GroupsController(IGroupService groupService) {
            _groupService = groupService;
        }

        /// <summary>
        /// Creates a new group for the authenticated user.
        /// </summary>
        /// <param name="groupDto">The group details provided in the request body.</param>
        /// <returns>
        /// Returns a 201 Created response if the group is successfully created.  
        /// Returns a 400 Bad Request response if the group creation fails.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequestDTO groupDto) {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _groupService.CreateGroupAsync(userId, groupDto);

            if (!success)
                return BadRequest(new { message = "Failed to create group." });

            return Created(string.Empty, new { message = "Group created successfully!" });
        }

        /// <summary>
        /// Retrieves all available groups.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response with a list of groups.  
        /// Returns a 404 Not Found response if no groups are available.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetGroupsAsync() {
            var groups = await _groupService.GetGroupsAsync();

            if (groups == null)
                return NotFound(new { message = "Groups not found" });

            return Ok(groups);
        }

        /// <summary>
        /// Retrieves all groups that the specified user is participating in.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// A 200 OK response with the list of groups.  
        /// A 404 Not Found response if no public groups are found.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetGroupsByUserIdAsync(int userId) {
            var groups = await _groupService.GetGroupsByUserIdAsync(userId);

            if (groups == null || !groups.Any())
                return NotFound(new { message = "No groups found for this user." });

            var visibleGroups = groups.Where(g => g.IsVisible).ToList();

            if (!visibleGroups.Any())
                return NotFound(new { message = "No public groups found for this user." });

            return Ok(visibleGroups);
        }


        /// <summary>
        /// Retrieves a group by its unique identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>
        /// A 200 OK response containing the group details if found.  
        /// A 404 Not Found response if the group does not exist.
        /// </returns>
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupByIdAsync(int groupId) {
            var group = await _groupService.GetGroupByIdAsync(groupId);

            if (group == null)
                return NotFound(new { message = "Group not found" });

            return Ok(group);
        }

        /// <summary>
        /// Retrieves all groups that the authenticated user is participating in.
        /// </summary>
        /// <returns>
        /// A 200 OK response containing the list of groups the user is part of.  
        /// A 404 Not Found response if no groups are found.
        /// </returns>
        [HttpGet("user")]
        public async Task<IActionResult> GetMyGroupsAsync() {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var groups = await _groupService.GetGroupsAsync(userId);

            if (groups == null)
                return NotFound(new { message = "Groups not found" });

            return Ok(groups);
        }

        /// <summary>
        /// Allows the authenticated user to join a specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group to join.</param>
        /// <returns>
        /// Returns a 200 OK response if the user successfully joins the group.  
        /// Returns a 400 Bad Request response if the group does not exist or the user is already a participant.
        /// </returns>
        [HttpPost("{groupId}/join")]
        public async Task<IActionResult> JoinGroup(int groupId) {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

JoinGroupStatus status = await _groupService.JoinGroupAsync(userId, groupId);
        switch (status)
        {
            case JoinGroupStatus.Success:
                return Ok(new { message = "Group joined successfully!" });
            case JoinGroupStatus.GroupNotFound:
                return BadRequest(new { message = "Group does not exist." });
            case JoinGroupStatus.AlreadyMember:
                return BadRequest(new { message = "You are already a member of this group." });
            case JoinGroupStatus.GroupFull:
                return BadRequest(new { message = "Group is full." });
            default:
                return BadRequest(new { message = "Unknown failure occurred while joining group." });
        }
        
        }

        /// <summary>
        /// Accept a pending user if you are the owner of the group.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// Returns a 200 OK response if the user has been accepted into the group successfully.  
        /// Returns a 400 Bad Request response if the group does not exist or the user is not a participant.
        /// </returns>
        [HttpGet("{groupId}/{userId}/accept")]
        public async Task<IActionResult> AcceptUserInGroupAsync(int groupId, int userId) {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _groupService.AcceptUserInGroupAsync(userId, groupId, ownerId);

            if (!success)
                return BadRequest(new { message = "Failed to accept user." });

            return Ok(new { message = "Successfully accepted the user!" });
        }

        /// <summary>
        /// Reject a pending user if you are the owner of the group.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// Returns a 200 OK response if the user has been rejected from the group successfully.  
        /// Returns a 400 Bad Request response if the group does not exist or the user is not a participant.
        /// </returns>
        [HttpGet("{groupId}/{userId}/reject")]
        public async Task<IActionResult> RejectUserInGroupAsync(int groupId, int userId) {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _groupService.RejectUserInGroupAsync(userId, groupId, ownerId);

            if (!success)
                return BadRequest(new { message = "Failed to reject user." });

            return Ok(new { message = "Successfully rejected the user!" });
        }

        /// <summary>
        /// Allows the authenticated user to leave a specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group to leave.</param>
        /// <returns>
        /// Returns a 200 OK response if the user successfully leaves the group.  
        /// Returns a 400 Bad Request response if the group does not exist or the user has already left.
        /// </returns>
        [HttpDelete("{groupId}/leave")]
        public async Task<IActionResult> LeaveGroup(int groupId) {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            bool success = await _groupService.LeaveGroupAsync(userId, groupId);

            if (!success)
                return BadRequest(new { message = "Failed to leave group. Either it does not exist or you've already left." });

            return Ok(new { message = "Successfully left the group!" });
        }
    }
}
