namespace InventorySalesDashboard.Models
{
    public class DashboardViewModel
    {
        public required List<Product> LowStockProducts { get; set; }
        public required List<Order> RecentOrders { get; set; }

        // New properties for enhanced dashboard
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int CriticalStockCount { get; set; }
    }
}