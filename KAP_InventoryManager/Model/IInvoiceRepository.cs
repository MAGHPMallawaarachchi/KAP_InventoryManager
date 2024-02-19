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
        Task<IEnumerable<InvoiceModel>> GetInvoiceByCustomerAsync(string customerId, int pageSize, int page);
    }
}
