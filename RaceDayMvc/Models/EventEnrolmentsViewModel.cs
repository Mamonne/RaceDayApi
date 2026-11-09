namespace RaceDayMvc.Models
{
    public class EventEnrolmentsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public List<EventEnrolmentItem> Enrolments { get; set; } = new List<EventEnrolmentItem>();
    }
}