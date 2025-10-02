namespace HipsDontLie.Shared.DTO {
    /// <summary>
    /// Data Transfer Object representing a chat instance within the application.
    /// </summary>
    public class ChatDTO {
        /// <summary>
        /// The unique identifier of the chat.
        /// </summary>
        public int ChatId { get; set; }

        /// <summary>
        /// The identifier of the session associated with the chat, if any.
        /// </summary>
        public int? SessionId { get; set; }
    }
}
