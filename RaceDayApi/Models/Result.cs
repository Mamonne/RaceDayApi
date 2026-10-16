using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "Finished";   // Finished, DNF or DSQ
    }
}