using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.Models
{
    public class Admin
    {
        [Key]
        public int id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string name { get; set; }

    }
}
