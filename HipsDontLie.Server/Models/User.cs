using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HipsDontLie.Models {
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User : IdentityUser<int> 
    {
        /// <summary>
        /// Gets or sets the profile associated with the user.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets the list of groups the user has joined.
        /// </summary>
        public List<UserGroup> JoinedGroups { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of messages sent by the user.
        /// </summary>
        public List<Message> SentMessages { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of chats the user is participating in.
        /// </summary>
        public List<UserChat> Chats { get; set; } = new();
    }
}
