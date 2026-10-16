using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class RaceRoute
    {
        public int RouteId { get; set; }

        public int EventId { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RouteName { get; set; } = "";

        [Column(TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        public int? ElevationGainM { get; set; }
    }
}