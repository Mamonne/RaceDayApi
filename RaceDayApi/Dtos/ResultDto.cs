namespace RaceDayApi.Dtos
{
    public class ResultDto
    {
        public int EnrolmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public string Status { get; set; } = "Finished";
    }
}