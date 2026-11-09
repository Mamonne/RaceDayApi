using System.ComponentModel.DataAnnotations;

namespace RaceDayMvc.Models
{
    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(150, ErrorMessage = "The event name must be 150 characters or fewer.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = "";

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(150, ErrorMessage = "The location must be 150 characters or fewer.")]
        public string Location { get; set; } = "";

        [StringLength(1000, ErrorMessage = "The description must be 1000 characters or fewer.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please enter the distance in kilometres.")]
        [Range(0.01, 1000, ErrorMessage = "The distance must be greater than 0 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Please choose the event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "";
    }
}