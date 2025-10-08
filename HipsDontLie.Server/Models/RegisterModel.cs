using System.ComponentModel.DataAnnotations;

namespace HipsDontLie.Models {
    /// <summary>
    /// Represents the data required for user registration.
    /// </summary>
    public class RegisterModel {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for the user account.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
