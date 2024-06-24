using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class ReturnItemModel
    {
        public int No { get; set; }
        public string ReturnNo { get; set; }
        public string PartNo { get; set; }
        public string Description { get; set; }
        public string BrandID { get; set; }
        public decimal BuyingPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int DamagedQty { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
    }
}
