using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using KAP_InventoryManager.Model;
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
        public void GenerateCustomerReportPDF(CustomerModel customer, IEnumerable<PaymentModel> payments, string path, string month, string reportType)
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
                                    columns.ConstantColumn(230);
                                });

                                table.Cell().Row(2).Column(1).Element(HeaderBlock).Text("MESSRS");
                                table.Cell().Row(3).Column(1).Element(HeaderBlock).Text("CITY");

                                table.Cell().Row(2).Column(2).Element(Block).Text(customer.Name);
                                table.Cell().Row(3).Column(2).Element(Block).Text(customer.City);

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
                                .PaddingHorizontal(4);
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
                                columns.ConstantColumn(18);
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

                            int counter = 0;
                            decimal totalAmount = 0;
                            foreach (var payment in payments) 
                            {
                                counter++;
                                table.Cell().Element(CellStyle).Text(counter.ToString());
                                table.Cell().Element(CellStyle).AlignCenter().Text(payment.Date.ToString("dd-MM-yyyy"));
                                table.Cell().Element(CellStyle).AlignCenter().Text(payment.InvoiceNo);
                                table.Cell().Element(CellStyle).AlignCenter().Text(payment.PaymentTerm);
                                table.Cell().Element(CellStyle).AlignCenter().Text(payment.Status);
                                table.Cell().Element(CellStyle).AlignLeft().Text(
                                    payment.PaymentTerm == "CASH" ? "1 WEEK" : payment.DueDate.ToString("yyyy-MM-dd")
                                );
                                table.Cell().Element(CellStyle).AlignRight().Text(payment.TotalAmount.ToString("N2"));
                                if (showPaymentColumns)
                                {
                                    table.Cell().Element(CellStyle).AlignCenter().Text(payment.ReceiptNo ?? " ");
                                    table.Cell().Element(CellStyle).AlignCenter().Text(payment.PaymentType ?? " ");
                                    table.Cell().Element(CellStyle).AlignCenter().Text(payment.PaymentDate != default(DateTime) ? payment.PaymentDate.ToString("dd-MM-yyyy") : " ");
                                }

                                totalAmount += payment.TotalAmount;
                            }

                            counter++;
                            table.Cell().Row((uint)counter).Column(6).ColumnSpan(1).Element(TotalCellStyle).Text("TOTAL");
                            table.Cell().Row((uint)counter).Column(7).Element(CellStyle).AlignRight().Text(totalAmount.ToString("N2")).Bold().FontSize(10);
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
