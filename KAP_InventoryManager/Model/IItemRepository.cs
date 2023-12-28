using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IItemRepository
    {
        void Add(ItemModel item);
        void Edit(ItemModel item);
        void Delete(string partNo);
        ItemModel GetByPartNo(string partNo);
        IEnumerable<ItemModel> GetAll();
        //...
    }
}
