namespace RaceDayApi.Dtos
{
    public class RouteDto
    {
        public string RouteName { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public int? ElevationGainM { get; set; }
    }
}