using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.Models
{
    public class ParkingOwner
    {
        [Key]
        public int id { get; set; }
        public int? parkingid { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
    }
}
