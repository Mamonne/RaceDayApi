namespace RaceDayApi.Dtos
{
    public class CategoryDto
    {
        public string CategoryName { get; set; } = "";
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public int? MaxParticipants { get; set; }
    }
}