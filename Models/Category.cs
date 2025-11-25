using System.ComponentModel.DataAnnotations;

namespace InventorySalesDashboard.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // Additional properties for future enhancements
        public string? Color { get; set; } // For UI color coding
        public string? Icon { get; set; } // For UI icons
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}