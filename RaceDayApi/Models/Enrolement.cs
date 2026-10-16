using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public int CategoryId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? BibNumber { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Column(TypeName = "varchar(20)")]
        public string PaymentStatus { get; set; } = "Pending";   // Pending, Paid or Cancelled
    }
}