using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class InvoiceCustomerRepository : RepositoryBase, IInvoiceCustomerRepository
    {
        public async Task ConfirmPaymentAsync(InvoiceCustomerModel payment)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", payment.CustomerId),
                    new MySqlParameter("@p_InvoiceNo", payment.InvoiceNo),
                    new MySqlParameter("@p_PaymentType", payment.PaymentType),
                    new MySqlParameter("@p_ChequeNo", payment.ChequeNo),
                    new MySqlParameter("@p_Bank", payment.Bank),
                    new MySqlParameter("@p_Date", payment.Date),
                    new MySqlParameter("@p_Comment", payment.Comment),
                    new MySqlParameter("@p_PaymentCount", MySqlDbType.Int32) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("AddPayment", CommandType.StoredProcedure, parameters);

                int paymentCount = Convert.ToInt32(parameters[7].Value);

                if (paymentCount == 0)
                {
                    MessageBox.Show("Payment confirmed successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Payment for this invoice is already confirmed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to confirm the payment. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
