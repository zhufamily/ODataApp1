namespace ODataApp1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Deltas;
    using Microsoft.AspNetCore.OData.Query;
    using Microsoft.AspNetCore.OData.Results;
    using Microsoft.AspNetCore.OData.Routing.Controllers;
    using Microsoft.EntityFrameworkCore;
    using OData.Data;
    using OData.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class CountriesController : ODataController
    {
        // point to database context
        // static for all instances
        private readonly DemoDbContext db;

        public CountriesController(DemoDbContext db)
        {
            this.db = db;
        }

        // odata collection query
        [EnableQuery]
        public ActionResult<IEnumerable<Country>> Get()
        {
            return Ok(db.Country);
        }

        // odata single item query
        [EnableQuery]
        public ActionResult<Country> Get([FromRoute] int key)
        {
            return Ok(SingleResult.Create(db.Country.Where(t => t.ID == key)));
        }

        // odata insert
        public ActionResult Post([FromBody] Country country)
        {
            db.Country.Add(country);
            
            db.SaveChanges();

            return Created(country);
        }

        // odata update 
        // NOT preferred approach
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

        // odata update
        // preferred approach
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

        // odata delete
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
    }
}
