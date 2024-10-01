using KAP_InventoryManager.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Utils
{
    public class SalesReport
    {
        public void GenerateSalesReportExcel(IEnumerable<SalesReportModel> invoices, string path, string month)
        {
            // Ensure EPPlus license is set
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            var directoryPath = $@"{path}sales-reports\{month}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{date}.xlsx";

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sales Report");

                worksheet.DefaultRowHeight = 18;

                // Header Styles
                worksheet.Cells["A1:H1"].Style.Font.Bold = true;
                worksheet.Cells["A1:H1"].Style.Font.Size = 9;
                worksheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells["A1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:H1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Headers
                worksheet.Cells[1, 1].Value = "NO";
                worksheet.Cells[1, 2].Value = "DATE";
                worksheet.Cells[1, 3].Value = "INVOICE NO";
                worksheet.Cells[1, 4].Value = "TERM";
                worksheet.Cells[1, 5].Value = "CUSTOMER";
                worksheet.Cells[1, 6].Value = "CITY";
                worksheet.Cells[1, 7].Value = "DUE DATE";
                worksheet.Cells[1, 8].Value = "AMOUNT (Rs)";

                int rowCounter = 2;
                decimal totalAmount = 0;
                int counter = 0;

                // Data Rows
                foreach (var invoice in invoices)
                {
                    counter++;
                    worksheet.Cells[rowCounter, 1].Value = counter;
                    worksheet.Cells[rowCounter, 2].Value = invoice.Date;
                    worksheet.Cells[rowCounter, 2].Style.Numberformat.Format = "yyyy-mm-dd";
                    worksheet.Cells[rowCounter, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 3].Value = invoice.InvoiceNo;
                    worksheet.Cells[rowCounter, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 4].Value = invoice.PaymentTerm;
                    worksheet.Cells[rowCounter, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 5].Value = invoice.CustomerName;
                    worksheet.Cells[rowCounter, 6].Value = invoice.CustomerCity;

                    if (invoice.PaymentTerm == "CASH")
                    {
                        worksheet.Cells[rowCounter, 7].Value = "1 WEEK";
                    }
                    else
                    {
                        worksheet.Cells[rowCounter, 7].Value = invoice.DueDate;
                        worksheet.Cells[rowCounter, 7].Style.Numberformat.Format = "yyyy-mm-dd";
                    }
                    worksheet.Cells[rowCounter, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells[rowCounter, 8].Value = invoice.Amount;
                    worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[rowCounter, 8].Style.Numberformat.Format = "#,##0.00";

                    totalAmount += invoice.Amount;
                    rowCounter++;
                }

                // Total Row
                worksheet.Cells[rowCounter, 7].Value = "TOTAL";
                worksheet.Cells[rowCounter, 7].Style.Font.Bold = true;
                worksheet.Cells[rowCounter, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[rowCounter, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowCounter, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[rowCounter, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[rowCounter, 8].Value = totalAmount;
                worksheet.Cells[rowCounter, 8].Style.Font.Bold = true;
                worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[rowCounter, 8].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowCounter, 8].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Apply border to all cells with data
                using (var range = worksheet.Cells[1, 1, rowCounter-1, 8])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // AutoFit Columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the file
                FileInfo fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }
        }

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

                    QuestPDF.Infrastructure.IContainer HeaderCellStyle(QuestPDF.Infrastructure.IContainer cont)
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
                            QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cont)
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

                            QuestPDF.Infrastructure.IContainer TotalCellStyle(QuestPDF.Infrastructure.IContainer cont)
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
                            int returnCounter = 0;
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

                                if (invoice.ReturnAmount != 0)
                                {
                                    returnCounter++;
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).Text("");
                                    table.Cell().Element(CellStyle).AlignCenter().Text(invoice.ReturnNo);
                                    table.Cell().Element(CellStyle).AlignRight().Text("-" + invoice.ReturnAmount.ToString("N2"));
                                }

                                totalAmount = totalAmount + invoice.Amount - invoice.ReturnAmount;
                            }

                            counter = counter + returnCounter + 1;
                            table.Cell().Row((uint)counter).Column(7).ColumnSpan(1).Element(TotalCellStyle).Text("TOTAL");
                            table.Cell().Row((uint)counter).Column(8).Element(CellStyle).AlignRight().Text(totalAmount.ToString("N2")).Bold().FontSize(10);
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
