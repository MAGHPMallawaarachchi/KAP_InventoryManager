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
    internal class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public void Add(CustomerModel customer)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new MySqlCommand("AddCustomer", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("@p_CustomerID", MySqlDbType.VarChar).Value = customer.CustomerID;
                            command.Parameters.Add("@p_Name", MySqlDbType.VarChar).Value = customer.Name;
                            command.Parameters.Add("@p_Address", MySqlDbType.VarChar).Value = customer.Address;
                            command.Parameters.Add("@p_Email", MySqlDbType.VarChar).Value = customer.Email;
                            command.Parameters.Add("@p_City", MySqlDbType.VarChar).Value = customer.City;
                            command.Parameters.Add("@p_ContactNo", MySqlDbType.VarChar).Value = customer.ContactNo;
                            command.Parameters.Add("@p_PaymentType", MySqlDbType.VarChar).Value = customer.PaymentType;
                            command.Parameters.Add("@p_DebtLimit", MySqlDbType.Decimal).Value = customer.DebtLimit;
                            command.Parameters.Add("@p_RepID", MySqlDbType.VarChar).Value = customer.RepID;

                            var p_CustomerCount = new MySqlParameter("@p_CustomerCount", MySqlDbType.Int32);
                            p_CustomerCount.Direction = ParameterDirection.Output;
                            command.Parameters.Add(p_CustomerCount);

                            command.ExecuteNonQuery();

                            // Get the value of the output parameter
                            int customerCount = Convert.ToInt32(p_CustomerCount.Value);

                            if (customerCount == 0)
                            {
                                MessageBox.Show("Customer added successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Customer already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<IEnumerable<CustomerModel>> GetAllAsync()
        {
            try
            {
                List<CustomerModel> customers = new List<CustomerModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT CustomerID FROM Customer ORDER BY CustomerID DESC LIMIT 20", connection))
                {
                    await connection.OpenAsync();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            CustomerModel customer = new CustomerModel()
                            {
                                Id = counter,
                                CustomerID = reader["CustomerID"].ToString(),
                            };

                            customers.Add(customer);
                        }
                    }
                }
                return customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get customers. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public List<string> SearchCustomer(string SearchText)
        {
            try
            {
                List<string> customers = new List<string>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchCustomer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchText", SearchText);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string customerId = reader["CustomerID"].ToString();
                            customers.Add(customerId);
                        }
                    }
                }

                return customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<CustomerModel>> SearchCustomerListAsync(string customerId)
        {
            try
            {
                List<CustomerModel> customers = new List<CustomerModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchCustomerList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_CustomerId", customerId);

                    connection.Open();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            CustomerModel customer = new CustomerModel()
                            {
                                Id = counter,
                                CustomerID = reader["CustomerID"].ToString(),
                            };

                            customers.Add(customer);
                        }
                    }
                }

                return customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        public CustomerModel GetByCustomerID(string customerID)
        {
            try
            {
                CustomerModel customer = null;

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT * FROM Customer WHERE CustomerID = @CustomerID", connection))
                {
                    command.Parameters.Add("@CustomerID", MySqlDbType.VarChar).Value = customerID;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new CustomerModel()
                            {
                                CustomerID = reader["CustomerID"].ToString(),
                                Name = reader["Name"] is DBNull ? null : reader["Name"].ToString(),
                                Address = reader["Address"] is DBNull ? null : reader["Address"].ToString(),
                                City = reader["City"] is DBNull ? null : reader["City"].ToString(),
                                ContactNo = reader["ContactNo"] is DBNull ? null : reader["ContactNo"].ToString(),
                                Email = reader["Email"] is DBNull ? null : reader["Email"].ToString(),
                                PaymentType = reader["PaymentType"] is DBNull ? null : reader["PaymentType"].ToString(),
                                DebtLimit = reader["DebtLimit"] is DBNull ? 99 : Convert.ToDecimal(reader["DebtLimit"]),
                                TotalDebt = reader["TotalDebt"] is DBNull ? 1 : Convert.ToDecimal(reader["TotalDebt"]),
                                RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString(),
                            };
                        }
                    }
                }
                return customer;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
