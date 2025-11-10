using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InventorySalesDashboard.Models;

namespace InventorySalesDashboard.Services
{
    public class InvoiceService
    {
        public byte[] GenerateInvoice(Order order)
        {
            // Set QuestPDF license (Community license is free)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("INVOICE")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            // Company and Order Info
                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("From:").SemiBold();
                                    col.Item().Text("Your Company Name");
                                    col.Item().Text("123 Business Street");
                                    col.Item().Text("City, State 12345");
                                    col.Item().Text("Email: info@yourcompany.com");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("To:").SemiBold();
                                    col.Item().Text(order.CustomerName);
                                    col.Item().Text($"Order #: {order.Id}");
                                    col.Item().Text($"Date: {order.OrderDate:yyyy-MM-dd}");
                                });
                            });

                            // Line items table
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Product");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var line in order.OrderLines)
                                {
                                    table.Cell().Element(CellStyle).Text(line.Product?.Name ?? "Product");
                                    table.Cell().Element(CellStyle).AlignRight().Text(line.Quantity.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text(line.UnitPrice.ToString("C"));
                                    table.Cell().Element(CellStyle).AlignRight().Text((line.Quantity * line.UnitPrice).ToString("C"));

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });

                            // Total
                            x.Item().AlignRight().Text($"Grand Total: {order.Total.ToString("C")}").SemiBold().FontSize(14);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Thank you for your business!");
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}