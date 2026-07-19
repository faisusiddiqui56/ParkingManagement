using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.Models
{
    public class dailyParking
    {
        [Key]
        public int id { get; set; }
        public int? parkingid { get; set; }
        public string? ownername { get; set; }
        public string? vehiclenumber { get; set; }
        public string? type { get; set; }
        public string? intime { get; set; }
        public string? outtime { get; set; }
        //public string? totaltime { get; set; } 
        public string? amount { get; set; }
        public string? date { get; set; }
        public string? datetime { get; set; }
        public string? outdate { get; set; }
    }
}
