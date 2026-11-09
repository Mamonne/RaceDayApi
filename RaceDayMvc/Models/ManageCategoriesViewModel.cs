namespace RaceDayMvc.Models
{
    public class ManageCategoriesViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public List<CategoryItem> Categories { get; set; } = new List<CategoryItem>();
    }
}