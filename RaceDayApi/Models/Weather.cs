using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class Weather
    {
        public int WeatherId { get; set; }

        public int EventId { get; set; }

        [Column(TypeName = "date")]
        public DateTime ForecastDate { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal? TemperatureC { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? WeatherCondition { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        public decimal? WindSpeedKmh { get; set; }

        public int? ChanceOfRainPercent { get; set; }
    }
}