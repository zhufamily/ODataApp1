namespace ODataApp1
{
    using Microsoft.AspNetCore.OData;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.OData.ModelBuilder;
    using OData.Data;
    using OData.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // add odata package
            var modelBuilder = new ODataConventionModelBuilder();
            // add controller must match controller name
            modelBuilder.EntitySet<Country>("Countries");
            modelBuilder.EntitySet<Airport>("Airports");
            modelBuilder.EntitySet<AirportMetric>("AirportMetrics");

            // register odata filter capabilities
            // default to all 
            // without limit for $top
            builder.Services.AddControllers().AddOData(
                options => options.EnableQueryFeatures(null).AddRouteComponents(
                    "odata",
                    modelBuilder.GetEdmModel()));

            // read db connection string from appsettings.json
            // at the top level, could be moved to any levels
            var configValue = builder.Configuration.GetValue<string>("DemoDbConnectionString");
            
            builder.Services.AddDbContext<DemoDbContext>(options => options.UseSqlServer(configValue));

            var app = builder.Build();

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.Run();
        }
    }
}
