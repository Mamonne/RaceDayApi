namespace RaceDayApi.Dtos
{
    public class EventDto
    {
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string? Description { get; set; }
        public string EventType { get; set; } = "";
    }
}