namespace RaceDayMvc.Models
{
    public class HomeIndexViewModel
    {
        public List<EventItem> Events { get; set; } = new List<EventItem>();

        // The filters the user chose
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public decimal? MinDistance { get; set; }
        public decimal? MaxDistance { get; set; }

        // A friendly message if the API could not be reached
        public string? ErrorMessage { get; set; }
    }
}