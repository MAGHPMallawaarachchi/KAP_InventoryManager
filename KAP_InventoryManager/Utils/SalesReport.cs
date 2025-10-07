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
        public void GenerateSalesReportExcel(IEnumerable<InvoiceModel> invoices, IEnumerable<ReturnModel> returns, string path, string month, string reportType, DateTime start, DateTime end)
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

                worksheet.Cells[1, 1, 1, 3].Merge = true;
                worksheet.Cells[1, 1].Value = month + " (" + start.ToString("yyyy-MM-dd") + " - " + end.ToString("yyyy-MM-dd") + ")";
                worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

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

                // Data Rows - Only invoices (not returns)
                foreach (var invoice in invoices)
                {
                    counter++;

                    worksheet.Cells[rowCounter, 1].Value = counter;

                    worksheet.Cells[rowCounter, 2].Value = invoice.Date;
                    worksheet.Cells[rowCounter, 2].Style.Numberformat.Format = "yyyy-MM-dd";

                    worksheet.Cells[rowCounter, 3].Value = invoice.InvoiceNo;

                    worksheet.Cells[rowCounter, 4].Value = invoice.Terms;

                    worksheet.Cells[rowCounter, 5].Value = invoice.Status;

                    worksheet.Cells[rowCounter, 6].Value = invoice.CustomerName;

                    worksheet.Cells[rowCounter, 7].Value = invoice.CustomerCity;

                    worksheet.Cells[rowCounter, 8].Value = invoice.Terms == "CASH" ? "1 WEEK" : invoice.DueDate.ToString("yyyy-MM-dd");

                    worksheet.Cells[rowCounter, 9].Value = invoice.TotalAmount;
                    worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";

/*                    if (showPaymentColumns)
                    {
                        worksheet.Cells[rowCounter, 10].Value = invoice.ReceiptNo ?? " ";
                        worksheet.Cells[rowCounter, 11].Value = invoice.PaymentType ?? " ";
                        worksheet.Cells[rowCounter, 12].Value = invoice.ChequeNo ?? " ";
                        worksheet.Cells[rowCounter, 13].Value = invoice.Bank ?? " ";
                        worksheet.Cells[rowCounter, 14].Value = invoice.PaymentDate != default(DateTime) ? invoice.PaymentDate.ToString("yyyy-MM-dd") : " ";

                        worksheet.Cells[rowCounter, 15].Value = invoice.PaymentAmount;
                        worksheet.Cells[rowCounter, 15].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[rowCounter, 16].Value = invoice.Comment ?? " ";
                    }*/

                    worksheet.Cells[rowCounter, 1, rowCounter, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 10, rowCounter, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 16 : 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    rowCounter++;

                    totalAmount = totalAmount + invoice.TotalAmount;
                }

                // Invoice Total Row
                worksheet.Cells[rowCounter, 8].Value = "INVOICE TOTAL";
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

                rowCounter += 3; // Add some space

                if (returns.Any())
                {
                    // Returns Header
                    worksheet.Cells[rowCounter, 1].Value = "RETURNS";
                    worksheet.Cells[rowCounter, 1].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 1].Style.Font.Size = 12;
                    rowCounter += 2;

                    // Returns Table Headers
                    worksheet.Cells[rowCounter, 1].Value = "NO";
                    worksheet.Cells[rowCounter, 2].Value = "DATE";
                    worksheet.Cells[rowCounter, 3, rowCounter, 4].Merge = true;
                    worksheet.Cells[rowCounter, 3].Value = "RETURN NO";
                    worksheet.Cells[rowCounter, 5, rowCounter, 6].Merge = true;
                    worksheet.Cells[rowCounter, 5].Value = "INVOICE NO";
                    worksheet.Cells[rowCounter, 7].Value = "CUSTOMER";
                    worksheet.Cells[rowCounter, 8].Value = "CITY";
                    worksheet.Cells[rowCounter, 9].Value = "AMOUNT (Rs)";

                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    rowCounter++;

                    // Returns Data
                    int returnCounter = 0;
                    decimal totalReturnAmount = 0;

                    foreach (var returnItem in returns)
                    {
                        returnCounter++;

                        worksheet.Cells[rowCounter, 1].Value = returnCounter;
                        worksheet.Cells[rowCounter, 2].Value = returnItem.Date;
                        worksheet.Cells[rowCounter, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                        worksheet.Cells[rowCounter, 3, rowCounter, 4].Merge = true;
                        worksheet.Cells[rowCounter, 3].Value = returnItem.ReturnNo;
                        worksheet.Cells[rowCounter, 5, rowCounter, 6].Merge = true;
                        worksheet.Cells[rowCounter, 5].Value = returnItem.InvoiceNo;
                        worksheet.Cells[rowCounter, 7].Value = returnItem.CustomerName;
                        worksheet.Cells[rowCounter, 8].Value = returnItem.CustomerCity;
                        worksheet.Cells[rowCounter, 9].Value = returnItem.TotalAmount;
                        worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[rowCounter, 1, rowCounter, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowCounter, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                        rowCounter++;
                        totalReturnAmount += returnItem.TotalAmount;
                    }

                    // Returns Total Row
                    worksheet.Cells[rowCounter, 8].Value = "RETURNS TOTAL";
                    worksheet.Cells[rowCounter, 8].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowCounter, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[rowCounter, 9].Value = totalReturnAmount;
                    worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[rowCounter, 9].Style.Font.Bold = true;

                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    rowCounter += 2;

                    // Net Total Row
                    worksheet.Cells[rowCounter, 8].Value = "NET TOTAL";
                    worksheet.Cells[rowCounter, 8].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 8].Style.Font.Size = 12;
                    worksheet.Cells[rowCounter, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowCounter, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[rowCounter, 9].Value = totalAmount - totalReturnAmount;
                    worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[rowCounter, 9].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 9].Style.Font.Size = 12;

                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                else
                {
                    rowCounter += 2;

                    // Net Total Row (same as invoice total when no returns)
                    worksheet.Cells[rowCounter, 8].Value = "NET TOTAL";
                    worksheet.Cells[rowCounter, 8].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 8].Style.Font.Size = 12;
                    worksheet.Cells[rowCounter, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowCounter, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[rowCounter, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[rowCounter, 9].Value = totalAmount;
                    worksheet.Cells[rowCounter, 9].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[rowCounter, 9].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 9].Style.Font.Size = 12;

                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 8, rowCounter, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // AutoFit Columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the file
                FileInfo fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }
        }

        public void GenerateSalesReportPDF(IEnumerable<InvoiceModel> invoices, IEnumerable<ReturnModel> returns, string path, string month, string reportType, DateTime start, DateTime end)
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
            decimal totalAmount = 0;
            decimal totalReturnAmount = 0;

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
                                .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri).Bold())
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

                            foreach (var invoice in invoices)
                            {
                                counter++;
                                table.Cell().Element(CellStyle).Text(counter.ToString());
                                table.Cell().Element(CellStyle).AlignCenter().Text(invoice.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).AlignCenter().Text(invoice.InvoiceNo);
                                table.Cell().Element(CellStyle).AlignCenter().Text(invoice.Terms);
                                table.Cell().Element(CellStyle).AlignLeft().Text(invoice.CustomerName);
                                table.Cell().Element(CellStyle).AlignLeft().Text(invoice.CustomerCity);
                                table.Cell().Element(CellStyle).AlignLeft().Text(
                                    invoice.Terms == "CASH" ? "1 WEEK" : invoice.DueDate.ToString("yyyy-MM-dd")
                                );
                                table.Cell().Element(CellStyle).AlignRight().Text(invoice.TotalAmount.ToString("N2"));

                                totalAmount = totalAmount + invoice.TotalAmount;
                            }

                            counter++;
                            table.Cell().Row((uint)counter).Column(7).ColumnSpan(1).Element(TotalCellStyle).Text("INVOICE TOTAL");
                            table.Cell().Row((uint)counter).Column(8).Element(CellStyle).AlignRight().Text(totalAmount.ToString("N2")).Bold().FontSize(10);
                        });

                        // Returns Section                        
                        if (returns.Any())
                        {
                            c.Item().PaddingTop(20).Text("RETURNS").FontSize(12).FontFamily(Fonts.Calibri).Bold();

                            c.Item().PaddingTop(10)
                            .Table(returnsTable =>
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
                                    .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri).Bold())
                                    .Border((float)0.6)
                                    .Background(Colors.Grey.Lighten2)
                                    .ShowOnce()
                                    .AlignRight()
                                    .AlignMiddle()
                                    .PaddingRight(4);
                                }

                                returnsTable.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(24);  // NO
                                    columns.ConstantColumn(56);  // DATE
                                    columns.ConstantColumn(90);  // RETURN NO
                                    columns.ConstantColumn(70);  // INVOICE NO
                                    columns.RelativeColumn();    // CUSTOMER
                                    columns.ConstantColumn(75);  // CITY
                                    columns.ConstantColumn(70);  // AMOUNT
                                });

                                returnsTable.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("NO");
                                    header.Cell().Element(HeaderCellStyle).Text("DATE");
                                    header.Cell().Element(HeaderCellStyle).Text("RETURN NO");
                                    header.Cell().Element(HeaderCellStyle).Text("INVOICE NO");
                                    header.Cell().Element(HeaderCellStyle).Text("CUSTOMER");
                                    header.Cell().Element(HeaderCellStyle).Text("CITY");
                                    header.Cell().Element(HeaderCellStyle).Text("AMOUNT (Rs)");
                                });

                                int returnCounter = 0;

                                foreach (var returnItem in returns)
                                {
                                    returnCounter++;
                                    returnsTable.Cell().Element(CellStyle).Text(returnCounter.ToString());
                                    returnsTable.Cell().Element(CellStyle).AlignCenter().Text(returnItem.Date.ToString("yyyy-MM-dd"));
                                    returnsTable.Cell().Element(CellStyle).AlignCenter().Text(returnItem.ReturnNo);
                                    returnsTable.Cell().Element(CellStyle).AlignCenter().Text(returnItem.InvoiceNo);
                                    returnsTable.Cell().Element(CellStyle).AlignLeft().Text(returnItem.CustomerName);
                                    returnsTable.Cell().Element(CellStyle).AlignLeft().Text(returnItem.CustomerCity);
                                    returnsTable.Cell().Element(CellStyle).AlignRight().Text(returnItem.TotalAmount.ToString("N2"));

                                    totalReturnAmount += returnItem.TotalAmount;
                                }

                                returnCounter++;
                                returnsTable.Cell().Row((uint)returnCounter).Column(6).ColumnSpan(1).Element(TotalCellStyle).Text("RETURNS TOTAL");
                                returnsTable.Cell().Row((uint)returnCounter).Column(7).Element(CellStyle).AlignRight().Text(totalReturnAmount.ToString("N2")).Bold().FontSize(10);
                            });

                            // Net Total
                            c.Item().PaddingTop(20)
                            .Table(netTable =>
                            {

                                QuestPDF.Infrastructure.IContainer TotalCellStyle(QuestPDF.Infrastructure.IContainer cont)
                                {
                                    return cont
                                    .DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri).Bold())
                                    .Border((float)0.6)
                                    .Background(Colors.Grey.Lighten2)
                                    .ShowOnce()
                                    .AlignRight()
                                    .AlignMiddle()
                                    .PaddingRight(4);
                                }

                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cont)
                                {
                                    return cont
                                    .DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri))
                                    .Border((float)0.6)
                                    .ShowOnce()
                                    .AlignMiddle()
                                    .PaddingHorizontal(4)
                                    .PaddingVertical(4);
                                }

                                netTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(70);
                                });

                                netTable.Cell().Element(TotalCellStyle).Text("NET TOTAL");
                                netTable.Cell().Element(CellStyle).AlignRight().Text((totalAmount - totalReturnAmount).ToString("N2")).Bold();
                            });
                        }
                        else
                        {
                            // Net Total (same as invoice total when no returns)
                            c.Item().PaddingTop(20)
                            .Table(netTable =>
                            {
                                QuestPDF.Infrastructure.IContainer TotalCellStyle(QuestPDF.Infrastructure.IContainer cont)
                                {
                                    return cont
                                    .DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri).Bold())
                                    .Border((float)0.6)
                                    .Background(Colors.Grey.Lighten2)
                                    .ShowOnce()
                                    .AlignRight()
                                    .AlignMiddle()
                                    .PaddingRight(4);
                                }

                                QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer cont)
                                {
                                    return cont
                                    .DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri))
                                    .Border((float)0.6)
                                    .ShowOnce()
                                    .AlignMiddle()
                                    .PaddingHorizontal(4)
                                    .PaddingVertical(4);
                                }

                                netTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(70);
                                });

                                netTable.Cell().Element(TotalCellStyle).Text("NET TOTAL");
                                netTable.Cell().Element(CellStyle).AlignRight().Text(totalAmount.ToString("N2")).Bold();
                            });
                        }
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
