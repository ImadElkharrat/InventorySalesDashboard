using InventorySalesDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesDashboard.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
        {
            // Default to last 30 days if no dates provided
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var orders = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var viewModel = new SalesReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Orders = orders,
                TotalRevenue = orders.Sum(o => o.Total),
                TotalProfit = orders.Sum(o => o.OrderLines.Sum(ol =>
                    (ol.UnitPrice - (ol.Product?.CostPrice ?? 0)) * ol.Quantity)),
                TotalOrders = orders.Count
            };

            return View(viewModel);
        }

        public async Task<IActionResult> InventoryReport()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            var categories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;

            var viewModel = new InventoryReportViewModel
            {
                Products = products,
                TotalStockValue = products.Sum(p => p.StockQuantity * p.CostPrice),
                LowStockCount = products.Count(p => p.StockQuantity <= p.ReorderLevel),
                OutOfStockCount = products.Count(p => p.StockQuantity == 0)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ProductPerformance()
        {
            var productPerformance = await _context.OrderLines
                .Include(ol => ol.Product)
                .GroupBy(ol => ol.Product)
                .Select(g => new ProductPerformanceViewModel
                {
                    Product = g.Key,
                    TotalSold = g.Sum(ol => ol.Quantity),
                    TotalRevenue = g.Sum(ol => ol.Quantity * ol.UnitPrice),
                    TotalProfit = g.Sum(ol => ol.Quantity * (ol.UnitPrice - (g.Key.CostPrice)))
                })
                .OrderByDescending(p => p.TotalRevenue)
                .ToListAsync();

            return View(productPerformance);
        }
    }
}