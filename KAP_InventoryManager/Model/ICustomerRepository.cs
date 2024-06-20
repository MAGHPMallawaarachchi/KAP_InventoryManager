using KAP_InventoryManager.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Repositories
{
    internal interface ICustomerRepository
    {
        Task AddAsync(CustomerModel customer);
        Task<IEnumerable<CustomerModel>> GetAllAsync();
        Task<IEnumerable<string>> SearchCustomerAsync(string searchText);
        Task<IEnumerable<CustomerModel>> SearchCustomerListAsync(string customerId);
        Task<CustomerModel> GetByCustomerIDAsync(string customerID);
    }
}
