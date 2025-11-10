using Microsoft.AspNetCore.SignalR;

namespace InventorySalesDashboard.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task SendLowStockAlert(string productName, int currentStock, int reorderLevel)
        {
            await Clients.All.SendAsync("ReceiveLowStockAlert", productName, currentStock, reorderLevel);
        }

        public async Task SendNewOrderAlert(string customerName, decimal orderTotal)
        {
            await Clients.All.SendAsync("ReceiveNewOrderAlert", customerName, orderTotal);
        }

        public async Task SendStockUpdate(string productName, int newStockLevel)
        {
            await Clients.All.SendAsync("ReceiveStockUpdate", productName, newStockLevel);
        }
    }
}