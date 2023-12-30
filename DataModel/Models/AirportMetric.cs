namespace OData.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class AirportMetric
    {
        [Key] // primary key
        public int ID { get; set; }
        public double? Score { get; set; }
        [ForeignKey("Airport")] // required one to one map object / key
        public int AirportID { get; set; }
        public virtual Airport Airport { get; set; }

    }
}
