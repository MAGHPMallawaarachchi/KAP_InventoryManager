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
    internal class ReturnRepository : RepositoryBase, IReturnRepository
    {
        public void AddReturn(ReturnModel vReturn)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new MySqlCommand("AddReturn", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("@p_ReturnNo", MySqlDbType.VarChar).Value = vReturn.ReturnNo;
                            command.Parameters.Add("@p_InvoiceNo", MySqlDbType.VarChar).Value = vReturn.InvoiceNo;
                            command.Parameters.Add("@p_CustomerID", MySqlDbType.VarChar).Value = vReturn.CustomerID;
                            command.Parameters.Add("@p_RepID", MySqlDbType.VarChar).Value = vReturn.RepID;
                            command.Parameters.Add("@p_Date", MySqlDbType.DateTime).Value = vReturn.Date;
                            command.Parameters.Add("@p_TotalAmount", MySqlDbType.Decimal).Value = vReturn.TotalAmount;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add return. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddReturnItem(ReturnItemModel returnItem, string invoiceNo)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new MySqlCommand("AddReturnItem", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("@p_No", MySqlDbType.Int32).Value = returnItem.No;
                            command.Parameters.Add("@p_ReturnNo", MySqlDbType.VarChar).Value = returnItem.ReturnNo;
                            command.Parameters.Add("@p_InvoiceNo", MySqlDbType.VarChar).Value = invoiceNo;
                            command.Parameters.Add("@p_PartNo", MySqlDbType.VarChar).Value = returnItem.PartNo;
                            command.Parameters.Add("@p_Quantity", MySqlDbType.Int32).Value = returnItem.Quantity;
                            command.Parameters.Add("@p_DamagedQty", MySqlDbType.Int32).Value = returnItem.DamagedQty;
                            command.Parameters.Add("@p_Amount", MySqlDbType.Decimal).Value = returnItem.Amount;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add return items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string GetNextReturnNumber()
        {
            try
            {
                string nextReturnNo = string.Empty;
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT GetNextReturnNo()", connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            nextReturnNo = result.ToString();
                        }
                    }
                }
                return nextReturnNo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get return number. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public async Task<IEnumerable<ReturnModel>> GetAllReturnsAsync()
        {
            try
            {
                List<ReturnModel> returns = new List<ReturnModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT ReturnNo FROM `Return` ORDER BY Date DESC LIMIT 20", connection))
                {

                    await connection.OpenAsync();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            ReturnModel returning = new ReturnModel()
                            {
                                Id = counter,
                                ReturnNo = reader["ReturnNo"].ToString(),
                            };

                            returns.Add(returning);
                        }
                    }
                }
                return returns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get all returns. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<ReturnModel>> SearchReturnListAsync(string returnNo)
        {
            try
            {
                List<ReturnModel> returns = new List<ReturnModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchReturnList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@p_ReturnNo", returnNo);

                    await connection.OpenAsync();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            ReturnModel invoice = new ReturnModel()
                            {
                                Id = counter,
                                ReturnNo = reader["ReturnNo"].ToString(),
                            };

                            returns.Add(invoice);
                        }
                    }
                }
                return returns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search return. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<ReturnModel> GetByReturnNo(string returnNo)
        {
            try
            {
                ReturnModel returnModel = null;

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT * FROM `Return` WHERE ReturnNo = @ReturnNo", connection))
                {
                    command.Parameters.Add("@ReturnNo", MySqlDbType.VarChar).Value = returnNo;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            returnModel = new ReturnModel()
                            {
                                ReturnNo = reader["ReturnNo"].ToString(),
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                CustomerID = reader["CustomerID"] is DBNull ? null : reader["CustomerID"].ToString(),
                                RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString(),
                                DateString = reader["Date"] is DBNull ? null : ((DateTime)reader["Date"]).ToString("dd-MM-yyyy @ HH.mm"),
                                TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                            };
                        }
                    }
                }
                return returnModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<ReturnItemModel>> GetReturnItems(string returnNo)
        {
            try
            {
                List<ReturnItemModel> returnItems = new List<ReturnItemModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("GetReturnItems", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_ReturnNo", returnNo);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ReturnItemModel returnItem = new ReturnItemModel()
                            {
                                No = (int)reader["No"],
                                PartNo = reader["PartNo"].ToString(),
                                Description = reader["Description"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                DamagedQty = (int)reader["DamagedQty"],
                                UnitPrice = (Decimal)reader["UnitPrice"],
                                Discount = (Decimal)reader["Discount"],
                                Amount = (Decimal)reader["Amount"],
                            };

                            returnItems.Add(returnItem);
                        }
                    }
                }
                return returnItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get return items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
