namespace GameTogetherAPI.DTO {
    /// <summary>
    /// Represents the data required to update a group.
    /// </summary>
    public class UpdateGroupRequestDTO {
        /// <summary>
        /// The new title of the group.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The age range suitable for the group.
        /// </summary>
        public string AgeRange { get; set; }

        /// <summary>
        /// A description of the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the group is publicly visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// The maximum number of members allowed in the group.
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>A list of tags associated with the group.</summary>
        public List<string> Tags { get; set; }

        /// <summary>A list of non-user member names (optional).</summary>
        public List<string>? NonUserMembers { get; set; }
    }
}
