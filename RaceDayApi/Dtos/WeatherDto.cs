namespace RaceDayApi.Dtos
{
    public class WeatherDto
    {
        public DateTime ForecastDate { get; set; }
        public decimal? TemperatureC { get; set; }
        public string? WeatherCondition { get; set; }
        public decimal? WindSpeedKmh { get; set; }
        public int? ChanceOfRainPercent { get; set; }
    }
}