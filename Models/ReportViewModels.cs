namespace InventorySalesDashboard.Models
{
    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Order> Orders { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
    }

    public class InventoryReportViewModel
    {
        public List<Product> Products { get; set; } = new();
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class ProductPerformanceViewModel
    {
        public required Product Product { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin => TotalRevenue > 0 ? (TotalProfit / TotalRevenue) * 100 : 0;
    }
}