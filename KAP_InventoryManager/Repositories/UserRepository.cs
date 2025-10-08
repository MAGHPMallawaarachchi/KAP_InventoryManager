using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class UserRepository : RepositoryBase, IUserRepository
    {
        public void Add(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser;

            ConnectionSemaphore.Wait();
            try
            {
                using (var connection = GetConnection())
                using (var command = new MySqlCommand("UserLogin", connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@p_EnteredUsername", credential.UserName);
                    command.Parameters.AddWithValue("@p_EnteredPassword", credential.Password);

                    // Add output parameter for login success
                    command.Parameters.Add("@p_LoginSuccess", MySqlDbType.Bit).Direction = ParameterDirection.Output;

                    // Execute the stored procedure
                    command.ExecuteNonQuery();

                    // Retrieve the output parameter value
                    validUser = Convert.ToBoolean(command.Parameters["@p_LoginSuccess"].Value);
                }
            }
            finally
            {
                ConnectionSemaphore.Release();
            }

            return validUser;
        }


        public void Edit(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserModel> GetAll()
        {
            throw new NotImplementedException();
        }

        public UserModel GetByID(int id)
        {
            throw new NotImplementedException();
        }

        public UserModel GetByUsername(string username)
        {
            UserModel user = null;

            ConnectionSemaphore.Wait();
            try
            {
                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT * FROM Admin WHERE Username = @Username", connection))
                {
                    connection.Open();

                    command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = username;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new UserModel()
                            {
                                UserName = reader["Username"].ToString(),
                                Name = reader["Name"].ToString(),
                            };
                        }
                    }
                }
            }
            finally
            {
                ConnectionSemaphore.Release();
            }

            return user;
        }

        public async Task<string> GetPath(string username)
        {
            string invoicePath = "";

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Username", username)
                };

                using (var reader = await ExecuteReaderAsync("SELECT InvoicePath FROM Admin WHERE Username = @Username", CommandType.Text, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        invoicePath = reader["InvoicePath"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the path. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return invoicePath;
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
