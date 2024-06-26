using System.Collections.Generic;
using System.Threading.Tasks;
using KAP_InventoryManager.Model;

namespace KAP_InventoryManager.Repositories
{
    public interface IItemRepository
    {
        Task AddAsync(ItemModel item);
        Task EditAsync(ItemModel item);
        Task<List<ItemModel>> GetAllAsync();
        Task<IEnumerable<ItemModel>> SearchItemListAsync(string partNo);
        Task<ItemModel> GetByPartNoAsync(string partNo);
        Task<int> GetItemCountAsync();
        Task<int> GetOutOfStockCountAsync();
        Task<int> GetCategoryCountAsync();
        Task<decimal> CalculateCurrentMonthRevenueByItemAsync(string partNo);
        Task<decimal> CalculateLastMonthRevenueByItemAsync(string partNo);
        Task<decimal> CalculateTodayRevenueByItemAsync(string partNo);
        Task<decimal> CalculatePercentageChangeAsync(decimal currentMonthRevenue, decimal lastMonthRevenue);
        bool CheckQty(string partNo, int qty);
        Task<List<string>> SearchPartNoAsync(string searchText);
        Task<List<string>> GetBrandsAsync();
        Task<string> GetSupplierByBrandAsync(string brand);
        Task<List<string>> GetCategoriesAsync(string brandId);
        Task ExportDataToCSVAsync(string brandId);
    }
}
