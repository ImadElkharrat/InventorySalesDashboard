using ClosedXML.Excel;
using InventorySalesDashboard.Models;

namespace InventorySalesDashboard.Services
{
    public class ExcelExportService
    {
        public byte[] ExportProductsToExcel(List<Product> products)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Name";
                worksheet.Cell(currentRow, 3).Value = "SKU";
                worksheet.Cell(currentRow, 4).Value = "Price";
                worksheet.Cell(currentRow, 5).Value = "Cost Price";
                worksheet.Cell(currentRow, 6).Value = "Stock Quantity";
                worksheet.Cell(currentRow, 7).Value = "Reorder Level";
                worksheet.Cell(currentRow, 8).Value = "Category";
                worksheet.Cell(currentRow, 9).Value = "Supplier";
                worksheet.Cell(currentRow, 10).Value = "Stock Status";

                // Format header
                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.Bold = true;

                // Data
                foreach (var product in products)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = product.Id;
                    worksheet.Cell(currentRow, 2).Value = product.Name;
                    worksheet.Cell(currentRow, 3).Value = product.SKU;
                    worksheet.Cell(currentRow, 4).Value = product.Price;
                    worksheet.Cell(currentRow, 5).Value = product.CostPrice;
                    worksheet.Cell(currentRow, 6).Value = product.StockQuantity;
                    worksheet.Cell(currentRow, 7).Value = product.ReorderLevel;
                    worksheet.Cell(currentRow, 8).Value = product.Category;
                    worksheet.Cell(currentRow, 9).Value = product.Supplier?.Name;

                    var statusCell = worksheet.Cell(currentRow, 10);
                    if (product.StockQuantity == 0)
                    {
                        statusCell.Value = "Out of Stock";
                        statusCell.Style.Font.FontColor = XLColor.Red;
                    }
                    else if (product.StockQuantity <= product.ReorderLevel)
                    {
                        statusCell.Value = "Low Stock";
                        statusCell.Style.Font.FontColor = XLColor.Orange;
                    }
                    else
                    {
                        statusCell.Value = "In Stock";
                        statusCell.Style.Font.FontColor = XLColor.Green;
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] ExportOrdersToExcel(List<Order> orders)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Orders");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "Order ID";
                worksheet.Cell(currentRow, 2).Value = "Customer";
                worksheet.Cell(currentRow, 3).Value = "Order Date";
                worksheet.Cell(currentRow, 4).Value = "Total Amount";
                worksheet.Cell(currentRow, 5).Value = "Number of Items";

                // Format header
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerRange.Style.Font.Bold = true;

                // Data
                foreach (var order in orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.Id;
                    worksheet.Cell(currentRow, 2).Value = order.CustomerName;
                    worksheet.Cell(currentRow, 3).Value = order.OrderDate;
                    worksheet.Cell(currentRow, 4).Value = order.Total;
                    worksheet.Cell(currentRow, 5).Value = order.OrderLines?.Sum(ol => ol.Quantity) ?? 0;
                }

                // Add summary
                currentRow += 2;
                worksheet.Cell(currentRow, 1).Value = "Summary";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow + 1, 1).Value = "Total Orders:";
                worksheet.Cell(currentRow + 1, 2).Value = orders.Count;
                worksheet.Cell(currentRow + 2, 1).Value = "Total Revenue:";
                worksheet.Cell(currentRow + 2, 2).Value = orders.Sum(o => o.Total);

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}