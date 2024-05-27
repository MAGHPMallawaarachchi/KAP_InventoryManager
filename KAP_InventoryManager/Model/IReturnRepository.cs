using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IReturnRepository
    {
        bool AddReturn(ReturnModel vReturn);
        void AddReturnItem(ReturnItemModel returnItem, string invoiceNo);
        string GetNextReturnNumber();
        Task<IEnumerable<ReturnModel>> GetAllReturnsAsync();
        Task<IEnumerable<ReturnModel>> SearchReturnListAsync(string returnNo);
        Task<ReturnModel> GetByReturnNo(string returnNo);
        Task<IEnumerable<ReturnItemModel>> GetReturnItems(string returnNo);
    }
}
