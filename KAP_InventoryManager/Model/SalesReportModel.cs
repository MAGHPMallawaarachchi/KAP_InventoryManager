using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class SalesReportModel
    {
        public string CustomerName { get; set; }
        public string CustomerCity { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentTerm { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public string ReceiptNo { get; set; }
        public string ChequeNo { get; set; }
        public string Bank { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Comment { get; set; }
    }
}
