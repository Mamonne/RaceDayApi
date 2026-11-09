namespace RaceDayMvc.Models
{
    // One of the organiser's events, as sent back by the API
    public class MyEventItem
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string? BannerImageUrl { get; set; }
        public int TotalEnrolments { get; set; }
    }
}