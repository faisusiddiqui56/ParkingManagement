using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.Models
{
    public class Feedback
    {
        [Key]
        public int id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string? message { get; set; }
        public string? date { get; set; }
    }
}
