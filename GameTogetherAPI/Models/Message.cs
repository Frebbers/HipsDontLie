using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace GameTogetherAPI.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public int ChatId { get; set; } 
        public Chat Chat { get; set; }
        public int? SenderId { get; set; }
        public User? Sender { get; set; }      
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
