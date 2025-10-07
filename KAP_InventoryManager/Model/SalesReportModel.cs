using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class SalesReportModel
    {
        public CustomerModel Customer {  get; set; }
        public InvoiceModel Invoice { get; set; }
    }
}
