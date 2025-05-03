namespace GameTogetherAPI.DTO {
    /// <summary>
    /// Represents the request data for updating a user profile.
    /// </summary>
    public class UpdateProfileRequestDTO {
        /// <summary>
        /// Gets or sets the birth date of the user.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the URL or base64 string of the user's profile picture.
        /// </summary>
        public string? ProfilePicture { get; set; }

        /// <summary>
        /// Gets or sets a brief description or bio of the user.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the region or location of the user.
        /// </summary>
        public string? Region { get; set; }
    }
}
