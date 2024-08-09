using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class InvoiceRepository : RepositoryBase, IInvoiceRepository
    {
        public async Task AddInvoiceAsync(InvoiceModel invoice)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoice.InvoiceNo),
                    new MySqlParameter("@p_Terms", invoice.Terms),
                    new MySqlParameter("@p_Date", invoice.Date),
                    new MySqlParameter("@p_DueDate", invoice.DueDate),
                    new MySqlParameter("@p_TotalAmount", invoice.TotalAmount),
                    new MySqlParameter("@p_CustomerID", invoice.CustomerID),
                    new MySqlParameter("@p_RepID", invoice.RepID)
                };

                await ExecuteNonQueryAsync("AddInvoice", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task AddInvoiceItemAsync(InvoiceItemModel invoiceItem)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceItem.InvoiceNo),
                    new MySqlParameter("@p_PartNo", invoiceItem.PartNo),
                    new MySqlParameter("@p_No", invoiceItem.No),
                    new MySqlParameter("@p_Quantity", invoiceItem.Quantity),
                    new MySqlParameter("@p_Amount", invoiceItem.Amount),
                    new MySqlParameter("@p_Discount", invoiceItem.Discount)
                };

                await ExecuteNonQueryAsync("AddInvoiceItem", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add invoice items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<string> GetNextInvoiceNumberAsync()
        {
            try
            {
                var result = await ExecuteScalarAsync("SELECT GetNextInvoiceNo()", CommandType.Text);
                return result?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice number. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> GetInvoiceByCustomerAsync(string customerId, int pageSize, int page)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PageSize", pageSize),
                    new MySqlParameter("@p_Offset", (page - 1) * pageSize),
                    new MySqlParameter("@p_CustomerID", customerId)
                };

                using (var reader = await ExecuteReaderAsync("GetInvoiceByCustomer", CommandType.StoredProcedure, parameters))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = pageSize * (page - 1);
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            DateString = ((DateTime)reader["Date"]).ToString("dd-MM-yyyy"),
                            DueDateString = ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                            TotalAmount = (Decimal)reader["TotalAmount"],
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> GetInvoiceByRepAsync(string repId, int pageSize, int page)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PageSize", pageSize),
                    new MySqlParameter("@p_Offset", (page - 1) * pageSize),
                    new MySqlParameter("@p_RepID", repId)
                };

                using (var reader = await ExecuteReaderAsync("GetInvoiceByRep", CommandType.StoredProcedure, parameters))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = pageSize * (page - 1);
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerID = reader["CustomerID"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            DateString = ((DateTime)reader["Date"]).ToString("dd-MM-yyyy"),
                            DueDateString = ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                            TotalAmount = (Decimal)reader["TotalAmount"],
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> SearchCustomerInvoiceListAsync(string invoiceNo, string customerId, int pageSize, int page)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo),
                    new MySqlParameter("@p_PageSize", pageSize),
                    new MySqlParameter("@p_Offset", (page - 1) * pageSize),
                    new MySqlParameter("@p_CustomerID", customerId)
                };

                using (var reader = await ExecuteReaderAsync("SearchCustomerInvoiceList", CommandType.StoredProcedure, parameters))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = pageSize * (page - 1);
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            DateString = ((DateTime)reader["Date"]).ToString("dd-MM-yyyy"),
                            DueDateString = ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                            TotalAmount = (Decimal)reader["TotalAmount"],
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> SearchRepInvoiceListAsync(string invoiceNo, string repId, int pageSize, int page)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo),
                    new MySqlParameter("@p_PageSize", pageSize),
                    new MySqlParameter("@p_Offset", (page - 1) * pageSize),
                    new MySqlParameter("@p_RepID", repId)
                };

                using (var reader = await ExecuteReaderAsync("SearchRepInvoiceList", CommandType.StoredProcedure, parameters))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = pageSize * (page - 1);
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerID = reader["CustomerID"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            DateString = ((DateTime)reader["Date"]).ToString("dd-MM-yyyy"),
                            DueDateString = ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                            TotalAmount = (Decimal)reader["TotalAmount"],
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> GetAllInvoicesAsync()
        {
            try
            {
                using (var reader = await ExecuteReaderAsync("SELECT InvoiceNo, CustomerID, Status FROM Invoice ORDER BY Date DESC LIMIT 20", CommandType.Text))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerID = reader["CustomerID"].ToString(),
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get all invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> SearchInvoiceListAsync(string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                using (var reader = await ExecuteReaderAsync("SearchInvoiceList", CommandType.StoredProcedure, parameters))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerID = reader["CustomerID"].ToString(),
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<InvoiceModel> GetByInvoiceNoAsync(string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@InvoiceNo", invoiceNo)
                };

                using (var reader = await ExecuteReaderAsync("SELECT * FROM Invoice WHERE InvoiceNo = @InvoiceNo", CommandType.Text, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        return new InvoiceModel
                        {
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            Terms = reader["Terms"] is DBNull ? null : reader["Terms"].ToString(),
                            DateString = reader["Date"] is DBNull ? null : ((DateTime)reader["Date"]).ToString("dd-MM-yyyy hh.mmtt"),
                            DueDateString = reader["DueDate"] is DBNull ? null : ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                            TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                            Status = reader["Status"] is DBNull ? null : reader["Status"].ToString(),
                            CustomerID = reader["CustomerID"] is DBNull ? null : reader["CustomerID"].ToString(),
                            RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString()
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

        public async Task<IEnumerable<InvoiceItemModel>> GetInvoiceItemsAsync(string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                using (var reader = await ExecuteReaderAsync("GetInvoiceItems", CommandType.StoredProcedure, parameters))
                {
                    var invoiceItems = new List<InvoiceItemModel>();
                    while (await reader.ReadAsync())
                    {
                        invoiceItems.Add(new InvoiceItemModel
                        {
                            No = (int)reader["No"],
                            PartNo = reader["PartNo"].ToString(),
                            BrandID = reader["BrandID"].ToString(),
                            Description = reader["Description"].ToString(),
                            Quantity = (int)reader["Quantity"],
                            UnitPrice = (Decimal)reader["UnitPrice"],
                            Discount = (Decimal)reader["Discount"],
                            Amount = (Decimal)reader["Amount"]
                        });
                    }
                    return invoiceItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceItemModel>> GetInvoicesByPartNoAsync(string partNo, int pageSize, int page)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", partNo),
                    new MySqlParameter("@p_PageSize", pageSize),
                    new MySqlParameter("@p_Offset", (page - 1) * pageSize)
                };

                using (var reader = await ExecuteReaderAsync("GetInvoicesByPartNo", CommandType.StoredProcedure, parameters))
                {
                    var invoiceItems = new List<InvoiceItemModel>();
                    int counter = pageSize * (page - 1);
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoiceItems.Add(new InvoiceItemModel
                        {
                            No = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            Quantity = (int)reader["Quantity"],
                            UnitPrice = (Decimal)reader["UnitPrice"],
                            Discount = (Decimal)reader["Discount"],
                            Amount = (Decimal)reader["Amount"],
                            CustomerID = reader["Name"].ToString(),
                        });
                    }
                    return invoiceItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task CancelInvoiceAsync(string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                await ExecuteNonQueryAsync("CancelInvoice", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to cancel the invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateOverdueInvoices()
        {
            try
            {
                await ExecuteNonQueryAsync("UpdateOverdueInvoices", CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update the overdue invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdatePaidInvoice(string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                await ExecuteNonQueryAsync("UpdatePaidInvoice", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update the status of the invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<List<string>> SearchInvoiceNumberAsync(string searchText)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@SearchText", searchText)
                };

                using (var reader = await ExecuteReaderAsync("SearchInvoiceNumber", CommandType.StoredProcedure, parameters))
                {
                    var invoiceNumbers = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        invoiceNumbers.Add(reader["InvoiceNo"].ToString());
                    }
                    return invoiceNumbers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<List<string>> GetPartNumbersByInvoiceAsync(string invoiceNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                using (var reader = await ExecuteReaderAsync("GetPartNumbersByInvoice", CommandType.StoredProcedure, parameters))
                {
                    var partNumbers = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        partNumbers.Add(reader["PartNo"].ToString());
                    }
                    return partNumbers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get part numbers. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<InvoiceItemModel> GetInvoiceItemAsync(string invoiceNo, string partNo)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_InvoiceNo", invoiceNo),
                    new MySqlParameter("@p_PartNo", partNo)
                };

                using (var reader = await ExecuteReaderAsync("GetInvoiceItem", CommandType.StoredProcedure, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        return new InvoiceItemModel
                        {
                            PartNo = reader["PartNo"].ToString(),
                            BrandID = reader["BrandID"] is DBNull ? null : reader["BrandID"].ToString(),
                            BuyingPrice = reader["BuyingPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["BuyingPrice"]),
                            UnitPrice = reader["UnitPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["UnitPrice"]),
                            Amount = reader["Amount"] is DBNull ? 0 : Convert.ToDecimal(reader["Amount"]),
                            Discount = reader["Discount"] is DBNull ? 0 : Convert.ToDecimal(reader["Discount"]),
                            Quantity = reader["Quantity"] is DBNull ? 0 : Convert.ToInt32(reader["Quantity"])
                        };
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceItemModel>> GetInvoiceItemsAsync(string invoiceNo, int pageSize, int page)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PageSize", pageSize),
                    new MySqlParameter("@p_Offset", (page - 1) * pageSize),
                    new MySqlParameter("@p_InvoiceNo", invoiceNo)
                };

                using (var reader = await ExecuteReaderAsync("GetInvoiceItemsPaginated", CommandType.StoredProcedure, parameters))
                {
                    var invoiceItems = new List<InvoiceItemModel>();
                    while (await reader.ReadAsync())
                    {
                        invoiceItems.Add(new InvoiceItemModel
                        {
                            No = (int)reader["No"],
                            PartNo = reader["PartNo"].ToString(),
                            BrandID = reader["BrandID"].ToString(),
                            Description = reader["Description"].ToString(),
                            Quantity = (int)reader["Quantity"],
                            UnitPrice = (Decimal)reader["UnitPrice"],
                            Discount = (Decimal)reader["Discount"],
                            Amount = (Decimal)reader["Amount"]
                        });
                    }
                    return invoiceItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> GetPastTwoDaysInvoicesAsync()
        {
            try
            {
                using (var reader = await ExecuteReaderAsync("GetPastTwoDaysInvoices", CommandType.StoredProcedure))
                {
                    var invoices = new List<InvoiceModel>();
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        invoices.Add(new InvoiceModel
                        {
                            Id = counter,
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            CustomerID = reader["CustomerID"].ToString(),
                            Status = reader["Status"].ToString()
                        });
                    }
                    return invoices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
