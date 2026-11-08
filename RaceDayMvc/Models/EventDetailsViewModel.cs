namespace RaceDayMvc.Models
{
    public class EventDetailsViewModel
    {
        public EventItem? Event { get; set; }
        public List<CategoryItem> Categories { get; set; } = new List<CategoryItem>();
        public string? ErrorMessage { get; set; }
    }
}