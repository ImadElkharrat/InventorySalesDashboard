using InventorySalesDashboard.Models;
using System.Net;
using System.Net.Mail;

namespace InventorySalesDashboard.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendLowStockAlertAsync(List<Product> lowStockProducts)
        {
            if (!lowStockProducts.Any()) return;

            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var portString = emailSettings["Port"];
                if (string.IsNullOrWhiteSpace(portString))
                    throw new InvalidOperationException("EmailSettings:Port configuration is missing or empty.");
                var port = int.Parse(portString);
                var senderEmail = emailSettings["SenderEmail"];
                var password = emailSettings["Password"];
                var senderName = emailSettings["SenderName"];

                // Add null checks for senderEmail and senderName
                if (string.IsNullOrWhiteSpace(senderEmail))
                    throw new InvalidOperationException("EmailSettings:SenderEmail configuration is missing or empty.");
                if (string.IsNullOrWhiteSpace(senderName))
                    throw new InvalidOperationException("EmailSettings:SenderName configuration is missing or empty.");

                using (var client = new SmtpClient(smtpServer, port))
                {
                    client.Credentials = new NetworkCredential(senderEmail, password);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = $"Low Stock Alert - {DateTime.Today:MMM dd, yyyy}",
                        Body = GenerateLowStockEmailBody(lowStockProducts),
                        IsBodyHtml = true
                    };

                    // In production, you would get this from configuration or database
                    mailMessage.To.Add("admin@yourcompany.com");

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Low stock alert email sent successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send low stock alert email");
                // Don't throw - email failure shouldn't break the application
            }
        }

        private string GenerateLowStockEmailBody(List<Product> lowStockProducts)
        {
            var html = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .alert {{ color: #856404; background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; margin: 10px 0; }}
                        table {{ border-collapse: collapse; width: 100%; }}
                        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                        th {{ background-color: #f2f2f2; }}
                    </style>
                </head>
                <body>
                    <h2>🚨 Low Stock Alert</h2>
                    <p>The following products are running low on stock and need to be reordered:</p>
                    
                    <table>
                        <thead>
                            <tr>
                                <th>Product Name</th>
                                <th>SKU</th>
                                <th>Current Stock</th>
                                <th>Reorder Level</th>
                                <th>Supplier</th>
                            </tr>
                        </thead>
                        <tbody>";

            foreach (var product in lowStockProducts)
            {
                html += $@"
                            <tr>
                                <td>{product.Name}</td>
                                <td>{product.SKU}</td>
                                <td style='color: {(product.StockQuantity == 0 ? "red" : "orange")}; font-weight: bold;'>{product.StockQuantity}</td>
                                <td>{product.ReorderLevel}</td>
                                <td>{(product.Supplier?.Name ?? "No Supplier")}</td>
                            </tr>";
            }

            html += $@"
                        </tbody>
                    </table>
                    
                    <p>Please take appropriate action to restock these items.</p>
                    <p><em>This is an automated message from your Inventory System.</em></p>
                </body>
                </html>";

            return html;
        }
    }
}