namespace RaceDayMvc.Models
{
    public class MyEnrolmentsViewModel
    {
        public List<EnrolmentItem> Enrolments { get; set; } = new List<EnrolmentItem>();
        public string? ErrorMessage { get; set; }
    }
}