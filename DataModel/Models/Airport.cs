namespace OData.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class Airport
    {
        [Key] // primary key
        public int ID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }

        [ForeignKey("Country")] // foreign key
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }
        // optional one to one mapping object
        public virtual AirportMetric? AirportMetric { get; set; }
    }
}
