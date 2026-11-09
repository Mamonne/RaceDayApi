using System.ComponentModel.DataAnnotations;

namespace RaceDayMvc.Models
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = "";

        public string Role { get; set; } = "";

        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }
    }
}