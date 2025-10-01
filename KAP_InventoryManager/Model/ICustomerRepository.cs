using KAP_InventoryManager.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Repositories
{
    internal interface ICustomerRepository
    {
        Task AddAsync(CustomerModel customer);
        Task EditAsync(CustomerModel customer);
        Task<IEnumerable<CustomerModel>> GetAllAsync();
        Task<IEnumerable<string>> SearchCustomerAsync(string searchText);
        Task<IEnumerable<CustomerModel>> SearchCustomerListAsync(string customerId);
        Task<CustomerModel> GetByCustomerIDAsync(string customerID);
        Task<IEnumerable<PaymentModel>> GetCustomerReport(string customerId, DateTime startDate, DateTime endDate, string statusFilter);
        Task<IEnumerable<string>> GetCustomersFromInvoice(DateTime startDate, DateTime endDate);
        Task<string> GetCustomerName(string customerID);
        Task<IEnumerable<string>> GetCustomersWithoutReps(DateTime startDate, DateTime endDate, string statusFilter);
        Task<IEnumerable<PaymentModel>> GetCustomerInvoices(string customerId, DateTime startDate, DateTime endDate, string statusFilter);
        Task<IEnumerable<ReturnModel>> GetCustomerReturns(string customerId, DateTime startDate, DateTime endDate);

    }
}
