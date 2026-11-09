using System.ComponentModel.DataAnnotations;

namespace RaceDayMvc.Models
{
    public class CategoryFormViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter the category name.")]
        [StringLength(100, ErrorMessage = "The category name must be 100 characters or fewer.")]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; } = "";

        [Required(ErrorMessage = "Please enter the distance in kilometres.")]
        [Range(0.01, 1000, ErrorMessage = "The distance must be greater than 0 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Please enter the entry fee (use 0 if it is free).")]
        [Range(0, 100000, ErrorMessage = "The entry fee cannot be negative.")]
        [Display(Name = "Entry fee (R)")]
        public decimal? EntryFee { get; set; }

        [Range(1, 1000000, ErrorMessage = "The number of places must be at least 1.")]
        [Display(Name = "Maximum participants")]
        public int? MaxParticipants { get; set; }
    }
}