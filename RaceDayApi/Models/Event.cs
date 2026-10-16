using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public int OrganiserId { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string EventName { get; set; } = "";

        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Location { get; set; } = "";

        [Column(TypeName = "varchar(1000)")]
        public string? Description { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string EventType { get; set; } = "";   // Running, Walking or Cycling

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}