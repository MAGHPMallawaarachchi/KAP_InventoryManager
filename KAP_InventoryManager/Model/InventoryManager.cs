using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    public static class InventoryManager
    {
        private static ItemModel _selectedInventoryItem;

        public static ItemModel SelectedInventoryItem
        {
            get { return _selectedInventoryItem; }
            set { _selectedInventoryItem = value; }
        }
    }
}
