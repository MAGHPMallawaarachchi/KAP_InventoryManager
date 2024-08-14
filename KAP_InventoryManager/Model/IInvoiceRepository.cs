using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IInvoiceRepository
    {
        Task AddInvoiceAsync(InvoiceModel invoice);
        Task AddInvoiceItemAsync(InvoiceItemModel invoiceItem);
        Task<string> GetNextInvoiceNumberAsync();
        Task<IEnumerable<InvoiceModel>> GetInvoiceByCustomerAsync(string customerId, int pageSize, int page);
        Task<IEnumerable<InvoiceModel>> GetInvoiceByRepAsync(string repId, int pageSize, int page);
        Task<IEnumerable<InvoiceModel>> SearchCustomerInvoiceListAsync(string invoiceNo, string customerId, int pageSize, int page);
        Task<IEnumerable<InvoiceModel>> SearchRepInvoiceListAsync(string invoiceNo, string repId, int pageSize, int page);
        Task<IEnumerable<InvoiceModel>> GetAllInvoicesAsync();
        Task<IEnumerable<InvoiceModel>> SearchInvoiceListAsync(string invoiceNo);
        Task<InvoiceModel> GetByInvoiceNoAsync(string invoiceNo);
        Task<IEnumerable<InvoiceItemModel>> GetInvoiceItemsAsync(string invoiceNo);
        Task<IEnumerable<InvoiceItemModel>> GetInvoicesByPartNoAsync(string partNo, int pageSize, int page);
        Task CancelInvoiceAsync(string invoiceNo);
        Task UpdateOverdueInvoices();
        Task UpdatePaidInvoice(string invoiceNo);
        Task<List<string>> SearchInvoiceNumberAsync(string searchText);
        Task<List<string>> GetPartNumbersByInvoiceAsync(string invoiceNo);
        Task<InvoiceItemModel> GetInvoiceItemAsync(string invoiceNo, string partNo);
        Task<IEnumerable<InvoiceItemModel>> GetInvoiceItemsAsync(string invoiceNo, int pageSize, int page);
        Task<IEnumerable<InvoiceModel>> GetPastTwoDaysInvoicesAsync();
        Task<IEnumerable<SalesReportModel>> GetSalesReportAsync(DateTime startDate, DateTime endDate);
    }
}
