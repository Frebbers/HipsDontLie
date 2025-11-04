using HipsDontLie.Models;
using System.ComponentModel.DataAnnotations;

namespace HipsDontLie.Server.Models {
    public class Appointment {

        [Key]
        public int id { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Text { get; set; }
    }
}

