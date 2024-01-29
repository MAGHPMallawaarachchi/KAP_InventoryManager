using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Repositories
{
    internal class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public void Add(CustomerModel customer)
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
                        command.Parameters.Add("@p_City", MySqlDbType.VarChar).Value = customer.City;
                        command.Parameters.Add("@p_ContactNo", MySqlDbType.VarChar).Value = customer.ContactNo;
                        command.Parameters.Add("@p_PaymentType", MySqlDbType.VarChar).Value = customer.PaymentType;
                        command.Parameters.Add("@p_DebtLimit", MySqlDbType.Decimal).Value = customer.DebtLimit;
                        command.Parameters.Add("@p_RepID", MySqlDbType.VarChar).Value = customer.RepID;

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        IEnumerable<CustomerModel> ICustomerRepository.GetAll()
        {
            List<CustomerModel> customers = new List<CustomerModel>();

            using (var connection = GetConnection())
            using (var command = new MySqlCommand("SELECT CustomerID FROM Customer", connection))
            {
                connection.Open();
                int counter = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
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
    }
}
