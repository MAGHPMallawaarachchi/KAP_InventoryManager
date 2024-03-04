using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IInvoiceRepository
    {
        void AddInvoice(InvoiceModel invoice);
        void AddInvoiceItem(InvoiceItemModel invoiceItem);
        string GetNextInvoiceNumber();
        Task<IEnumerable<InvoiceModel>> GetAllInvoicesAsync();
        Task<IEnumerable<InvoiceModel>> SearchInvoiceListAsync(string invoiceNo);
        Task<IEnumerable<InvoiceModel>> GetInvoiceByCustomerAsync(string customerId, int pageSize, int page);
        Task<IEnumerable<InvoiceModel>> SearchCustomerInvoiceListAsync(string invoiceNo, string customerId, int pageSize, int page);
        Task<InvoiceModel> GetByInvoiceNo(string invoiceNo);
        Task<IEnumerable<InvoiceItemModel>> GetInvoiceItems(string invoiceNo);
        Task<IEnumerable<InvoiceItemModel>> GetInvoicesByPartNo(string partNo, int pageSize, int page);
        Task CancelInvoice(string invoiceNo);
        List<string> SearchInvoiceNumber(string SearchText);
        Task<List<string>> GetPartNumbersByInvoice(string invoiceNo);
        Task<InvoiceItemModel> GetInvoiceItem(string invoiceNo, string partNo);
    }
}
