using KAP_InventoryManager.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Utils
{
    public class SalesReport
    {
        public void GenerateSalesReportPDF(IEnumerable<SalesReportModel> invoices, string path, string month)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            var directoryPath = $@"{path}sales-reports\{month}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{date}.pdf";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(18);

                    IContainer HeaderCellStyle(IContainer cont)
                    {
                        return cont
                        .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri).Bold())
                        .Border((float)0.6)
                        .Background(Colors.Grey.Lighten2)
                        .ShowOnce()
                        .MinHeight(16)
                        .AlignCenter()
                        .AlignMiddle();
                    }

                    page.Content()
                    .Column(c =>
                    {

                        c.Item().PaddingTop(10)
                        .Table(table =>
                        {
                            IContainer CellStyle(IContainer cont)
                            {
                                return cont
                                .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri))
                                .BorderVertical((float)0.6)
                                .BorderBottom((float)0.6)
                                .ShowOnce()
                                .AlignMiddle()
                                .PaddingHorizontal(4)
                                .PaddingVertical(4);
                            }

                            IContainer TotalCellStyle(IContainer cont)
                            {
                                return cont
                                .DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri).Bold())
                                .Border((float)0.6)
                                .Background(Colors.Grey.Lighten2)
                                .ShowOnce()
                                .AlignRight()
                                .AlignMiddle()
                                .PaddingRight(4);
                            }

                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(24);
                                columns.ConstantColumn(56);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(36);
                                columns.RelativeColumn();
                                columns.ConstantColumn(75);
                                columns.ConstantColumn(56);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("NO");
                                header.Cell().Element(HeaderCellStyle).Text("DATE");
                                header.Cell().Element(HeaderCellStyle).Text("INVOICE NO");
                                header.Cell().Element(HeaderCellStyle).Text("TERM");
                                header.Cell().Element(HeaderCellStyle).Text("CUSTOMER");
                                header.Cell().Element(HeaderCellStyle).Text("CITY");
                                header.Cell().Element(HeaderCellStyle).Text("DUE DATE");
                                header.Cell().Element(HeaderCellStyle).Text("AMOUNT (Rs)");
                            });

                            int counter = 0;
                            decimal totalAmount = 0;
                            foreach (var invoice in invoices)
                            {
                                counter++;
                                table.Cell().Element(CellStyle).Text(counter.ToString());
                                table.Cell().Element(CellStyle).AlignCenter().Text(invoice.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).AlignCenter().Text(invoice.InvoiceNo);
                                table.Cell().Element(CellStyle).AlignCenter().Text(invoice.PaymentTerm);
                                table.Cell().Element(CellStyle).AlignLeft().Text(invoice.CustomerName);
                                table.Cell().Element(CellStyle).AlignLeft().Text(invoice.CustomerCity);
                                table.Cell().Element(CellStyle).AlignLeft().Text(
                                    invoice.PaymentTerm == "CASH" ? "1 WEEK" : invoice.DueDate.ToString("yyyy-MM-dd")
                                );
                                table.Cell().Element(CellStyle).AlignRight().Text(invoice.Amount.ToString("N2"));

                                totalAmount += invoice.Amount;
                            }

                            counter++;
                            table.Cell().Row((uint)counter).Column(7).ColumnSpan(1).Element(TotalCellStyle).Text("TOTAL");
                            table.Cell().Row((uint)counter).Column(8).Element(CellStyle).AlignRight().Text(totalAmount.ToString("N2")).Bold().FontSize(10);
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
