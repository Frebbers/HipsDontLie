namespace GameTogetherAPI.Models {
    /// <summary>
    /// Represents the relationship between a user and a group, indicating that a user has joined a group.
    /// </summary>
    public class UserGroup {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the group.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the group.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the status of the participant.
        /// </summary>
        public UserGroupStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the group associated with the user.
        /// </summary>
        public Group Group { get; set; }
    }
}

public enum UserGroupStatus
{
    Pending,
    Accepted,
    Rejected
}