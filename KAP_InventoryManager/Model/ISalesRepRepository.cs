using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface ISalesRepRepository
    {
        Task<List<string>> GetAllRepIdsAsync();
        Task<IEnumerable<SalesRepModel>> GetAllAsync();
        Task<IEnumerable<SalesRepModel>> SearchRepsListAsync(string repId);
        Task<SalesRepModel> GetByRepIDAsync(string repId);
        Task<decimal> CalculateCurrentMonthCommissionAsync(string repId);
        Task<decimal> CalculateLastMonthCommissionAsync(string repId);
        Task<decimal> CalculateTodayCommissionAsync(string repId);
        Task<decimal> CalculatePercentageChangeAsync(decimal currentMonthCommission, decimal lastMonthCommission);
        Task<IEnumerable<string>> GetCustomersFromInovoiceByRep(string repId, DateTime startDate, DateTime endDate, string statusFilter);
        Task<IEnumerable<PaymentModel>> GetRepReport(string customerId, string repId, DateTime startDate, DateTime endDate, string statusFilter);

    }
}
