namespace RaceDayMvc.Models
{
    public class DashboardViewModel
    {
        public List<MyEventItem> Events { get; set; } = new List<MyEventItem>();

        // Summary numbers for the cards at the top
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int UpcomingEvents { get; set; }
        public DateTime? NextEventDate { get; set; }

        public string? ErrorMessage { get; set; }
    }
}