using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class ReturnModel
    {
        public int Id { get; set; }
        public string ReturnNo { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCity { get; set; }
        public string RepID { get; set; }
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
