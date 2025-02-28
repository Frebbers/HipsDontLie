using System.ComponentModel.DataAnnotations;

namespace GameTogetherAPI.Models
{
    public class RegisterModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
