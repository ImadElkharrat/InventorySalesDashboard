using InventorySalesDashboard.Models;
using InventorySalesDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventorySalesDashboard.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageService _imageService;
        private readonly IWebHostEnvironment _environment;
        private readonly ExcelExportService _excelExportService;
        private readonly JsonService _jsonService;

        public ProductsController(ApplicationDbContext context, ImageService imageService,
            IWebHostEnvironment environment, ExcelExportService excelExportService, JsonService jsonService)
        {
            _context = context;
            _imageService = imageService;
            _environment = environment;
            _excelExportService = excelExportService;
            _jsonService = jsonService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null)
                    {
                        product.ImageUrl = await _imageService.SaveImageAsync(imageFile);
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, bool removeImage = false)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Handle image removal
                    if (removeImage && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        _imageService.DeleteImage(existingProduct.ImageUrl);
                        existingProduct.ImageUrl = null;
                    }

                    // Handle new image upload
                    if (imageFile != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            _imageService.DeleteImage(existingProduct.ImageUrl);
                        }

                        // Save new image
                        existingProduct.ImageUrl = await _imageService.SaveImageAsync(imageFile);
                    }

                    // Update other properties
                    existingProduct.Name = product.Name;
                    existingProduct.SKU = product.SKU;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.ReorderLevel = product.ReorderLevel;
                    existingProduct.SupplierId = product.SupplierId;
                    existingProduct.CostPrice = product.CostPrice;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    // Show success notification
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                    return View(product);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .ToListAsync();

            var excelBytes = _excelExportService.ExportProductsToExcel(products);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        // Export to JSON
        public async Task<IActionResult> ExportToJson()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .ToListAsync();

            var jsonBytes = _jsonService.ExportProductsToJsonFile(products);
            return File(jsonBytes, "application/json", $"products_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        // Import from JSON - GET
        public IActionResult ImportFromJson()
        {
            return View();
        }

        // Import from JSON - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromJson(IFormFile jsonFile, bool updateExisting = false)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ModelState.AddModelError("", "Veuillez sélectionner un fichier JSON.");
                return View();
            }

            try
            {
                // Read file content
                using var stream = new StreamReader(jsonFile.OpenReadStream());
                var jsonContent = await stream.ReadToEndAsync();

                // Deserialize products
                var importedProducts = _jsonService.ImportProductsFromJson(jsonContent);

                if (!importedProducts.Any())
                {
                    ModelState.AddModelError("", "Le fichier JSON ne contient aucun produit valide.");
                    return View();
                }

                var results = new ImportResults
                {
                    TotalProcessed = importedProducts.Count,
                    SuccessCount = 0,
                    UpdatedCount = 0,
                    SkippedCount = 0,
                    ErrorCount = 0,
                    Errors = new List<string>()
                };

                // Process each imported product
                foreach (var importedProduct in importedProducts)
                {
                    try
                    {
                        // Check if product already exists
                        var existingProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.SKU == importedProduct.SKU);

                        if (existingProduct != null && updateExisting)
                        {
                            // Update existing product
                            existingProduct.Name = importedProduct.Name;
                            existingProduct.Price = importedProduct.Price;
                            existingProduct.CostPrice = importedProduct.CostPrice;
                            existingProduct.StockQuantity = importedProduct.StockQuantity;
                            existingProduct.ReorderLevel = importedProduct.ReorderLevel;
                            existingProduct.Description = importedProduct.Description;
                            
                            // Handle category
                            if (!string.IsNullOrEmpty(importedProduct.CategoryName))
                            {
                                var category = await _context.Categories
                                    .FirstOrDefaultAsync(c => c.Name == importedProduct.CategoryName);

                                if (category == null)
                                {
                                    category = new Category { Name = importedProduct.CategoryName, IsActive = true, CreatedDate = DateTime.Now };
                                    _context.Categories.Add(category);
                                    await _context.SaveChangesAsync();
                                }
                                existingProduct.CategoryId = category.Id;
                            }

                            // Handle supplier
                            if (!string.IsNullOrEmpty(importedProduct.SupplierName))
                            {
                                var supplier = await _context.Suppliers
                                    .FirstOrDefaultAsync(s => s.Name == importedProduct.SupplierName);

                                if (supplier == null)
                                {
                                    supplier = new Supplier
                                    {
                                        Name = importedProduct.SupplierName,
                                        Email = "unknown@example.com",
                                        Phone = "N/A",
                                        Address = "N/A"
                                    };
                                    _context.Suppliers.Add(supplier);
                                    await _context.SaveChangesAsync();
                                }
                                existingProduct.SupplierId = supplier.Id;
                            }

                            _context.Products.Update(existingProduct);
                            results.UpdatedCount++;
                        }
                        else if (existingProduct == null)
                        {
                            // Create new product
                            var newProduct = new Product
                            {
                                Name = importedProduct.Name,
                                SKU = importedProduct.SKU,
                                Price = importedProduct.Price,
                                CostPrice = importedProduct.CostPrice,
                                StockQuantity = importedProduct.StockQuantity,
                                ReorderLevel = importedProduct.ReorderLevel,
                                Description = importedProduct.Description,
                                ImageUrl = importedProduct.ImageUrl
                            };

                            // Handle category
                            if (!string.IsNullOrEmpty(importedProduct.CategoryName))
                            {
                                var category = await _context.Categories
                                    .FirstOrDefaultAsync(c => c.Name == importedProduct.CategoryName);

                                if (category == null)
                                {
                                    category = new Category { Name = importedProduct.CategoryName, IsActive = true, CreatedDate = DateTime.Now };
                                    _context.Categories.Add(category);
                                    await _context.SaveChangesAsync();
                                }
                                newProduct.CategoryId = category.Id;
                            }

                            // Handle supplier
                            if (!string.IsNullOrEmpty(importedProduct.SupplierName))
                            {
                                var supplier = await _context.Suppliers
                                    .FirstOrDefaultAsync(s => s.Name == importedProduct.SupplierName);

                                if (supplier == null)
                                {
                                    supplier = new Supplier
                                    {
                                        Name = importedProduct.SupplierName,
                                        Email = "unknown@example.com",
                                        Phone = "N/A",
                                        Address = "N/A"
                                    };
                                    _context.Suppliers.Add(supplier);
                                    await _context.SaveChangesAsync();
                                }
                                newProduct.SupplierId = supplier.Id;
                            }

                            _context.Products.Add(newProduct);
                            results.SuccessCount++;
                        }
                        else
                        {
                            // Product exists but updateExisting is false
                            results.SkippedCount++;
                            results.Errors.Add($"Produit avec SKU '{importedProduct.SKU}' existe déjà. Non mis à jour.");
                        }

                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        results.ErrorCount++;
                        results.Errors.Add($"Erreur avec produit '{importedProduct.Name}': {ex.Message}");
                    }
                }

                // Save results to TempData
                TempData["ImportResults"] = JsonSerializer.Serialize(results);
                TempData["SuccessMessage"] = $"Importation terminée! {results.SuccessCount} produits importés, {results.UpdatedCount} mis à jour, {results.SkippedCount} ignorés, {results.ErrorCount} erreurs.";

                return RedirectToAction(nameof(ImportResults));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur lors de l'importation: {ex.Message}");
                return View();
            }
        }

        // Import Results
        public IActionResult ImportResults()
        {
            if (TempData["ImportResults"] is string resultsJson)
            {
                var results = JsonSerializer.Deserialize<ImportResults>(resultsJson);
                return View(results);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}