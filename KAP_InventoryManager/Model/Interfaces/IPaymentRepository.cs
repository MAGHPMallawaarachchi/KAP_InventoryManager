using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Model
{
    internal interface IPaymentRepository
    {
        Task<int> AddPaymentAsync(PaymentModel payment);
        Task<IEnumerable<PaymentModel>> GetPaymentsByInvoiceAsync(string invoiceNo, string customerId);
        Task<PaymentModel> GetPaymentByIdAsync(int paymentId);
        Task UpdatePaymentAsync(PaymentModel payment);
        Task DeletePaymentAsync(int paymentId);
        Task<PaymentSummaryModel> GetPaymentSummaryAsync(string invoiceNo);
    }
}
