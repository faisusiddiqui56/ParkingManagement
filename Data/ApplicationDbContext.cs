using Microsoft.EntityFrameworkCore;
using ParkingManagement.Models;

namespace ParkingManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<HiddenSpot> HiddenSpot { get; set; }
        public DbSet<ParkingOwner> ParkingOwner { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<dailyParking> DailyParking { get; set; }
    }
}
