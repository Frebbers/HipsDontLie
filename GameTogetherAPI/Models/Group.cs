using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.Models {
    /// <summary>
    /// Represents a game group that users can join.
    /// </summary>
    public class Group {
        /// <summary>
        /// Gets or sets the unique identifier of the group.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the group.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the group owner.
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns the group.
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is visible to others.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the recommended age range for participants.
        /// </summary>
        public string AgeRange { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of members allowed in the group.
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// Gets or sets the list of members in the group.
        /// </summary>
        public List<UserGroup> Members { get; set; } = new();

        /// <summary>
        /// Gets or sets a list of tags associated with the group for filtering and categorization.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        public Chat Chat { get; set; }
    }
}
