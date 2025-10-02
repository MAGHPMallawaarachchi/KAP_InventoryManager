using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class PaymentModel
    {
        public int PaymentID { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerId { get; set; }
        public string PaymentType { get; set; }
        public string ReceiptNo { get; set; }
        public string ChequeNo { get; set; }
        public string Bank { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
    }
}
