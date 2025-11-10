using InventorySalesDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesDashboard.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var lowStockProducts = _context.Products
                .Where(p => p.StockQuantity <= p.ReorderLevel)
                .Include(p => p.Supplier)
                .ToList();

            var recentOrders = _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            // Calculate enhanced metrics
            var totalProducts = _context.Products.Count();
            var totalOrders = _context.Orders.Count();
            var totalRevenue = _context.Orders.Sum(o => o.Total);

            // Calculate profit (Revenue - Cost)
            var totalProfit = _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .AsEnumerable()
                .Sum(o => o.OrderLines.Sum(ol =>
                    (ol.UnitPrice - (ol.Product?.CostPrice ?? 0)) * ol.Quantity));

            var criticalStockCount = _context.Products
                .Count(p => p.StockQuantity <= p.ReorderLevel && p.StockQuantity > 0);

            var viewModel = new DashboardViewModel
            {
                LowStockProducts = lowStockProducts,
                RecentOrders = recentOrders,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                CriticalStockCount = criticalStockCount
            };

            return View(viewModel);
        }
    }
}