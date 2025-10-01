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
        public async Task<bool> AddReturnAsync(ReturnModel vReturn)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_ReturnNo", vReturn.ReturnNo),
                    new MySqlParameter("@p_InvoiceNo", vReturn.InvoiceNo),
                    new MySqlParameter("@p_CustomerID", vReturn.CustomerID),
                    new MySqlParameter("@p_RepID", vReturn.RepID),
                    new MySqlParameter("@p_Date", vReturn.Date),
                    new MySqlParameter("@p_TotalAmount", vReturn.TotalAmount)
                };

                await ExecuteNonQueryAsync("AddReturn", CommandType.StoredProcedure, parameters);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add return. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task AddReturnItemAsync(ReturnItemModel returnItem, string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_No", returnItem.No),
                    new MySqlParameter("@p_ReturnNo", returnItem.ReturnNo),
                    new MySqlParameter("@p_InvoiceNo", invoiceNo),
                    new MySqlParameter("@p_PartNo", returnItem.PartNo),
                    new MySqlParameter("@p_Quantity", returnItem.Quantity),
                    new MySqlParameter("@p_DamagedQty", returnItem.DamagedQty),
                    new MySqlParameter("@p_Amount", returnItem.Amount)
                };

                await ExecuteNonQueryAsync("AddReturnItem", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add return items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<string> GetNextReturnNumberAsync()
        {
            try
            {
                var result = await ExecuteScalarAsync("SELECT GetNextReturnNo()", CommandType.Text);
                return result?.ToString() ?? string.Empty;
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
                var returns = new List<ReturnModel>();
                var query = "SELECT ReturnNo FROM `Return` ORDER BY Date DESC LIMIT 20";

                using (var reader = await ExecuteReaderAsync(query, CommandType.Text))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        returns.Add(new ReturnModel
                        {
                            Id = counter,
                            ReturnNo = reader["ReturnNo"].ToString()
                        });
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
                var returns = new List<ReturnModel>();
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_ReturnNo", returnNo)
                };

                using (var reader = await ExecuteReaderAsync("SearchReturnList", CommandType.StoredProcedure, parameters))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        returns.Add(new ReturnModel
                        {
                            Id = counter,
                            ReturnNo = reader["ReturnNo"].ToString()
                        });
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

        public async Task<ReturnModel> GetByReturnNoAsync(string returnNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@ReturnNo", returnNo)
                };

                using (var reader = await ExecuteReaderAsync("SELECT * FROM `Return` WHERE ReturnNo = @ReturnNo", CommandType.Text, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        return new ReturnModel()
                        {
                            ReturnNo = reader["ReturnNo"].ToString(),
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerID = reader["CustomerID"] is DBNull ? null : reader["CustomerID"].ToString(),
                            RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString(),
                            DateString = reader["Date"] is DBNull ? null : ((DateTime)reader["Date"]).ToString("dd-MM-yyyy @ HH.mm"),
                            TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                        };
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<ReturnItemModel>> GetReturnItemsAsync(string returnNo)
        {
            try
            {
                var returnItems = new List<ReturnItemModel>();
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_ReturnNo", returnNo)
                };

                using (var reader = await ExecuteReaderAsync("GetReturnItems", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        returnItems.Add(new ReturnItemModel
                        {
                            No = (int)reader["No"],
                            PartNo = reader["PartNo"].ToString(),
                            Description = reader["Description"].ToString(),
                            Quantity = (int)reader["Quantity"],
                            DamagedQty = (int)reader["DamagedQty"],
                            UnitPrice = (decimal)reader["UnitPrice"],
                            Discount = (decimal)reader["Discount"],
                            Amount = (decimal)reader["Amount"]
                        });
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

        public async Task<IEnumerable<ReturnModel>> GetReturns(DateTime startDate, DateTime endDate)
        {
            var returns = new List<ReturnModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_StartDate", MySqlDbType.DateTime) { Value = startDate },
                    new MySqlParameter("@p_EndDate", MySqlDbType.DateTime) { Value = endDate }
                };

                using (var reader = await ExecuteReaderAsync("GetReturns", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        returns.Add(new ReturnModel
                        {
                            Date = reader["Date"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["Date"]),
                            ReturnNo = reader["ReturnNo"] is DBNull ? "" : reader["ReturnNo"].ToString(),
                            TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                            InvoiceNo = reader["InvoiceNo"] is DBNull ? "" : reader["InvoiceNo"].ToString(),
                            CustomerName = reader["CustomerName"] is DBNull ? "" : reader["CustomerName"].ToString(),
                            CustomerCity = reader["CustomerCity"] is DBNull ? "" : reader["CustomerCity"].ToString()
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
