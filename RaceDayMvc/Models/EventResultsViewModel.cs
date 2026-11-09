namespace RaceDayMvc.Models
{
    // One row of the page: an enrolment and its result, if there is one
    public class ResultRow
    {
        public EventEnrolmentItem Enrolment { get; set; } = new EventEnrolmentItem();
        public EventResultItem? Result { get; set; }
    }

    public class EventResultsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public List<ResultRow> Rows { get; set; } = new List<ResultRow>();
    }
}