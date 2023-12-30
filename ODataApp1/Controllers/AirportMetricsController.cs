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
    public class AirportMetricsController : ODataController
    {
        private readonly DemoDbContext db;

        public AirportMetricsController(DemoDbContext db)
        {
            this.db = db;
        }

        [EnableQuery]
        public ActionResult<IEnumerable<AirportMetric>> Get()
        {
            return Ok(db.AirportMetric);
        }

        [EnableQuery]
        public ActionResult<AirportMetric> Get([FromRoute] int key)
        {
            return Ok(SingleResult.Create(db.AirportMetric.Where(t => t.ID == key)));
        }

        // odata insert
        public ActionResult Post([FromBody] AirportMetric airportMetric)
        {
            db.AirportMetric.Add(airportMetric);

            db.SaveChanges();

            return Created(airportMetric);
        }

        // odata update
        // preferred approach
        public ActionResult Patch([FromRoute] int key, [FromBody] Delta<AirportMetric> delta)
        {
            var airportMetric = db.AirportMetric.SingleOrDefault(d => d.ID == key);

            if (airportMetric == null)
            {
                return NotFound();
            }

            delta.Patch(airportMetric);

            db.SaveChanges();

            return Updated(airportMetric);
        }

        // odata delete
        public ActionResult Delete([FromRoute] int key)
        {
            var airportMetric = db.AirportMetric.SingleOrDefault(d => d.ID == key);

            if (airportMetric == null)
            {
                return NotFound();
            }

            db.AirportMetric.Remove(airportMetric);

            db.SaveChanges();

            return NoContent();
        }
    }
}
