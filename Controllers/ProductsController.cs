using InventorySalesDashboard.Models;
using InventorySalesDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySalesDashboard.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageService _imageService;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, ImageService imageService, IWebHostEnvironment environment)
        {
            _context = context;
            _imageService = imageService;
            _environment = environment;
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
    }
}
