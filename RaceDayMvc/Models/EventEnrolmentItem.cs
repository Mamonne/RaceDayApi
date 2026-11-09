namespace RaceDayMvc.Models
{
    // One enrolment for an event, as sent back by the API
    public class EventEnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public DateTime RegistrationDate { get; set; }
        public string PaymentStatus { get; set; } = "";
    }
}