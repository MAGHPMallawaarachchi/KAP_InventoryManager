using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class ItemModel
    {
        public string PartNo {  get; set; }
        public string OEMNo { get; set; }
        public string Description { get; set; }
        public string BrandID { get; set; }
        public string Category { get; set; }
        public string SupplierID { get; set; }
        public int TotalQty { get; set; }
        public int QtyInHand { get; set; }
        public int qtySold { get; set; }
        public decimal BuyingPrice { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
