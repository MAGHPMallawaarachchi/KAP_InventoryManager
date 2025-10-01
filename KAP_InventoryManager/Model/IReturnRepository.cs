using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IReturnRepository
    {
        Task<bool> AddReturnAsync(ReturnModel vReturn);
        Task AddReturnItemAsync(ReturnItemModel returnItem, string invoiceNo);
        Task<string> GetNextReturnNumberAsync();
        Task<IEnumerable<ReturnModel>> GetAllReturnsAsync();
        Task<IEnumerable<ReturnModel>> SearchReturnListAsync(string returnNo);
        Task<ReturnModel> GetByReturnNoAsync(string returnNo);
        Task<IEnumerable<ReturnItemModel>> GetReturnItemsAsync(string returnNo);
        Task<IEnumerable<ReturnModel>> GetReturns(DateTime startDate, DateTime endDate);
    }
}
