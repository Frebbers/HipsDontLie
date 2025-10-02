namespace HipsDontLie.DTO {
    /// <summary>
    /// Data Transfer Object representing a participant in a chat.
    /// </summary>
    public class ChatParticipantDTO {
        /// <summary>
        /// The unique identifier of the user participating in the chat.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The display name of the chat participant.
        /// </summary>
        public string Name { get; set; }
    }
}
