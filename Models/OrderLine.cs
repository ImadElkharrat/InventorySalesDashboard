namespace InventorySalesDashboard.Models
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation properties
        public required Order Order { get; set; }
        public required Product Product { get; set; }
    }
}