using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public class InventoryItemModel
    {
        public int AutoID { get; set; }
        public string PartNo { get; set; }
        public int QtyInHand { get; set; }
    }
}
