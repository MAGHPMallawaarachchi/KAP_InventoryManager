using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class PaymentModel
    {
        public DateTime Date { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentTerm { get; set;}
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public Decimal TotalAmount { get; set; }
        public string PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
