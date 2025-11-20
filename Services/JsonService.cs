using System.Text.Json;
using System.Text.Json.Serialization;
using InventorySalesDashboard.Models;

namespace InventorySalesDashboard.Services
{
    public class JsonService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public string ExportProductsToJson(List<Product> products)
        {
            var exportData = products.Select(p => new ProductExportModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                Description = p.Description,
                Category = p.Category,
                ImageUrl = p.ImageUrl,
                SupplierName = p.Supplier?.Name,
                CreatedDate = DateTime.Now
            }).ToList();

            return JsonSerializer.Serialize(exportData, _jsonOptions);
        }

        public List<ProductImportModel> ImportProductsFromJson(string jsonContent)
        {
            try
            {
                var importData = JsonSerializer.Deserialize<List<ProductImportModel>>(jsonContent, _jsonOptions);
                return importData ?? new List<ProductImportModel>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Le fichier JSON est invalide ou corrompu.", ex);
            }
        }

        public byte[] ExportProductsToJsonFile(List<Product> products)
        {
            var json = ExportProductsToJson(products);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }

    // Modèle pour l'exportation
    public class ProductExportModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public string? SupplierName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // Modèle pour l'importation
    public class ProductImportModel
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public string? SupplierName { get; set; }
    }
}