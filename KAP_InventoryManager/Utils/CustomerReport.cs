using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using KAP_InventoryManager.Model;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using PdfiumViewer;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace KAP_InventoryManager.Utils
{
    public class CustomerReport
    {
        public void GenerateCustomerReportPDF(CustomerModel customer, IEnumerable<PaymentModel> invoices, IEnumerable<ReturnModel> returns, string path, string month, string reportType, DateTime start, DateTime end)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            var directoryPath = $@"{path}customer-reports\{month}\{reportType}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{customer.Name}, {customer.City}-{date}.pdf";
            bool showPaymentColumns = reportType != "only pending or overdue";
            decimal totalAmount = 0;
            decimal totalReturnAmount = 0;

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
                        c.Item()
                        .Row(row =>
                        {
                            row.ConstantItem(345)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(55);
                                    columns.ConstantColumn(290);
                                });

                                table.Cell().Row(2).Column(1).Element(HeaderBlock).Text("MESSRS");
                                table.Cell().Row(3).Column(1).Element(HeaderBlock).Text("MONTH");

                                table.Cell().Row(2).Column(2).Element(Block).Text(customer.Name+ " - " +customer.City);
                                table.Cell().Row(3).Column(2).Element(Block).Text(month + " (" + start.ToString("yyyy-MM-dd") + " - " + end.ToString("yyyy-MM-dd") + ")");

                                IContainer HeaderBlock(IContainer cont)
                                {
                                    return cont
                                        .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri).Bold())
                                        .Border((float)0.6)
                                        .Background(Colors.Grey.Lighten2)
                                        .ShowOnce()
                                        .PaddingLeft(4)
                                        .AlignMiddle();
                                }

                                IContainer Block(IContainer cont)
                                {
                                    return cont
                                        .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri).Bold())
                                        .BorderRight((float)0.6)
                                        .BorderHorizontal((float)0.6)
                                        .ShowOnce()
                                        .AlignMiddle()
                                        .PaddingLeft(4);
                                }

                            });
                        });

                        c.Item().EnsureSpace()
                            .Column(column =>
                            {
                                column.Item().PaddingTop(15).PaddingBottom(10)
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
                                        .PaddingHorizontal(4);
                                    }

                                    IContainer TotalCellStyle(IContainer cont)
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
                                        columns.ConstantColumn(55);
                                        columns.ConstantColumn(70);
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(40);
                                        columns.ConstantColumn(55);
                                        columns.RelativeColumn();
                                        if (showPaymentColumns)
                                        {
                                            columns.ConstantColumn(50);
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(HeaderCellStyle).Text("NO");
                                        header.Cell().Element(HeaderCellStyle).Text("DATE");
                                        header.Cell().Element(HeaderCellStyle).Text("INVOICE NO");
                                        header.Cell().Element(HeaderCellStyle).Text("PAYMENT TERM");
                                        header.Cell().Element(HeaderCellStyle).Text("STATUS");
                                        header.Cell().Element(HeaderCellStyle).Text("DUE DATE");
                                        header.Cell().Element(HeaderCellStyle).Text("TOTAL AMOUNT");
                                        if (showPaymentColumns)
                                        {
                                            header.Cell().Element(HeaderCellStyle).Text("RECEIPT NO");
                                            header.Cell().Element(HeaderCellStyle).Text("PAYMENT TYPE");
                                            header.Cell().Element(HeaderCellStyle).Text("PAYMENT DATE");
                                        }
                                    });

                                    int invoiceCounter = 0;

                                    // First, display all invoices
                                    foreach (var invoice in invoices)
                                    {
                                        invoiceCounter++;
                                        table.Cell().Element(CellStyle).Text(invoiceCounter.ToString());
                                        table.Cell().Element(CellStyle).AlignCenter().Text(invoice.Date.ToString("yyyy-MM-dd"));
                                        table.Cell().Element(CellStyle).AlignCenter().Text(invoice.InvoiceNo);
                                        table.Cell().Element(CellStyle).AlignCenter().Text(invoice.PaymentTerm);
                                        table.Cell().Element(CellStyle).AlignCenter().Text(invoice.Status);
                                        table.Cell().Element(CellStyle).AlignLeft().Text(
                                            invoice.PaymentTerm == "CASH" ? "1 WEEK" : invoice.DueDate.ToString("yyyy-MM-dd")
                                        );
                                        table.Cell().Element(CellStyle).AlignRight().Text(invoice.TotalAmount.ToString("N2"));

                                        if (showPaymentColumns)
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text(invoice.ReceiptNo ?? " ");
                                            table.Cell().Element(CellStyle).AlignCenter().Text(invoice.PaymentType ?? " ");
                                            table.Cell().Element(CellStyle).AlignCenter().Text(invoice.PaymentDate != default(DateTime) ? invoice.PaymentDate.ToString("yyyy-MM-dd") : " ");
                                        }

                                        totalAmount += invoice.TotalAmount;
                                    }

                                    // Now display all returns from this customer
                                    if (returns.Any())
                                    {
                                        int returnCounter = 0;
                                        foreach (var returnItem in returns)
                                        {
                                            returnCounter++;
                                            invoiceCounter++;
                                            table.Cell().Element(CellStyle).Text("R" + returnCounter.ToString());
                                            table.Cell().Element(CellStyle).AlignCenter().Text(returnItem.Date.ToString("yyyy-MM-dd"));
                                            table.Cell().ColumnSpan(4).Element(CellStyle).AlignLeft().Text(returnItem.ReturnNo + " (" + returnItem.InvoiceNo + ")");
                                            table.Cell().Element(CellStyle).AlignRight().Text("-" + returnItem.TotalAmount.ToString("N2"));

                                            if (showPaymentColumns)
                                            {
                                                table.Cell().Element(CellStyle).Text("");
                                                table.Cell().Element(CellStyle).Text("");
                                                table.Cell().Element(CellStyle).Text("");
                                            }

                                            totalReturnAmount += returnItem.TotalAmount;
                                        }                                      
                                    }

                                    // Add final net total row
                                    invoiceCounter++;
                                    table.Cell().Row((uint)invoiceCounter).Column(6).ColumnSpan(1).Element(TotalCellStyle).Text("NET TOTAL");
                                    table.Cell().Row((uint)invoiceCounter).Column(7).Element(CellStyle).AlignRight().Text((totalAmount - totalReturnAmount).ToString("N2")).Bold().FontSize(10);

                                });
                            });
                    });
                });
            }).GeneratePdf(filePath);
        }

        public void GenerateCustomerReportExcel(CustomerModel customer, IEnumerable<PaymentModel> invoices, IEnumerable<ReturnModel> returns, string path, string month, string reportType, DateTime start, DateTime end)
        {
            // Ensure EPPlus license is set
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            var directoryPath = $@"{path}customer-reports\{month}\{reportType}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{customer.Name}, {customer.City}-{date}.xlsx";
            bool showPaymentColumns = reportType != "only pending or overdue";
            decimal totalAmount = 0;
            decimal totalReturnAmount = 0;

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Customer Report");

                worksheet.DefaultRowHeight = 18;

                // Header for the Rep Information
                worksheet.Cells[1, 1].Value = "MESSRS";
                worksheet.Cells[2, 1].Value = "MONTH";
                worksheet.Cells[1, 2].Value = customer.Name+ " - " +customer.City;
                worksheet.Cells[2, 2].Value = month+ " (" +start.ToString("yyyy-MM-dd")+ " - "+end.ToString("yyyy-MM-dd")+ ")";

                worksheet.Cells[1, 1, 2, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 2, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 2, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[1, 1, 2, 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, 2, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, 2, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, 2, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[1, 2, 2, 2].Style.Font.Bold = true;

                worksheet.Cells[1, 2, 2, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 2, 2, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 2, 2, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 2, 2, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;


                int rowCounter = 4; // Starting row for the reports

                // Header for Invoices
                worksheet.Cells[rowCounter, 1].Value = "NO";
                worksheet.Cells[rowCounter, 2].Value = "DATE";
                worksheet.Cells[rowCounter, 3].Value = "INVOICE NO";
                worksheet.Cells[rowCounter, 4].Value = "PAYMENT TERM";
                worksheet.Cells[rowCounter, 5].Value = "STATUS";
                worksheet.Cells[rowCounter, 6].Value = "DUE DATE";
                worksheet.Cells[rowCounter, 7].Value = "TOTAL AMOUNT (Rs)";
                if (showPaymentColumns)
                {
                    worksheet.Cells[rowCounter, 8].Value = "RECEIPT NO";
                    worksheet.Cells[rowCounter, 9].Value = "PAYMENT TYPE";
                    worksheet.Cells[rowCounter, 10].Value = "CHEQUE NO";
                    worksheet.Cells[rowCounter, 11].Value = "BANK";
                    worksheet.Cells[rowCounter, 12].Value = "PAYMENT DATE";
                    worksheet.Cells[rowCounter, 13].Value = "PAYMENT AMOUNT";
                    worksheet.Cells[rowCounter, 14].Value = "COMMENT";
                }
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Font.Bold = true;
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                int invoiceCounter = 0;

                // Invoices Data
                foreach (var invoice in invoices)
                {
                    invoiceCounter++;
                    worksheet.Cells[rowCounter, 1].Value = invoiceCounter;

                    worksheet.Cells[rowCounter, 2].Value = invoice.Date;
                    worksheet.Cells[rowCounter, 2].Style.Numberformat.Format = "yyyy-MM-dd";

                    worksheet.Cells[rowCounter, 3].Value = invoice.InvoiceNo;

                    worksheet.Cells[rowCounter, 4].Value = invoice.PaymentTerm;

                    worksheet.Cells[rowCounter, 5].Value = invoice.Status;

                    worksheet.Cells[rowCounter, 6].Value = invoice.PaymentTerm == "CASH" ? "1 WEEK" : invoice.DueDate.ToString("yyyy-MM-dd");

                    worksheet.Cells[rowCounter, 7].Value = invoice.TotalAmount;
                    worksheet.Cells[rowCounter, 7].Style.Numberformat.Format = "#,##0.00";

                    if (showPaymentColumns)
                    {
                        worksheet.Cells[rowCounter, 8].Value = invoice.ReceiptNo ?? " ";
                        worksheet.Cells[rowCounter, 9].Value = invoice.PaymentType ?? " ";
                        worksheet.Cells[rowCounter, 10].Value = invoice.ChequeNo ?? " ";
                        worksheet.Cells[rowCounter, 11].Value = invoice.Bank ?? " ";
                        worksheet.Cells[rowCounter, 12].Value = invoice.PaymentDate != default(DateTime) ? invoice.PaymentDate.ToString("yyyy-MM-dd") : " ";
                        worksheet.Cells[rowCounter, 13].Value = invoice.PaymentAmount;
                        worksheet.Cells[rowCounter, 13].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowCounter, 14].Value = invoice.Comment ?? " ";
                    }

                    worksheet.Cells[rowCounter, 1, rowCounter, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 8, rowCounter, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    totalAmount += invoice.TotalAmount;
                    rowCounter++;
                }

                // Now display all returns from this customer
                if (returns.Any())
                {
                    int returnCounter = 0;
                    foreach (var returnItem in returns)
                    {
                        returnCounter++;
                        worksheet.Cells[rowCounter, 1].Value = "R" + returnCounter.ToString();
                        worksheet.Cells[rowCounter, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells[rowCounter, 2].Value = returnItem.Date;
                        worksheet.Cells[rowCounter, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                        worksheet.Cells[rowCounter, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells[rowCounter, 3, rowCounter, 6].Merge = true;
                        worksheet.Cells[rowCounter, 3].Value = returnItem.ReturnNo + " (" + returnItem.InvoiceNo + ")";
                        worksheet.Cells[rowCounter, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        worksheet.Cells[rowCounter, 7].Value = returnItem.TotalAmount * (-1);
                        worksheet.Cells[rowCounter, 7].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[rowCounter, 1, rowCounter, showPaymentColumns ? 14 : 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                        totalReturnAmount += returnItem.TotalAmount;
                        rowCounter++;
                    }
                }

                // Add final net total row
                worksheet.Cells[rowCounter, 6].Value = "NET TOTAL";
                worksheet.Cells[rowCounter, 6].Style.Font.Bold = true;
                worksheet.Cells[rowCounter, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowCounter, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[rowCounter, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[rowCounter, 7].Value = totalAmount - totalReturnAmount;
                worksheet.Cells[rowCounter, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowCounter, 7].Style.Font.Bold = true;

                worksheet.Cells[rowCounter, 6, rowCounter, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 6, rowCounter, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 6, rowCounter, 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[rowCounter, 6, rowCounter, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                rowCounter++;
                rowCounter++;

            // AutoFit Columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the file
                FileInfo fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }
        }
    }
}
