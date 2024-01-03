<h1>A tutorial for SQL Server OData endpoint with Entity Framework Core</h1>
This is a brief tutorial for SQL Sever as backend, OData through http(s) as frontend, with Entity Framework Core as middle-tier.
<h2>SQL Server</h2>
The first step is to set up some sample tables inside a SQL Server database.  I know, I know lots of you might scream for a code first approach.  However, in a real enterprise or big development environment, database is usually controlled by DB administrators rather than developers, and further more, outside development environment, frontend application codes might have limited or no access to DDL but only full access to DML.  So, I try to simulate the real world senario rather then some conceptional design approaches.  Please refer to https://github.com/zhufamily/ODataApp1/blob/main/demoTables.sql for more code details. 
<h3>Tables</h3>
<ol>
<li>Country table -- parent table for Aiport table (one-to-many).</li>
<li>Airport table -- child table of Country table, and parent table for AirportMetric table (one-to-one).</li>
<li>AirportMetric table -- child table for Airport table.</li>
</ol>  
<h2>.Net Core Project for Data Models and Data Context</h2>
Next, we are going to set up a .NET Core Library project for data models and data context.  The reason we setup a separate project is that we can reuse that in the future, e.g., OData endpoint, or SQL related Azure Function, or web application frontend and etc.  
<h3>Models</h3>
If you are inside Visual Studio, there is a GUI tool, you can download here -- https://marketplace.visualstudio.com/items?itemName=ErikEJ.EFCorePowerTools, at the moment, it supportes Entity Framework Core 6-8.  Otherwise, you can refer to command line tools https://learn.microsoft.com/en-us/ef/core/cli/ or simply setup POCO classes manually.  Please refer to https://github.com/zhufamily/ODataApp1/tree/main/DataModel for more details.  Create a folder called "Models" and for every table, there will be a corresponding POCO class; and inside POCO class database fields are mapped to class attributes.
<h4>Primary Key</h4>
For primary key field, just annotate with "[Key]" attribute.
<code>
[Key] // primary key
public int ID { get; set; }
</code>
<h4>One-to-many relationship</h4>
This is the most common relationship inside relational database, which can also cover many-to-many relationship with two one-to-many relationships.  It is quite easy to implement, from parent side, it will be a virtual iCollection.
<code>
public virtual ICollection<ChildType> Children { get; set; }
</code>
On the child side, it will be a virtual parent object.
<code>
[ForeignKey("ParentType")] // foreign key
public int ParentID { get; set; }
public virtual ParentType Parent { get; set; }  
</code>
<h4>One-to-one relationship</h4>
One to one relationship is rarely used, but useful in certain situations, e.g., partial data are sensitive so more access control will be needed.  Usually, one entity is considered as base (parent) entity and other one is considered as attached (child) entity.  Therefore, for one base entity it could have zero or one attached entity; while for one attached entity there will be one and only one base entity.  From parent entity, it will be a virtual child object, no foreign key annotation needed.
<code>
public virtual ChildType? Child { get; set; }  
</code>
From child entity, it will be the same as pointing to parent entity for one-to-many relationship
<code>
[ForeignKey("Parent")] // required one to one map object / key
public int ParentID { get; set; }
public virtual ParentType Parent { get; set; }  
</code>
<h3>DB Context</h3>
This is a simple wrapper class for all tables, or classes inside Models folder for the project.  Create a folder called "Data" and put the context class in that folder.
<code>
public class DemoDbContext : DbContext
{
    public YourDbContext(DbContextOptions<DemoDbContext> options)
        : base(options)
    {
    }
    public DbSet<TypeI> TypeI (get; set;)   
    ...    
}  
</code>
<h2>OData Endpoint</h2>
You can start with an empty ASP.NET Core project, then follow steps below.  For more code details, please refer to https://github.com/zhufamily/ODataApp1/tree/main/ODataApp1.
<h3>Add Database Connections</h3>
Find a file named "appsettings.json", and then add a configiration value
<code>
"DemoDbConnectionString": "your_conn_str"
</code>
<h3>Add Controllers</h3>
Create a folder called "Controllers",  then add controllers for OData containers (which are corresponding to tables inside the database).  For this project, there are three controllers, and they are very similiar; here we just discuss more details for the CountriesController, and the same principles can be applied to other controllers.
<h4>Init Database Context with Context Injection</h4>
Later on you will see how to initialize the database context for the whole application inside Program.cs        
<code>
private readonly DemoDbContext db;
public CountriesController(DemoDbContext db)
{
    this.db = db;
}    
</code>
<h4>Add query for collection and single item</h4>
<code>
[EnableQuery]
public ActionResult<IEnumerable<Country>> Get()
{
    return Ok(db.Country);
}
[EnableQuery]
public ActionResult<Country> Get([FromRoute] int key)
{
    return Ok(SingleResult.Create(db.Country.Where(t => t.ID == key)));
}
</code>
<h4>Add Create/Update/Delete operations</h4>
The patch operation is the recommendded way for update, however put is still supported by OData v4 protocols.  The differences are very clear that patch only updates attributes inside the payload while put updates the entire record for all attributes.  Only insertion will return a record for all other operations there will be NO return contents.  You have to query again to confirm results.
<code>
public ActionResult Post([FromBody] Country country)
{
    db.Country.Add(country);
    db.SaveChanges();
    return Created(country);
}
public ActionResult Put([FromRoute] int key, [FromBody] Country updatedCountry)
{
    var country = db.Country.SingleOrDefault(d => d.ID == key);
    if (country == null)
    {
        return NotFound();
    }
    if (updatedCountry.ID <= 0)
    {
        updatedCountry.ID = key;    // no key provide, add the key
    }
    else if (updatedCountry.ID != key)
    {
        return BadRequest();        // wrong key
    }
    // detach the original object
    db.Entry(country).State = EntityState.Detached;
    // attach and mark dirty for updated object
    db.Entry(updatedCountry).State = EntityState.Modified;
    db.SaveChanges();
    return Updated(updatedCountry);
}
public ActionResult Patch([FromRoute] int key, [FromBody] Delta<Country> delta)
{
    var country = db.Country.SingleOrDefault(d => d.ID == key);
    if (country == null)
    {
        return NotFound();
    }
    delta.Patch(country);
    db.SaveChanges();
    return Updated(country);
}
public ActionResult Delete([FromRoute] int key)
{
    var country = db.Country.SingleOrDefault(d => d.ID == key);
    if (country == null)
    {
        return NotFound();
    }
    db.Country.Remove(country);
    db.SaveChanges();
    return NoContent();
}
</code>
<h3>Modify Program.cs</h3>
Finally, you will make some changes to Program.cs to register controllers.
<code>
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Country>("Countries");
modelBuilder.EntitySet<Airport>("Airports");
modelBuilder.EntitySet<AirportMetric>("AirportMetrics");
</code>    
Also, you need registering the OData endpoint.
<code>
builder.Services.AddControllers().AddOData(
    options => options.EnableQueryFeatures(null).AddRouteComponents(
        "odata",
        modelBuilder.GetEdmModel()));    
</code>
At the end, initialize the database context.
<code>
var configValue = builder.Configuration.GetValue<string>("DemoDbConnectionString");
builder.Services.AddDbContext<DemoDbContext>(options => options.UseSqlServer(configValue));
</code>    
<h2>Run OData Application</h2>
I assume you are a developer with experiences for SQL Server, VS 2022 and PostMan, or at least have some familarity with those tools.  Apparently, you need to know basics for OData v4 protocols.  This tutorial is built on .NET 6 with Entity Framework Core 6; you can certainly upgrade to .NET8 with Entity Framework 8 and all principals will stay the very same.    
<ol>
    <li>Clone the repository to local</li>
    <li>Use SSMS to connect to an empty SQL Server database and run script "demoTables.sql" to create three tables</li>
    <li>Open the solution with VS2022</li>
    <li>If needed reload all nuget packages</li>
    <li>Set up startup project to ODataApp1</li>
    <li>Open appsettings.json inside ODataApp1 project and replace "your_conn_str" with your real SQL Server Database connection string</li>
    <li>Click start debug button</li>
    <li>Work with PostMan to see results</li>
</ol>
<h2>Bonus -- Azure function Triggered by SQLServer</h2>
Now, it is still preview for Azure function triggered by SQL -- https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql-trigger?tabs=isolated-process%2Cportal&pivots=programming-language-csharp.  I include that in the solution to demo Data Models and Data Context can be used for multiple applications, as well as play with the particular trigger type.  For more code details, please refer to https://github.com/zhufamily/ODataApp1/tree/main/SQLFunctApp1.
<h3>Set Up Tracking inside SQL Server database</h3>
Before you can write Azure function for SQL trigger, you need setting up SQL Server Database -- https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql-trigger?tabs=isolated-process%2Cportal&pivots=programming-language-csharp#set-up-change-tracking-required.  In this case, set up the databaes and three tables for tracking.
<h3>SQL Trigger</h3>
Create a new Azure function, now, there is no template for SQL trigger, so you just put following codes manually.
<code>
 [FunctionName("CountryInsertTrigger")]
public void Run(
    [SqlTrigger("[dbo].[Country]", "SQL_CONN_STR")]
    IReadOnlyList<SqlChange<Country>> changes,
    ILogger logger)    
</code>
<h3>Context Injection for Database Context</h3>
Follow the Context Injection pattern -- https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection, you can initialize the database context in a startup.cs as shown below.
<code>
﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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
</code>        
Then, you can inject the context for each class as shown below.
<code>
private readonly DemoDbContext _db;

public SqlFunction(DemoDbContext db)
{
    this._db = db; 
}
</code>
<h3>Set up Environment Variable</h3>
As you have seen, there is an environment variable "SQL_CONN_STR", so please set that up, of course you can use key-vault if needed to protext the connection string.
<h3>Further Thinking</h3>
After played a while with the SQL triggered function, here are some limitations and ideas.
<ol>
    <li>It is async trigger, i.e., it will NOT roll back transaction in case of failure.</li>
    <li>It can only provide new values, but not old values.</li>
    <li>Performance is no where near native triggers inside the SQL Server.</li>
    <li>Extra tables for tracking and azure function leasing needs to be created, and data will transport outside SQL Server security domain.</li>
    <li>My personal feeling is that, if you need an aysnc triiger to integrate with other systems, this could be a great choice.</li>
    <li>If you trigger is purely DDL inside the database, I do not really recommend this approach due to its limitations above.</li>
    <li>Last, you can follow this pattern to have a native async trigger.
        <code>
CREATE TRIGGER [trigger_name]
   ON [table_name]
   AFTER INSERT|UPDATE|DELETE
AS 
BEGIN
    -- do not interfere with counts
    SET NOCOUNT ON;
    -- trigger is async, i.e. will NOT rollback main TRAN
    SET XACT_ABORT OFF;	
    -- save main TRAN to the point
    SAVE TRANSACTION Before_Trigger;
    BEGIN TRY
        DDL Statements Here ...
    END TRY
    BEGIN CATCH
        -- only roll back to the point saved
        ROLLBACK TRANSACTION Before_Trigger;
        -- raise error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR (@ErrorMessage,	-- Message text.
            @ErrorSeverity,			-- Severity.
            @ErrorState				-- State.
        );
    END CATCH
END
        </code>
    </li>
    <li>For each action, you can combine into one sync trigger and one async trigger then use this commamd -- https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-settriggerorder-transact-sql?view=sql-server-ver16 to ensure the sync trigger running before async trigger.</li>
</ol>
