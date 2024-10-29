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
        public void GenerateSalesReportExcel(IEnumerable<SalesReportModel> invoices, string path, string month, string reportType, DateTime start, DateTime end)
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
            bool showPaymentColumns = reportType != "only pending or overdue";

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sales Report");

                worksheet.DefaultRowHeight = 18;

                worksheet.Cells[1, 2].Value = month + " (" + start.ToString("yyyy-MM-dd") + " - " + end.ToString("yyyy-MM-dd") + ")";
                worksheet.Cells[1, 2].Style.Font.Bold = true;

                // Header for Invoices
                worksheet.Cells[3, 1].Value = "NO";
                worksheet.Cells[3, 2].Value = "DATE";
                worksheet.Cells[3, 3].Value = "INVOICE NO";
                worksheet.Cells[3, 4].Value = "TERM";
                worksheet.Cells[3, 5].Value = "STATUS";
                worksheet.Cells[3, 6].Value = "CUSTOMER";
                worksheet.Cells[3, 7].Value = "CITY";
                worksheet.Cells[3, 8].Value = "DUE DATE";
                worksheet.Cells[3, 9].Value = "AMOUNT (Rs)";
                if (showPaymentColumns)
                {
                    worksheet.Cells[3, 10].Value = "RECEIPT NO";
                    worksheet.Cells[3, 11].Value = "PAYMENT TYPE";
                    worksheet.Cells[3, 12].Value = "CHEQUE NO";
                    worksheet.Cells[3, 13].Value = "BANK";
                    worksheet.Cells[3, 14].Value = "PAYMENT DATE";
                    worksheet.Cells[3, 15].Value = "PAYMENT AMOUNT";
                    worksheet.Cells[3, 16].Value = "COMMENT";
                }
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Font.Bold = true;
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, 3, showPaymentColumns ? 16 : 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                int rowCounter = 4;
                int counter = 0;
                decimal totalAmount = 0;

                // Data Rows
                foreach (var invoice in invoices)
                {
                    counter++;

                    worksheet.Cells[rowCounter, 1].Value = counter;

                    worksheet.Cells[rowCounter, 2].Value = invoice.Date;
                    worksheet.Cells[rowCounter, 2].Style.Numberformat.Format = "yyyy-MM-dd";

                    worksheet.Cells[rowCounter, 3].Value = invoice.InvoiceNo;

                    worksheet.Cells[rowCounter, 4].Value = invoice.PaymentTerm;

                    worksheet.Cells[rowCounter, 5].Value = invoice.Status;

                    worksheet.Cells[rowCounter, 6].Value = invoice.CustomerName;

                    worksheet.Cells[rowCounter, 7].Value = invoice.CustomerCity;

                    worksheet.Cells[rowCounter, 8].Value = invoice.PaymentTerm == "CASH" ? "1 WEEK" : invoice.DueDate.ToString("yyyy-MM-dd");

                    worksheet.Cells[rowCounter, 9].Value = invoice.Amount;
                    worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";

                    if (showPaymentColumns)
                    {
                        worksheet.Cells[rowCounter, 10].Value = invoice.ReceiptNo ?? " ";
                        worksheet.Cells[rowCounter, 11].Value = invoice.PaymentType ?? " ";
                        worksheet.Cells[rowCounter, 12].Value = invoice.ChequeNo ?? " ";
                        worksheet.Cells[rowCounter, 13].Value = invoice.Bank ?? " ";
                        worksheet.Cells[rowCounter, 14].Value = invoice.PaymentDate != default(DateTime) ? invoice.PaymentDate.ToString("yyyy-MM-dd") : " ";

                        worksheet.Cells[rowCounter, 15].Value = invoice.PaymentAmount;
                        worksheet.Cells[rowCounter, 15].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[rowCounter, 16].Value = invoice.Comment ?? " ";
                    }

                    worksheet.Cells[rowCounter, 1, rowCounter, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 10, rowCounter, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    rowCounter++;

                    // If there's a return, add return information as a separate row
                    if (invoice.ReturnAmount != 0)
                    {
                        worksheet.Cells[rowCounter, 8].Value = invoice.ReturnNo;
                        worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells[rowCounter, 9].Value = invoice.ReturnAmount * (-1);
                        worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                        rowCounter++;
                    }

                    totalAmount = totalAmount + invoice.Amount - invoice.ReturnAmount;
                }

                // Total Row
                worksheet.Cells[rowCounter, 8].Value = "TOTAL";
                worksheet.Cells[rowCounter, 8].Style.Font.Bold = true;
                worksheet.Cells[rowCounter, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowCounter, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[rowCounter, 9].Value = totalAmount;
                worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowCounter, 9].Style.Font.Bold = true;

                worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // AutoFit Columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the file
                FileInfo fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }
        }

        public void GenerateSalesReportPDF(IEnumerable<SalesReportModel> invoices, string path, string month, string reportType, DateTime start, DateTime end)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            var directoryPath = $@"{path}sales-reports\{month}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{date}.pdf";
            bool showPaymentColumns = reportType != "only pending or overdue";

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
                        c.Item().PaddingTop(10).Text(month + " (" + start.ToString("yyyy-MM-dd") + " - " + end.ToString("yyyy-MM-dd") + ")").FontSize(10).FontFamily(Fonts.Calibri).Bold();

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
