namespace GameTogetherAPI.DTO
{
    /// <summary>
    /// Represents the data required to create a new game group.
    /// </summary>
    public class CreateGroupRequestDTO
    {
        /// <summary>
        /// The title of the group.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Indicates whether the group is visible to others.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// The recommended age range for participants.
        /// </summary>
        public string AgeRange { get; set; }

        /// <summary>
        /// A brief description of the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The maximum number of members allowed in the group.
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// A list of tags associated with the group for filtering and categorization.
        /// </summary>
        public List<string> Tags { get; set; } = new();
        public List<string>? NonUserMembers { get; set; } = new();
    }
}
