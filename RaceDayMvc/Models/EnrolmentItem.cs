namespace RaceDayMvc.Models
{
    // One enrolment, as sent back by the API
    public class EnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public DateTime RegistrationDate { get; set; }
        public string PaymentStatus { get; set; } = "";
    }
}