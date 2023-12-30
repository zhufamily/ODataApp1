<h1>A detailed tutorial for SQL Server and OData endpoint with Entity Framework Core</h1>
<h2>SQL Serve</h2>
The first step is to set up a SQL Server database.  I know, I know lots of you might scream for a code first approach.  However, in a real enterprise or big development environment, database is usually controlled by DB administrators rather than developers, and further more, outside development environment, frontend application codes might have limited or no access to DDL but only fully access to DML.  So, I try to simulate the real world senario rather then some conceptional design approach.  Please refer to https://github.com/zhufamily/ODataApp1/blob/main/demoTables.sql for more details. 
<h3>Tables</h3>
<ol>
<li>Country table parent table for Aiport table (one-to-many).</li>
<li>Airport table child of Country table, and parent table for AirportMetric table (one-to-one).</li>
<li>AirportMetric table child table for Airport table.</li>
</ol>  
<h2>.Net Core Project for Data Models and Data Context</h2>
Next, we are going to set up a .NET core project for data models and data context.  The reason we setup a separate project is that we can reuse that in the future, e.g., OData endpoint, or SQL related Azure Function, or web application frontend and etc.  If you are inside Visual Studio, there is a GUI tool, you can download here -- https://marketplace.visualstudio.com/items?itemName=ErikEJ.EFCorePowerTools, at the moment, it supportes Entity Framework Core 6-8.  Otherwise, you can refer to command line tool https://learn.microsoft.com/en-us/ef/core/cli/ or simply setup POCO classes manually.  Please refer to https://github.com/zhufamily/ODataApp1/tree/main/DataModel for more details.
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

    // Must match the table name inside database
    // Default to dbo namespace
    public DbSet<TypeI> TableI { get; set; }
    public DbSet<TypeII> TableII { get; set; }
    public DbSet<TypeIII> TableIII { get; set; }
}  
</code>  
