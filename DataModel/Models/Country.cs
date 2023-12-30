namespace OData.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Country
    {
        [Key] // primary key
        public int ID { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        // optional child collection
        public virtual ICollection<Airport> Airports { get; set; }
    }
}
