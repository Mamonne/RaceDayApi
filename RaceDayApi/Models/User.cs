using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string FullName { get; set; } = "";

        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = "";

        [Column(TypeName = "varchar(255)")]
        public string PasswordHash { get; set; } = "";

        [Column(TypeName = "varchar(20)")]
        public string Role { get; set; } = "Participant";

        [Column(TypeName = "varchar(20)")]
        public string? PhoneNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}