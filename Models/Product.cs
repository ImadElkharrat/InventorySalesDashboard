using System.ComponentModel.DataAnnotations;

namespace InventorySalesDashboard.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string SKU { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public int? SupplierId { get; set; }  // Nullable because a product might not have a supplier initially
        public Supplier? Supplier { get; set; }

        [Display(Name = "Cost Price")]
        [Range(0.01, double.MaxValue)]
        public decimal CostPrice { get; set; }

        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
    }
}