using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CategoryName { get; set; } = "";

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal EntryFee { get; set; }

        public int? MaxParticipants { get; set; }
    }
}