using FishingIndustry.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FishingIndustry.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vessel> Vessels { get; set; }
        public DbSet<FishType> FishTypes { get; set; }
        public DbSet<FishingZone> FishingZones { get; set; }
    }
}
