using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.Models
{
    public class HiddenSpot
    {
        [Key]
        public int id { get; set; }
        public string? ownername { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string? parkingname { get; set; }
        public string? hourrate { get; set; }
        public string? operatinghours { get; set; }
        public string? type { get; set; }
        public string? map { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public bool covered { get; set; }
        public bool evcharging { get; set; }
        public string? photo { get; set; }
        public bool verification { get; set; }
        public string? carspace { get; set; }
        public string? bikespace { get; set; }

    }
}
