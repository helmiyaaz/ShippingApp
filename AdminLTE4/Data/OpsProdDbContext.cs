using WeeklyShipPlan.Models;
using Microsoft.EntityFrameworkCore;
using FILog.Models;

namespace FILog.Data
{
    public class OpsProdDbContext : DbContext
    {
        public OpsProdDbContext(DbContextOptions<OpsProdDbContext> options) : base(options)
        {
            try
            {
                Database.CanConnect();
                Console.WriteLine("Database connection successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection failed: {ex.Message}");
                throw;
            }
        }
        public DbSet<WeeklyShipPlanModel> MaterialMaster { get; set; } = null!;
        public DbSet<MasterShipPlanModel> MasterShipPlanModels { get; set; } = null!;
        public DbSet<ShipmentLogModel> ShipmentLogModels { get; set; } = null !;
    }
}
