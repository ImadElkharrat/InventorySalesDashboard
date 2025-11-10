using System.ComponentModel.DataAnnotations;

namespace InventorySalesDashboard.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public required string CustomerName { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal Total { get; set; }

        // Navigation property
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}