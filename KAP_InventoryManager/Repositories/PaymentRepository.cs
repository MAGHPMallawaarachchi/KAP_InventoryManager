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
    internal class PaymentRepository : RepositoryBase, IPaymentRepository
    {
        public async Task<int> AddPaymentAsync(PaymentModel payment)
        {
            int paymentId = 0;

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", payment.InvoiceNo),
                    new MySqlParameter("@p_CustomerID", payment.CustomerId),
                    new MySqlParameter("@p_PaymentType", payment.PaymentType ?? (object)DBNull.Value),
                    new MySqlParameter("@p_ReceiptNo", payment.ReceiptNo ?? (object)DBNull.Value),
                    new MySqlParameter("@p_ChequeNo", payment.ChequeNo ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Bank", payment.Bank ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Amount", payment.Amount),
                    new MySqlParameter("@p_Date", payment.Date),
                    new MySqlParameter("@p_Comment", payment.Comment ?? (object)DBNull.Value),
                    new MySqlParameter("@p_PaymentID", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
                    new MySqlParameter("@p_RemainingBalance", MySqlDbType.Decimal) { Direction = ParameterDirection.Output },
                    new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("AddPayment", CommandType.StoredProcedure, parameters);

                paymentId = parameters[9].Value != DBNull.Value ? Convert.ToInt32(parameters[9].Value) : 0;
                string errorMessage = parameters[11].Value != DBNull.Value ? parameters[11].Value.ToString() : null;

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, "Payment Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return 0;
                }

                if (paymentId > 0)
                {
                    decimal remainingBalance = parameters[10].Value != DBNull.Value ? Convert.ToDecimal(parameters[10].Value) : 0;
                    string message = remainingBalance == 0
                        ? "Payment added successfully. Invoice is now fully paid!"
                        : $"Payment added successfully. Remaining balance: {remainingBalance:C}";

                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add payment. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return paymentId;
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByInvoiceAsync(string invoiceNo, string customerId)
        {
            var payments = new List<PaymentModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo),
                    new MySqlParameter("@p_CustomerID", customerId)
                };

                using (var reader = await ExecuteReaderAsync("GetPaymentsByInvoice", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        payments.Add(new PaymentModel
                        {
                            PaymentID = reader["PaymentID"] is DBNull ? 0 : Convert.ToInt32(reader["PaymentID"]),
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerId = reader["CustomerID"].ToString(),
                            PaymentType = reader["PaymentType"] is DBNull ? null : reader["PaymentType"].ToString(),
                            ReceiptNo = reader["ReceiptNo"] is DBNull ? null : reader["ReceiptNo"].ToString(),
                            ChequeNo = reader["ChequeNo"] is DBNull ? null : reader["ChequeNo"].ToString(),
                            Bank = reader["Bank"] is DBNull ? null : reader["Bank"].ToString(),
                            Amount = reader["Amount"] is DBNull ? 0 : Convert.ToDecimal(reader["Amount"]),
                            Date = reader["Date"] is DBNull ? DateTime.Now : Convert.ToDateTime(reader["Date"]),
                            Comment = reader["Comment"] is DBNull ? null : reader["Comment"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get payments. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return payments;
        }

        public async Task<PaymentModel> GetPaymentByIdAsync(int paymentId)
        {
            PaymentModel payment = null;

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PaymentID", paymentId)
                };

                using (var reader = await ExecuteReaderAsync("GetPaymentById", CommandType.StoredProcedure, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        payment = new PaymentModel
                        {
                            PaymentID = reader["PaymentID"] is DBNull ? 0 : Convert.ToInt32(reader["PaymentID"]),
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerId = reader["CustomerID"].ToString(),
                            PaymentType = reader["PaymentType"] is DBNull ? null : reader["PaymentType"].ToString(),
                            ReceiptNo = reader["ReceiptNo"] is DBNull ? null : reader["ReceiptNo"].ToString(),
                            ChequeNo = reader["ChequeNo"] is DBNull ? null : reader["ChequeNo"].ToString(),
                            Bank = reader["Bank"] is DBNull ? null : reader["Bank"].ToString(),
                            Amount = reader["Amount"] is DBNull ? 0 : Convert.ToDecimal(reader["Amount"]),
                            Date = reader["Date"] is DBNull ? DateTime.Now : Convert.ToDateTime(reader["Date"]),
                            Comment = reader["Comment"] is DBNull ? null : reader["Comment"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get payment. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return payment;
        }

        public async Task UpdatePaymentAsync(PaymentModel payment)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PaymentID", payment.PaymentID),
                    new MySqlParameter("@p_PaymentType", payment.PaymentType ?? (object)DBNull.Value),
                    new MySqlParameter("@p_ReceiptNo", payment.ReceiptNo ?? (object)DBNull.Value),
                    new MySqlParameter("@p_ChequeNo", payment.ChequeNo ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Bank", payment.Bank ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Amount", payment.Amount),
                    new MySqlParameter("@p_Date", payment.Date),
                    new MySqlParameter("@p_Comment", payment.Comment ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Success", MySqlDbType.Bit) { Direction = ParameterDirection.Output },
                    new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("UpdatePayment", CommandType.StoredProcedure, parameters);

                bool success = parameters[8].Value != DBNull.Value && Convert.ToBoolean(parameters[8].Value);
                string errorMessage = parameters[9].Value != DBNull.Value ? parameters[9].Value.ToString() : null;

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (success)
                {
                    MessageBox.Show("Payment updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update payment. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task DeletePaymentAsync(int paymentId)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PaymentID", paymentId),
                    new MySqlParameter("@p_Success", MySqlDbType.Bit) { Direction = ParameterDirection.Output },
                    new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("DeletePayment", CommandType.StoredProcedure, parameters);

                bool success = parameters[1].Value != DBNull.Value && Convert.ToBoolean(parameters[1].Value);
                string errorMessage = parameters[2].Value != DBNull.Value ? parameters[2].Value.ToString() : null;

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (success)
                {
                    MessageBox.Show("Payment deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete payment. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<PaymentSummaryModel> GetPaymentSummaryAsync(string invoiceNo)
        {
            PaymentSummaryModel summary = null;

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                using (var reader = await ExecuteReaderAsync("GetPaymentSummary", CommandType.StoredProcedure, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        summary = new PaymentSummaryModel
                        {
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            InvoiceTotal = reader["InvoiceTotal"] is DBNull ? 0 : Convert.ToDecimal(reader["InvoiceTotal"]),
                            TotalPaid = reader["TotalPaid"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalPaid"]),
                            RemainingBalance = reader["RemainingBalance"] is DBNull ? 0 : Convert.ToDecimal(reader["RemainingBalance"]),
                            PaymentCount = reader["PaymentCount"] is DBNull ? 0 : Convert.ToInt32(reader["PaymentCount"]),
                            FirstPaymentDate = reader["FirstPaymentDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["FirstPaymentDate"]),
                            LastPaymentDate = reader["LastPaymentDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["LastPaymentDate"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get payment summary. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return summary;
        }

    }
}
