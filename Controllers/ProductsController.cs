using InventorySalesDashboard.Models;
using InventorySalesDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        // Add export action
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .ToListAsync();

            var excelBytes = _excelExportService.ExportProductsToExcel(products);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
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
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    existingProduct.Category = product.Category;

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
                    return View(product);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Export JSON
        public async Task<IActionResult> ExportToJson()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .ToListAsync();

            var jsonBytes = _jsonService.ExportProductsToJsonFile(products);
            return File(jsonBytes, "application/json", $"products_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        // Import JSON - GET
        public IActionResult ImportFromJson()
        {
            return View();
        }

        // Import JSON - POST
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
                // Lire le contenu du fichier
                using var stream = new StreamReader(jsonFile.OpenReadStream());
                var jsonContent = await stream.ReadToEndAsync();

                // Désérialiser les produits
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
                    ErrorCount = 0,
                    Errors = new List<string>()
                };

                // Traiter chaque produit importé
                foreach (var importedProduct in importedProducts)
                {
                    try
                    {
                        // Vérifier si le produit existe déjà
                        var existingProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.SKU == importedProduct.SKU);

                        if (existingProduct != null && updateExisting)
                        {
                            // Mettre à jour le produit existant
                            existingProduct.Name = importedProduct.Name;
                            existingProduct.Price = importedProduct.Price;
                            existingProduct.CostPrice = importedProduct.CostPrice;
                            existingProduct.StockQuantity = importedProduct.StockQuantity;
                            existingProduct.ReorderLevel = importedProduct.ReorderLevel;
                            existingProduct.Description = importedProduct.Description;
                            existingProduct.Category = importedProduct.Category;
                            existingProduct.ImageUrl = importedProduct.ImageUrl;

                            // Gérer le fournisseur
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
                            // Créer un nouveau produit
                            var newProduct = new Product
                            {
                                Name = importedProduct.Name,
                                SKU = importedProduct.SKU,
                                Price = importedProduct.Price,
                                CostPrice = importedProduct.CostPrice,
                                StockQuantity = importedProduct.StockQuantity,
                                ReorderLevel = importedProduct.ReorderLevel,
                                Description = importedProduct.Description,
                                Category = importedProduct.Category,
                                ImageUrl = importedProduct.ImageUrl
                            };

                            // Gérer le fournisseur
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
                            // Produit existe mais updateExisting est false
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

                // Sauvegarder les résultats
                await _context.SaveChangesAsync();

                // Passer les résultats à la vue
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

        // Résultats d'importation
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
