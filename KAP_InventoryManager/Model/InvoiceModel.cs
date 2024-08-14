using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class InvoiceModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string Terms { get; set; }
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDateString { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string CustomerID { get; set; }
        public string RepID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCity { get; set; }
    }
}
