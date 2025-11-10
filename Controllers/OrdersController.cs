using InventorySalesDashboard.Hubs;
using InventorySalesDashboard.Models;
using InventorySalesDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySalesDashboard.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InvoiceService _invoiceService;
        private readonly IHubContext<DashboardHub> _hubContext;

        public OrdersController(ApplicationDbContext context, InvoiceService invoiceService, IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _invoiceService = invoiceService;
            _hubContext = hubContext;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, List<int> productIds, List<int> quantities, List<decimal> prices)
        {
            // Send real-time notification
            await _hubContext.Clients.All.SendAsync("ReceiveNewOrderAlert", order.CustomerName, order.Total);

            // Send stock updates for each product
            foreach (var line in order.OrderLines)
            {
                var product = await _context.Products.FindAsync(line.ProductId);
                if (product != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", product.Name, product.StockQuantity);

                    // Send low stock alert if applicable
                    if (product.StockQuantity <= product.ReorderLevel)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity, product.ReorderLevel);
                    }
                }
            }
            if (ModelState.IsValid)
            {
                // Calculate total and create order lines
                decimal total = 0;
                for (int i = 0; i < productIds.Count; i++)
                {
                    var product = await _context.Products.FindAsync(productIds[i]);
                    if (product != null)
                    {
                        var orderLine = new OrderLine
                        {
                            ProductId = productIds[i],
                            Quantity = quantities[i],
                            UnitPrice = prices[i],
                            Order = order,
                            Product = product
                        };
                        order.OrderLines.Add(orderLine);
                        total += quantities[i] * prices[i];

                        // Update stock
                        product.StockQuantity -= quantities[i];
                    }
                }

                order.Total = total;
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // IMPORTANT: Include OrderLines and their Products
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerName,OrderDate,Total")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        // Add this new method for PDF generation
        public IActionResult GenerateInvoice(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var pdfBytes = _invoiceService.GenerateInvoice(order);
            return File(pdfBytes, "application/pdf", $"Invoice_{order.Id}_{order.CustomerName}.pdf");
        }
    }
}
