using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class SalesReportModel
    {
        public DateTime Date { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentTerm { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCity { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string ReturnNo { get; set; }
        public decimal ReturnAmount { get; set; }
    }
}
