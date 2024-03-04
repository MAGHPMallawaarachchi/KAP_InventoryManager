using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IReturnRepository
    {
        void AddReturn(ReturnModel vReturn);
        void AddReturnItem(ReturnItemModel returnItem, string invoiceNo);
        string GetNextReturnNumber();
    }
}
