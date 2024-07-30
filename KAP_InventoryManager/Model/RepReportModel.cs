using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class RepReportModel
    {
        public string CustomrName { get; set; }
        public IEnumerable<PaymentModel> Payments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
    }
}
