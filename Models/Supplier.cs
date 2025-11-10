using System.ComponentModel.DataAnnotations;

namespace InventorySalesDashboard.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Supplier Name")]
        public required string Name { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public required string Phone { get; set; }

        public required string Address { get; set; }

        // Navigation property - one supplier can have many products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}