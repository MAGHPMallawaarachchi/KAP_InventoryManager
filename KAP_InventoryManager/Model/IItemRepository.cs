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
        Task<IEnumerable<ItemModel>> GetAllAsync();
        Task<IEnumerable<ItemModel>> SearchItemListAsync(string partNo);
        Task<ItemModel> GetByPartNoAsync(string partNo);
        ItemModel GetByPartNo(string partNo);
        List<string> SearchPartNo(string SearchText);
        Task<int> GetItemCount();
        Task<int> GetOutOfStockCount();
        int GetLowInStockCount();
        Task<int> GetCategoryCount();
        bool CheckQty(string partNo, int qty);
        List<string> GetBrands();
        string GetSupplierByBrand(string brand);
        List<string> GetCategories(string brandId);
        //...
    }
}
