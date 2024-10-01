using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using KAP_InventoryManager.Model;
using KAP_InventoryManager.Repositories;
using PdfiumViewer;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace KAP_InventoryManager.Utils
{
    public class RepReport
    {
        public void GenerateRepReportPDF(SalesRepModel rep, IEnumerable<RepReportModel> repReports, string path, string month, string reportType)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            var directoryPath = $@"{path}rep-reports\{month}\{reportType}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{rep.Name}-{date}.pdf";
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

                                table.Cell().Row(2).Column(1).Element(HeaderBlock).Text("NAME");
                                table.Cell().Row(2).Column(2).Element(Block).Text(rep.Name);

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

                        foreach (var repReport in repReports)
                        {
                            c.Item().PaddingTop(10).Text(repReport.CustomrName + " - " + repReport.CustomerCity).FontSize(10).FontFamily(Fonts.Calibri).Bold();

                            c.Item().PaddingTop(5).PaddingBottom(10)
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

                                int invoiceCounter = 0;
                                int returnCounter = 0;

                                foreach (var invoice in repReport.Payments)
                                {
                                    invoiceCounter++;
                                    table.Cell().Element(CellStyle).Text(invoiceCounter.ToString());
                                    table.Cell().Element(CellStyle).AlignCenter().Text(invoice.Date.ToString("dd-MM-yyyy"));
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
                                        table.Cell().Element(CellStyle).AlignCenter().Text(invoice.PaymentDate != default(DateTime) ? invoice.PaymentDate.ToString("dd-MM-yyyy") : " ");
                                    }

                                    if (invoice.ReturnAmount != 0)
                                    {
                                        returnCounter++;
                                        table.Cell().Element(CellStyle).Text("");  // Empty row for ReturnNo
                                        table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).AlignCenter().Text(invoice.ReturnNo);
                                        table.Cell().Element(CellStyle).AlignRight().Text("-" + invoice.ReturnAmount.ToString("N2"));

                                        if (showPaymentColumns)
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text("");
                                            table.Cell().Element(CellStyle).AlignCenter().Text("");
                                            table.Cell().Element(CellStyle).AlignCenter().Text("");
                                        }
                                    }
                                }

                                invoiceCounter = invoiceCounter + returnCounter + 1;
                                table.Cell().Row((uint)invoiceCounter).Column(6).ColumnSpan(1).Element(TotalCellStyle).Text("TOTAL");
                                table.Cell().Row((uint)invoiceCounter).Column(7).Element(CellStyle).AlignRight().Text((repReport.TotalAmount - repReport.TotalReturnAmount).ToString("N2")).Bold().FontSize(10);
                            });
                        }

                        c.Item().PaddingTop(15)
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
                                columns.ConstantColumn(19);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("NO");
                                header.Cell().Element(HeaderCellStyle).Text("CUSTOMER NAME");
                                header.Cell().Element(HeaderCellStyle).Text("TOTAL AMOUNT");
                                header.Cell().Element(HeaderCellStyle).Text("COMMISSION AMOUNT");
                            });

                            int summaryCounter = 0;
                            decimal totalAmount = 0;
                            decimal totalCommissionAmount = 0;

                            foreach (var repReport in repReports)
                            {
                                summaryCounter++;
                                table.Cell().Element(CellStyle).Text(summaryCounter.ToString());
                                table.Cell().Element(CellStyle).AlignLeft().Text(repReport.CustomrName);
                                table.Cell().Element(CellStyle).AlignRight().Text((repReport.TotalAmount - repReport.TotalReturnAmount).ToString("N2"));
                                table.Cell().Element(CellStyle).AlignRight().Text(repReport.CommissionAmount.ToString("N2"));

                                totalAmount += (repReport.TotalAmount - repReport.TotalReturnAmount);
                                totalCommissionAmount += repReport.CommissionAmount;
                            }

                            summaryCounter++;
                            table.Cell().Row((uint)summaryCounter).Column(2).ColumnSpan(1).Element(TotalCellStyle).Text("TOTAL");
                            table.Cell().Row((uint)summaryCounter).Column(3).Element(CellStyle).AlignRight().Text(totalAmount.ToString("N2")).Bold().FontSize(10);
                            table.Cell().Row((uint)summaryCounter).Column(4).Element(CellStyle).AlignRight().Text(totalCommissionAmount.ToString("N2")).Bold().FontSize(10);
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }
    }

}
