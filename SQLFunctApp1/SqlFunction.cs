namespace SQLFunctApp1
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Sql;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using OData.Data;
    using OData.Models;
    using System.Collections.Generic;
    public class SqlFunction
    {
        private readonly DemoDbContext _db;

        public SqlFunction(DemoDbContext db)
        {
            this._db = db; 
        }

        [FunctionName("CountryInsertTrigger")]
        public void Run(
            [SqlTrigger("[dbo].[Country]", "SQL_CONN_STR")]
            IReadOnlyList<SqlChange<Country>> changes,
            ILogger logger)
        {
            foreach (SqlChange<Country> change in changes)
            {
                if (change.Operation != SqlChangeOperation.Insert)
                {
                    continue;
                }
                Country fc = _db.Country.FirstOrDefaultAsync().GetAwaiter().GetResult();
                logger.LogInformation($"{fc.Name}");
                var c = change.Item;
                logger.LogInformation($"Change operation: {change.Operation}");
                logger.LogInformation($"Id: {c.ID}, Title: {c.Name}");
            }
        }
    }
}
