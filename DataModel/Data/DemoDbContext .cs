namespace OData.Data
{
    using Microsoft.EntityFrameworkCore;
    using OData.Models;
    
    public class DemoDbContext : DbContext
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options)
            : base(options)
        {
        }

        // Must match the table name inside database
        // Default to dbo namespace
        public DbSet<Country> Country { get; set; }
        public DbSet<Airport> Airport { get; set; }
        public DbSet<AirportMetric> AirportMetric { get; set; }
    }
}
