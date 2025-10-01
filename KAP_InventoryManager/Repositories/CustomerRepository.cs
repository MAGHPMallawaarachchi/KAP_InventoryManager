using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public async Task AddAsync(CustomerModel customer)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", customer.CustomerID),
                    new MySqlParameter("@p_Name", customer.Name),
                    new MySqlParameter("@p_Address", customer.Address),
                    new MySqlParameter("@p_Email", customer.Email),
                    new MySqlParameter("@p_City", customer.City),
                    new MySqlParameter("@p_ContactNo", customer.ContactNo),
                    new MySqlParameter("@p_PaymentType", customer.PaymentType),
                    new MySqlParameter("@p_RepID", string.IsNullOrEmpty(customer.RepID) ? (object)DBNull.Value : customer.RepID),
                    new MySqlParameter("@p_CustomerCount", MySqlDbType.Int32) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("AddCustomer", CommandType.StoredProcedure, parameters);

                int customerCount = Convert.ToInt32(parameters[8].Value);

                MessageBox.Show(customerCount == 0
                    ? "Customer added successfully."
                    : "Customer already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task EditAsync(CustomerModel customer)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", customer.CustomerID),
                    new MySqlParameter("@p_Name", customer.Name),
                    new MySqlParameter("@p_Address", customer.Address),
                    new MySqlParameter("@p_Email", customer.Email),
                    new MySqlParameter("@p_City", customer.City),
                    new MySqlParameter("@p_ContactNo", customer.ContactNo),
                    new MySqlParameter("@p_PaymentType", customer.PaymentType),
                    new MySqlParameter("@p_RepID", string.IsNullOrEmpty(customer.RepID) ? (object)DBNull.Value : customer.RepID),
                    new MySqlParameter("@p_AffectedRows", MySqlDbType.Int32) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("EditCustomer", CommandType.StoredProcedure, parameters);

                int affectedRows = Convert.ToInt32(parameters[8].Value);

                if (affectedRows > 0)
                {
                    MessageBox.Show("Customer updated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Customer does not exist or no changes were made.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update the Customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<IEnumerable<CustomerModel>> GetAllAsync()
        {
            var customers = new List<CustomerModel>();
            try
            {
                using (var reader = await ExecuteReaderAsync("SELECT CustomerID, Name, City FROM Customer ORDER BY CustomerID DESC", CommandType.Text))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        customers.Add(new CustomerModel
                        {
                            Id = counter,
                            CustomerID = reader["CustomerID"].ToString(),
                            Name = reader["Name"].ToString(),
                            City = reader["City"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get customers. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return customers;
        }

        public async Task<IEnumerable<string>> SearchCustomerAsync(string searchText)
        {
            var customers = new List<string>();
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@SearchText", searchText)
                };

                using (var reader = await ExecuteReaderAsync("SearchCustomer", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        customers.Add(reader["CustomerID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return customers;
        }

        public async Task<IEnumerable<CustomerModel>> SearchCustomerListAsync(string customerId)
        {
            var customers = new List<CustomerModel>();
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerId", customerId)
                };

                using (var reader = await ExecuteReaderAsync("SearchCustomerList", CommandType.StoredProcedure, parameters))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        customers.Add(new CustomerModel
                        {
                            Id = counter,
                            CustomerID = reader["CustomerID"].ToString(),
                            Name = reader["Name"].ToString(),
                            City = reader["City"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return customers;
        }

        public async Task<CustomerModel> GetByCustomerIDAsync(string customerID)
        {
            CustomerModel customer = null;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@CustomerID", customerID)
                };

                using (var reader = await ExecuteReaderAsync("SELECT * FROM Customer WHERE CustomerID = @CustomerID", CommandType.Text, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        customer = new CustomerModel
                        {
                            CustomerID = reader["CustomerID"].ToString(),
                            Name = reader["Name"] is DBNull ? null : reader["Name"].ToString(),
                            Address = reader["Address"] is DBNull ? null : reader["Address"].ToString(),
                            City = reader["City"] is DBNull ? null : reader["City"].ToString(),
                            ContactNo = reader["ContactNo"] is DBNull ? null : reader["ContactNo"].ToString(),
                            Email = reader["Email"] is DBNull ? null : reader["Email"].ToString(),
                            PaymentType = reader["PaymentType"] is DBNull ? null : reader["PaymentType"].ToString(),
                            TotalDebt = reader["TotalDebt"] is DBNull ? 1 : Convert.ToDecimal(reader["TotalDebt"]),
                            RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return customer;
        }

        public async Task<IEnumerable<PaymentModel>> GetCustomerReport(string customerId, DateTime startDate, DateTime endDate, string statusFilter)
        {
            var payments = new List<PaymentModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", MySqlDbType.VarChar) { Value = customerId },
                    new MySqlParameter("@p_StartDate", MySqlDbType.DateTime) { Value = startDate },
                    new MySqlParameter("@p_EndDate", MySqlDbType.DateTime) { Value = endDate },
                    new MySqlParameter("@statusFilter", MySqlDbType.VarChar) { Value = statusFilter }
                };

                using (var reader = await ExecuteReaderAsync("GetCustomerReport", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        payments.Add(new PaymentModel
                        {
                            Date = reader["Date"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["Date"]),
                            InvoiceNo = reader["InvoiceNo"] is DBNull ? " " : reader["InvoiceNo"].ToString(),
                            PaymentTerm = reader["PaymentTerm"] is DBNull ? " " : reader["PaymentTerm"].ToString(),
                            Status = reader["Status"] is DBNull ? " " : reader["Status"].ToString(),
                            DueDate = reader["DueDate"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["DueDate"]),
                            TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                            PaymentType = reader["PaymentType"] is DBNull ? " " : reader["PaymentType"].ToString(),
                            ReceiptNo = reader["ReceiptNo"] is DBNull ? " " : reader["ReceiptNo"].ToString(),
                            ChequeNo = reader["ChequeNo"] is DBNull ? " " : reader["ChequeNo"].ToString(),
                            Bank = reader["Bank"] is DBNull ? " " : reader["Bank"].ToString(),
                            PaymentDate = reader["PaymentDate"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["PaymentDate"]),
                            PaymentAmount = reader["PaymentAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["PaymentAmount"]),
                            Comment = reader["Comment"] is DBNull ? "" : reader["Comment"].ToString(),
                            ReturnNo = reader["ReturnNo"] is DBNull ? "" : reader["ReturnNo"].ToString(),
                            ReturnAmount = reader["ReturnAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["ReturnAmount"])
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to get payments. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return payments;
        }

        public async Task<IEnumerable<string>> GetCustomersFromInvoice(DateTime startDate, DateTime endDate)
        {
            var customers = new List<string>();
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_StartDate", MySqlDbType.DateTime) { Value = startDate },
                    new MySqlParameter("@p_EndDate", MySqlDbType.DateTime) { Value = endDate },
                };

                using (var reader = await ExecuteReaderAsync("GetCustomersFromInvoice", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        customers.Add(reader["CustomerID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return customers;
        }

        public async Task<string> GetCustomerName(string customerID)
        {
            string name = null;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@CustomerID", customerID)
                };

                using (var reader = await ExecuteReaderAsync("SELECT Name FROM Customer WHERE CustomerID = @CustomerID", CommandType.Text, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        name = reader["Name"] is DBNull ? null : reader["Name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the name. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return name;
        }

        public async Task<IEnumerable<string>> GetCustomersWithoutReps(DateTime startDate, DateTime endDate, string statusFilter)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_StartDate", startDate),
                    new MySqlParameter("@p_EndDate", endDate),
                    new MySqlParameter("statusFilter", statusFilter)
                };

                using (var reader = await ExecuteReaderAsync("GetCustomersWithoutReps", CommandType.StoredProcedure, parameters))
                {
                    var customers = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        customers.Add(reader["CustomerID"].ToString());
                    }

                    return customers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get customer ids. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetCustomerInvoices(string customerId, DateTime startDate, DateTime endDate, string statusFilter)
        {
            var invoices = new List<PaymentModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", MySqlDbType.VarChar) { Value = customerId },
                    new MySqlParameter("@p_StartDate", MySqlDbType.DateTime) { Value = startDate },
                    new MySqlParameter("@p_EndDate", MySqlDbType.DateTime) { Value = endDate },
                    new MySqlParameter("@statusFilter", MySqlDbType.VarChar) { Value = statusFilter }
                };

                using (var reader = await ExecuteReaderAsync("GetCustomerInvoices", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        invoices.Add(new PaymentModel
                        {
                            Date = reader["Date"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["Date"]),
                            InvoiceNo = reader["InvoiceNo"] is DBNull ? " " : reader["InvoiceNo"].ToString(),
                            PaymentTerm = reader["PaymentTerm"] is DBNull ? " " : reader["PaymentTerm"].ToString(),
                            Status = reader["Status"] is DBNull ? " " : reader["Status"].ToString(),
                            DueDate = reader["DueDate"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["DueDate"]),
                            TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                            PaymentType = reader["PaymentType"] is DBNull ? " " : reader["PaymentType"].ToString(),
                            ReceiptNo = reader["ReceiptNo"] is DBNull ? " " : reader["ReceiptNo"].ToString(),
                            ChequeNo = reader["ChequeNo"] is DBNull ? " " : reader["ChequeNo"].ToString(),
                            Bank = reader["Bank"] is DBNull ? " " : reader["Bank"].ToString(),
                            PaymentDate = reader["PaymentDate"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["PaymentDate"]),
                            PaymentAmount = reader["PaymentAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["PaymentAmount"]),
                            Comment = reader["Comment"] is DBNull ? "" : reader["Comment"].ToString(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get payments. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return invoices;
        }

        public async Task<IEnumerable<ReturnModel>> GetCustomerReturns(string customerId, DateTime startDate, DateTime endDate)
        {
            var returns = new List<ReturnModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", MySqlDbType.VarChar) { Value = customerId },
                    new MySqlParameter("@p_StartDate", MySqlDbType.DateTime) { Value = startDate },
                    new MySqlParameter("@p_EndDate", MySqlDbType.DateTime) { Value = endDate }
                };

                using (var reader = await ExecuteReaderAsync("GetCustomerReturns", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        returns.Add(new ReturnModel
                        {
                            Date = reader["Date"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["Date"]),
                            ReturnNo = reader["ReturnNo"] is DBNull ? "" : reader["ReturnNo"].ToString(),
                            TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                            InvoiceNo = reader["InvoiceNo"] is DBNull ? "" : reader["InvoiceNo"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get returns. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return returns;
        }
    }
}
