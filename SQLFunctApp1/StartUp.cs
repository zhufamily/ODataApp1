using Microsoft.Azure.Functions.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(SQLFunctApp1.StartUp))]

namespace SQLFunctApp1
{
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OData.Data;
    using System;

    public class StartUp : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("SQL_CONN_STR");
            builder.Services.AddDbContext<DemoDbContext>(
              options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));  
        }
    }
}
