namespace ODataApp1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Deltas;
    using Microsoft.AspNetCore.OData.Query;
    using Microsoft.AspNetCore.OData.Results;
    using Microsoft.AspNetCore.OData.Routing.Controllers;
    using OData.Data;
    using OData.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class AirportsController : ODataController
    {
        private readonly DemoDbContext db;

        public AirportsController(DemoDbContext db)
        {
            this.db = db;
        }

        [EnableQuery]
        public ActionResult<IEnumerable<Airport>> Get()
        {
            return Ok(db.Airport);
        }

        [EnableQuery]
        public ActionResult<Airport> Get([FromRoute] int key)
        {
            return Ok(SingleResult.Create(db.Airport.Where(t => t.ID == key)));
        }

        // odata insert
        public ActionResult Post([FromBody] Airport airport)
        {
            db.Airport.Add(airport);

            db.SaveChanges();

            return Created(airport);
        }

        // odata update
        // preferred approach
        public ActionResult Patch([FromRoute] int key, [FromBody] Delta<Airport> delta)
        {
            var airport = db.Airport.SingleOrDefault(d => d.ID == key);

            if (airport == null)
            {
                return NotFound();
            }

            delta.Patch(airport);

            db.SaveChanges();

            return Updated(airport);
        }

        // odata delete
        public ActionResult Delete([FromRoute] int key)
        {
            var airport = db.Airport.SingleOrDefault(d => d.ID == key);

            if (airport == null)
            {
                return NotFound();
            }

            db.Airport.Remove(airport);

            db.SaveChanges();

            return NoContent();
        }
    }
}
