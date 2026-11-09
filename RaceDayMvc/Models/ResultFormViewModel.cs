using System.ComponentModel.DataAnnotations;

namespace RaceDayMvc.Models
{
    public class ResultFormViewModel
    {
        public int EventId { get; set; }
        public int EnrolmentId { get; set; }

        [RegularExpression(@"^\d{1,3}:[0-5]\d:[0-5]\d$", ErrorMessage = "Please enter the time as hh:mm:ss, for example 01:05:30.")]
        [Display(Name = "Finish time (hh:mm:ss)")]
        public string? FinishTime { get; set; }

        [Display(Name = "Overall position")]
        public int? OverallPosition { get; set; }

        [Display(Name = "Position in category")]
        public int? CategoryPosition { get; set; }

        public string Status { get; set; } = "Finished";
    }
}