namespace RaceDayMvc.Models
{
    // One category, as sent back by the API
    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public int? MaxParticipants { get; set; }
    }
}