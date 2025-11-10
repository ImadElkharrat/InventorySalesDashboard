using InventorySalesDashboard.Models;
using InventorySalesDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventorySalesDashboard.BackgroundServices
{
    public class LowStockAlertService : BackgroundService
    {
        private readonly ILogger<LowStockAlertService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check daily

        public LowStockAlertService(ILogger<LowStockAlertService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Low Stock Alert Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                        var lowStockProducts = await dbContext.Products
                            .Include(p => p.Supplier)
                            .Where(p => p.StockQuantity <= p.ReorderLevel)
                            .ToListAsync(stoppingToken);

                        if (lowStockProducts.Any())
                        {
                            await emailService.SendLowStockAlertAsync(lowStockProducts);
                            _logger.LogInformation("Sent low stock alert for {Count} products", lowStockProducts.Count);
                        }
                    }

                    // Wait until next day 8:00 AM
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddDays(1).AddHours(8);
                    var delay = nextRun - now;

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Low Stock Alert Service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Retry after 1 hour
                }
            }
        }
    }
}