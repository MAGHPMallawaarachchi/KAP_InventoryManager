using GalaSoft.MvvmLight;
using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class SalesRepRepository : RepositoryBase, ISalesRepRepository
    {
        public async Task<List<string>> GetAllRepIdsAsync()
        {
            try
            {
                var salesReps = new List<string> { "None" };

                using (var reader = await ExecuteReaderAsync("SELECT RepID FROM SalesRep", CommandType.Text))
                {
                    while (await reader.ReadAsync())
                    {
                        string repId = reader["RepID"].ToString();
                        salesReps.Add(repId);
                    }
                }

                return salesReps;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get sales reps. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<SalesRepModel>> GetAllAsync()
        {
            var reps = new List<SalesRepModel>();
            try
            {
                using (var reader = await ExecuteReaderAsync("SELECT RepID FROM SalesRep ORDER BY RepID DESC LIMIT 20", CommandType.Text))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        reps.Add(new SalesRepModel
                        {
                            Id = counter,
                            RepID = reader["RepID"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get sales reps. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return reps;
        }

        public async Task<IEnumerable<SalesRepModel>> SearchRepsListAsync(string repId)
        {
            var reps = new List<SalesRepModel>();
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_RepId", repId)
                };

                using (var reader = await ExecuteReaderAsync("SearchRepsList", CommandType.StoredProcedure, parameters))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        reps.Add(new SalesRepModel
                        {
                            Id = counter,
                            RepID = reader["RepID"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search sales rep. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return reps;
        }

        public async Task<SalesRepModel> GetByRepIDAsync(string repId)
        {
            SalesRepModel rep = null;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@RepID", repId)
                };

                using (var reader = await ExecuteReaderAsync("SELECT * FROM SalesRep WHERE RepID = @RepID", CommandType.Text, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        rep = new SalesRepModel
                        {
                            RepID = reader["RepID"].ToString(),
                            Name = reader["Name"] is DBNull ? null : reader["Name"].ToString(),
                            Address = reader["Address"] is DBNull ? null : reader["Address"].ToString(),
                            ContactNo = reader["ContactNo"] is DBNull ? null : reader["ContactNo"].ToString(),
                            CommissionPercentage = reader["CommissionPercentage"] is DBNull ? 0 : Convert.ToInt32(reader["CommissionPercentage"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get sales rep. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return rep;
        }

        public async Task<decimal> CalculateCurrentMonthCommissionAsync(string repId)
        {
            decimal currentMonthCommission = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_RepID", repId)
                };

                var result = await ExecuteScalarAsync("SELECT CalculateCurrentMonthCommission(@p_RepID)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    currentMonthCommission = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate current month commission. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return currentMonthCommission;
        }

        public async Task<decimal> CalculateLastMonthCommissionAsync(string repId)
        {
            decimal lastMonthCommission = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_RepID", repId)
                };

                var result = await ExecuteScalarAsync("SELECT CalculateLastMonthCommission(@p_RepID)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    lastMonthCommission = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate last month commission. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lastMonthCommission;
        }

        public async Task<decimal> CalculateTodayCommissionAsync(string repId)
        {
            decimal todayCommission = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_RepID", repId)
                };

                var result = await ExecuteScalarAsync("SELECT CalculateTodayCommission(@p_RepID)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    todayCommission = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate today's commission. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return todayCommission;
        }

        public async Task<decimal> CalculatePercentageChangeAsync(decimal currentMonthCommission, decimal lastMonthCommission)
        {
            decimal percentageChange = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CurrentMonthRevenue", currentMonthCommission),
                    new MySqlParameter("@p_LastMonthRevenue", lastMonthCommission)
                };

                var result = await ExecuteScalarAsync("SELECT CalculatePercentageChange(@p_CurrentMonthRevenue, @p_LastMonthRevenue)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    percentageChange = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate percentage change. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return percentageChange;
        }

        public async Task<IEnumerable<string>> GetCustomersFromInovoiceByRep(string repId, DateTime startDate, DateTime endDate, string statusFilter)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_RepID", repId),
                    new MySqlParameter("@p_StartDate", startDate),
                    new MySqlParameter("@p_EndDate", endDate),
                    new MySqlParameter("statusFilter", statusFilter)
                };

                using (var reader = await ExecuteReaderAsync("GetCustomersFromInovoiceByRep", CommandType.StoredProcedure, parameters))
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

        public async Task<IEnumerable<PaymentModel>> GetRepReport(string customerId, string repId, DateTime startDate, DateTime endDate, string statusFilter)
        {
            var payments = new List<PaymentModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CustomerID", MySqlDbType.VarChar) { Value = customerId },
                    new MySqlParameter("@p_RepID", MySqlDbType.VarChar) { Value = repId },
                    new MySqlParameter("@p_StartDate", MySqlDbType.DateTime) { Value = startDate },
                    new MySqlParameter("@p_EndDate", MySqlDbType.DateTime) { Value = endDate },
                    new MySqlParameter("@statusFilter", MySqlDbType.VarChar) { Value = statusFilter }
                };

                using (var reader = await ExecuteReaderAsync("GetRepReport", CommandType.StoredProcedure, parameters))
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
                            ReceiptNo = reader["ReceiptNo"] is DBNull ? " " : reader["ReceiptNo"].ToString(),
                            PaymentType = reader["PaymentType"] is DBNull ? " " : reader["PaymentType"].ToString(),
                            PaymentDate = reader["PaymentDate"] is DBNull ? default(DateTime) : Convert.ToDateTime(reader["PaymentDate"]),
                            ReturnNo = reader["ReturnNo"] is DBNull ? "" : reader["ReturnNo"].ToString(),
                            ReturnAmount = reader["ReturnAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["ReturnAmount"])
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
    }
}
