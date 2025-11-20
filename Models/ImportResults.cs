namespace InventorySalesDashboard.Models
{
    public class ImportResults
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}