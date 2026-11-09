namespace RaceDayMvc.Models
{
    // One race result, as sent back by the API
    public class ResultItem
    {
        public int ResultId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public TimeSpan? FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public string Status { get; set; } = "";
        public int TotalFinishers { get; set; }
    }
}