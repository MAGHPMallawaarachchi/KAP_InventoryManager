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
    public class InvoiceDocument
    {
        public void GenerateInvoicePDF(string invoiceNo, CustomerModel customer, InvoiceModel invoice, IEnumerable<InvoiceItemModel> invoiceItems, string path, string shopName, int totalQty)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            //var filePath = @"C:\Users\Hasini\OneDrive\Documents\Kamal Auto Parts\invoices\"+ invoiceNo +".pdf";

            var directoryPath = $@"{path}invoices";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = $@"{directoryPath}\{invoiceNo}.pdf";
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "logo_color.png");

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
                        .AlignCenter()
                        .AlignMiddle();
                    }

                    page.Content()
                    .Column(column =>
                    {
                        column.Spacing(10);
                        column.Item()
                        .Row(row =>
                        {
                            row.Spacing(0);

                            row.ConstantItem(345).Column(c =>
                            {
                                c.Spacing(2);

                                c.Item()
                                .Row(r =>
                                {
                                    r.Spacing(10);
                                    r.ConstantItem(25).Image(logoPath);
                                    r.RelativeItem().Text("KAMAL AUTO PARTS (PVT) LTD").Bold().FontSize(18).FontFamily(Fonts.Calibri);
                                });

                                c.Item().Text("NO. 101/50, ANIYAKANDA WATTA, NAGODA,").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                                c.Item().Text("KANDANA 11320.").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                                c.Item().Text("PHONE: 011-2248131 / 071-9509700").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                                c.Item().Text("EMAIL: kamalautoparts19@gmail.com").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                            });

                            row.AutoItem().PaddingHorizontal(10).LineVertical((float)0.6);

                            row.RelativeItem().AlignCenter().AlignMiddle().Column(c =>
                            {
                                c.Spacing(-8);
                                c.Item().AlignCenter().Text("INVOICE").FontSize(28).FontFamily(Fonts.Calibri).Bold();
                                c.Item().AlignCenter().Text(invoiceNo).FontSize(18).FontFamily(Fonts.Calibri).Bold();
                            });
                        });

                        column.Item().LineHorizontal((float)0.6);

                        column.Item()
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

                                table.Cell().Row(1).ColumnSpan(2).Element(HeaderCellStyle).AlignCenter().Text("CUSTOMER");
                                table.Cell().Row(2).Column(1).Element(HeaderBlock).Text("MESSRS");
                                table.Cell().Row(3).Column(1).Element(HeaderBlock).Text("ADDRESS");
                                table.Cell().Row(4).Column(1).Element(HeaderBlock).Text("CITY");
                                table.Cell().Row(5).Column(1).Element(HeaderBlock).Text("PHONE");
                                table.Cell().Row(6).Column(1).Element(HeaderBlock).Text("E-MAIL");
                                table.Cell().Row(7).ColumnSpan(2).AlignLeft().Text(shopName).FontSize(9).FontFamily(Fonts.Calibri);

                                table.Cell().Row(2).Column(2).Element(Block).Text(customer.Name);
                                table.Cell().Row(3).Column(2).Element(Block).Text(customer.Address);
                                table.Cell().Row(4).Column(2).Element(Block).Text(customer.City);
                                table.Cell().Row(5).Column(2).Element(Block).Text(customer.ContactNo);
                                table.Cell().Row(6).Column(2).Element(Block).Text(customer.Email);

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

                            row.RelativeItem().AlignRight()
                            .Column(c =>
                            {
                                IContainer CellStyle(IContainer cont)
                                {
                                    return cont
                                        .DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri).ExtraBold())
                                        .BorderVertical((float)0.6)
                                        .BorderBottom((float)0.6)
                                        .ShowOnce()
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .PaddingHorizontal(4);
                                }

                                c.Spacing(10);
                                c.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(100);
                                        columns.ConstantColumn(100);
                                    });

                                    table.Cell().Row(1).Column(1).Element(HeaderCellStyle).Text("INVOICE NO");
                                    table.Cell().Row(1).Column(2).Element(HeaderCellStyle).Text("DATE");

                                    table.Cell().Row(2).Column(1).Element(CellStyle).Text(invoiceNo);
                                    table.Cell().Row(2).Column(2).Element(CellStyle).Text((DateTime.Now).ToString("yyyy-MM-dd hh.mmtt"));
                                });

                                c.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(100);
                                        columns.ConstantColumn(100);
                                    });

                                    table.Cell().Row(1).Column(1).Element(HeaderCellStyle).Text("CUSTOMER ID");
                                    table.Cell().Row(1).Column(2).Element(HeaderCellStyle).Text("TERMS");

                                    table.Cell().Row(2).Column(1).Element(CellStyle).Text(invoice.CustomerID);
                                    table.Cell().Row(2).Column(2).Element(CellStyle).Text(invoice.Terms);

                                });

                                c.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(100);
                                        columns.ConstantColumn(100);
                                    });

                                    table.Cell().Row(1).Column(1).Element(HeaderCellStyle).Text("SALES REP");
                                    table.Cell().Row(1).Column(2).Element(HeaderCellStyle).Text("DUE DATE");

                                    table.Cell().Row(2).Column(1).Element(CellStyle).Text(invoice.RepID);
                                    table.Cell().Row(2).Column(2).Element(CellStyle).Text((invoice.DueDate).ToString("yyyy-MM-dd hh.mmtt"));

                                });
                            });

                        });

                        column.Item().PaddingTop(10)
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
                                    .PaddingVertical(3)
                                    .PaddingHorizontal(4);
                            }

                            IContainer TotalCellStyle(IContainer cont)
                            {
                                return cont
                                    .DefaultTextStyle(x => x.FontSize(13).FontFamily(Fonts.Calibri).Bold())
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
                                columns.ConstantColumn(75);
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(24);
                                columns.ConstantColumn(48);
                                columns.ConstantColumn(24);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("NO");
                                header.Cell().Element(HeaderCellStyle).Text("PART NO");
                                header.Cell().Element(HeaderCellStyle).Text("BRAND");
                                header.Cell().Element(HeaderCellStyle).Text("DESCRIPTION");
                                header.Cell().Element(HeaderCellStyle).Text("QTY");
                                header.Cell().Element(HeaderCellStyle).Text("U/PRICE");
                                header.Cell().Element(HeaderCellStyle).Text("DIS%");
                                header.Cell().Element(HeaderCellStyle).Text("AMOUNT");


                            });

                            int counter = 0;
                            foreach (var item in invoiceItems)
                            {
                                counter++;
                                table.Cell().Element(CellStyle).Text(counter.ToString());
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.PartNo);
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.BrandID);
                                table.Cell().Element(CellStyle).AlignLeft().Text(item.Description.Length > 55 ? item.Description.Substring(0, 55) + "..." : item.Description);
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.UnitPrice.ToString("N2"));
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Discount.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Amount.ToString("N2"));
                            }

                            counter++;
                            table.Cell().Row((uint)counter).Column(4).AlignMiddle().Text("Thank you for your business!").FontFamily(Fonts.Calibri).Bold().FontSize(9);
                            table.Cell().Row((uint)counter).Column(5).ColumnSpan(1).Element(CellStyle).Text(totalQty.ToString());
                            table.Cell().Row((uint)counter).Column(6).ColumnSpan(2).Element(TotalCellStyle).Text("TOTAL");
                            table.Cell().Row((uint)counter).Column(8).Element(CellStyle).AlignRight().Text(invoice.TotalAmount.ToString("N2")).Bold().FontSize(11);
                        });

                        column.Item().PaddingTop(30)
                        .Row(r =>
                        {
                            r.RelativeItem()
                            .Column(c =>
                            {
                                c.Item().AlignCenter().Text(".............................").FontSize(9).FontFamily(Fonts.Calibri);
                                c.Item().AlignCenter().Text("CHECKED BY").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                            });

                            r.RelativeItem()
                            .Column(c =>
                            {
                                c.Item().AlignCenter().Text(".............................").FontSize(9).FontFamily(Fonts.Calibri);
                                c.Item().AlignCenter().Text("RECEIVER'S NAME & STAMP").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                            });
                        });

                        column.Item().AlignLeft()
                        .Column(c =>
                        {
                            c.Item().Text("If you have any questions about this invoice, please contact").FontSize(9).FontFamily(Fonts.Calibri);
                            c.Item().Text("[Chamindu, 077-7541984]").FontSize(9).FontFamily(Fonts.Calibri).Bold();
                        });
                    });
                });
            }).GeneratePdf(filePath);

            PrintPDF("LBP6030w/6018w", "A4", filePath, 2);
        }

        public bool PrintPDF(string printer, string paperName, string filename, int copies)
        {
            try
            {
                // Create the printer settings for our printer
                var printerSettings = new PrinterSettings
                {
                    PrinterName = printer,
                    Copies = (short)copies,
                };

                // Create our page settings for the paper size selected
                var pageSettings = new PageSettings(printerSettings)
                {
                    Margins = new Margins(0, 0, 0, 0),
                };
                foreach (PaperSize paperSize in printerSettings.PaperSizes)
                {
                    if (paperSize.PaperName == paperName)
                    {
                        pageSettings.PaperSize = paperSize;
                        break;
                    }
                }

                // Now print the PDF document
                using (var document = PdfDocument.Load(filename))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.DefaultPageSettings = pageSettings;
                        printDocument.PrintController = new StandardPrintController();
                        printDocument.Print();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}