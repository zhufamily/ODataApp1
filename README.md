<h1>A tutorial for SQL Server and OData endpoint with Entity Framework Core</h1>
<h2>SQL Serve</h2>
The first step is to set up a SQL Server database.  I know, I know lots of you might scream for a code first approach.  However, in a real enterprise or big development environment, database is usually controlled by DB administrators rather than developers, and further more, outside development environment, frontend application codes might have limited or no access to DDL but only fully access to DML.  So, I try to simulate the real world senario rather then some conceptional design approach.  Please refer to https://github.com/zhufamily/ODataApp1/blob/main/demoTables.sql for more details. 
<h3>Tables</h3>
<ol>
<li>Country table parent table for Aiport table (one-to-many).</li>
<li>Airport table child of Country table, and parent table for AirportMetric table (one-to-one).</li>
<li>AirportMetric table child table for Airport table.</li>
</ol>  
<h2>.Net Core Project for Data Models and Data Context</h2>
Next, we are going to set up a .NET Core Library project for data models and data context.  The reason we setup a separate project is that we can reuse that in the future, e.g., OData endpoint, or SQL related Azure Function, or web application frontend and etc.  If you are inside Visual Studio, there is a GUI tool, you can download here -- https://marketplace.visualstudio.com/items?itemName=ErikEJ.EFCorePowerTools, at the moment, it supportes Entity Framework Core 6-8.  Otherwise, you can refer to command line tool https://learn.microsoft.com/en-us/ef/core/cli/ or simply setup POCO classes manually.  Please refer to https://github.com/zhufamily/ODataApp1/tree/main/DataModel for more details.
<h3>Primary Key</h3>
For primary key field, just annotate with "[Key]" attribute.
<code>
[Key] // primary key
public int ID { get; set; }
</code>
<h3>One-to-many relationship</h3>
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
<h3>One-to-one relationship</h3>
One to one relationship is rare used, but useful in certain situations, e.g., partial data are sensitive to security needs more access control.  Usually, one entity is considered as base (parent) entity and other one is considered as attached (child) entity.  Therefore, for one base entity it could have zero or one attached entity; while for one attached entity there will be one and only one base entity.  From parent entity, it will be a virtual child object, no foreign key annotation needed.
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
This is a simple wrapper classes for all tables, or classes inside Models for data project.
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
You can strat with an empty ASP.NET Core project, then following steps below.  For more details, please refer to https://github.com/zhufamily/ODataApp1/tree/main/ODataApp1.
<h3>Add Database Connections</h3>
Find a file named "appsettings.json", and then add a configiration value
<code>
"DemoDbConnectionString": "your_conn_str"
</code>
<h3>Add Controllers</h3>
Create a folder called "Controllers",  then add controllers for OData containers (which are corresponding to tables inside the database).  For this project, there are three controllers, and they are very similiar; here we just discuss more details for the CountriesController, and the same principles can be applied to other controllers.
<h4>Init Database Context with Context Injection</h4>
Later on you will see how to init the database context for the whole application inside Program.cs        
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
The patch operation is the recommendded way for update, however put is still supported by OData.  The difference are very clear that patch only update attributes inside the payload while put update the entire record for all attributes.  Only insertion will return a record for all other operations will NOT retuen contents.  You have to query again to confirm the update results.
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
<h2>Run OData Application</h2>
I assume you are a developer with experiences for SQL Server, VS 2022 and PostMan, or at least have some familarity with those tools.  Apparently, you need to know basics for OData v4 protocols.  This tutorial is built on .NET 6 with Entity Framework Core 6; you can certainly upgrade to .NET8 with Entity Framework 8 and all principals will stay the very same.    
<ol>
    <li>Clone the repository to local</li>
    <li>Use SSMS to connect to an empty SQL Server database and run script "demoTables" to create three tables</li>
    <li>Open the solution with VS2022</li>
    <li>If needed reload all nuget packages</li>
    <li>Set up startup project to ODataApp1</li>
    <li>Open appsettings.json inside ODataApp1 project and replace "your_conn_str" with your real SQL Server Database connection string</li>
    <li>Click start debug button</li>
    <li>Work with PostMan to see results</li>
</ol>
