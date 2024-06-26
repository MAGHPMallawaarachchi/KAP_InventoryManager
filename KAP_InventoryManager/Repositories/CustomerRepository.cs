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
                    new MySqlParameter("@p_RepID", customer.RepID),
                    new MySqlParameter("@p_CustomerCount", MySqlDbType.Int32) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("AddCustomer", CommandType.StoredProcedure, parameters);

                int customerCount = Convert.ToInt32(parameters[9].Value);

                MessageBox.Show(customerCount == 0
                    ? "Customer added successfully."
                    : "Customer already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<IEnumerable<CustomerModel>> GetAllAsync()
        {
            var customers = new List<CustomerModel>();
            try
            {
                using (var reader = await ExecuteReaderAsync("SELECT CustomerID FROM Customer ORDER BY CustomerID DESC LIMIT 20", CommandType.Text))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        customers.Add(new CustomerModel
                        {
                            Id = counter,
                            CustomerID = reader["CustomerID"].ToString()
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
                            CustomerID = reader["CustomerID"].ToString()
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
    }
}
