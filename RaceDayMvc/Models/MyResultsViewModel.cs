namespace RaceDayMvc.Models
{
    public class MyResultsViewModel
    {
        public List<ResultItem> Results { get; set; } = new List<ResultItem>();
        public string? ErrorMessage { get; set; }
    }
}