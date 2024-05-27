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
    internal class InvoiceRepository : RepositoryBase, IInvoiceRepository
    {
        public void AddInvoice(InvoiceModel invoice)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new MySqlCommand("AddInvoice", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("@p_InvoiceNo", MySqlDbType.VarChar).Value = invoice.InvoiceNo;
                            command.Parameters.Add("@p_Terms", MySqlDbType.VarChar).Value = invoice.Terms;
                            command.Parameters.Add("@p_Date", MySqlDbType.DateTime).Value = invoice.Date;
                            command.Parameters.Add("@p_DueDate", MySqlDbType.DateTime).Value = invoice.DueDate;
                            command.Parameters.Add("@p_TotalAmount", MySqlDbType.Decimal).Value = invoice.TotalAmount;
                            command.Parameters.Add("@p_CustomerID", MySqlDbType.VarChar).Value = invoice.CustomerID;
                            command.Parameters.Add("@p_RepID", MySqlDbType.VarChar).Value = invoice.RepID;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddInvoiceItem(InvoiceItemModel invoiceItem)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new MySqlCommand("AddInvoiceItem", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("@p_InvoiceNo", MySqlDbType.VarChar).Value = invoiceItem.InvoiceNo;
                            command.Parameters.Add("@p_PartNo", MySqlDbType.VarChar).Value = invoiceItem.PartNo;
                            command.Parameters.Add("@p_No", MySqlDbType.Int32).Value = invoiceItem.No;
                            command.Parameters.Add("@p_Quantity", MySqlDbType.Int32).Value = invoiceItem.Quantity;
                            command.Parameters.Add("@p_Amount", MySqlDbType.Decimal).Value = invoiceItem.Amount;
                            command.Parameters.Add("@p_Discount", MySqlDbType.Decimal).Value = invoiceItem.Discount;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add invoice items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string GetNextInvoiceNumber()
        {
            try
            {
                string nextInvoiceNo = string.Empty;
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT GetNextInvoiceNo()", connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            nextInvoiceNo = result.ToString();
                        }
                    }
                }
                return nextInvoiceNo;
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
                List<InvoiceModel> invoices = new List<InvoiceModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("GetInvoiceByCustomer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_PageSize", pageSize);
                    command.Parameters.AddWithValue("@p_Offset", (page - 1) * pageSize);
                    command.Parameters.AddWithValue("@p_CustomerID", customerId);

                    await connection.OpenAsync();
                    int counter = pageSize*(page - 1);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            InvoiceModel invoice = new InvoiceModel()
                            {
                                Id = counter,
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Terms = reader["Terms"].ToString(),
                                DateString = ((DateTime)reader["Date"]).ToString("dd-MM-yyyy"),
                                DueDateString = ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                                TotalAmount = (Decimal)reader["TotalAmount"],
                                Status = reader["Status"].ToString()
                            };

                            invoices.Add(invoice);
                        }
                    }
                }
                return invoices;
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
                List<InvoiceModel> invoices = new List<InvoiceModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchCustomerInvoiceList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@p_InvoiceNo", invoiceNo);
                    command.Parameters.AddWithValue("@p_PageSize", pageSize);
                    command.Parameters.AddWithValue("@p_Offset", (page - 1) * pageSize);
                    command.Parameters.AddWithValue("@p_CustomerID", customerId);

                    await connection.OpenAsync();
                    int counter = pageSize * (page - 1);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            InvoiceModel invoice = new InvoiceModel()
                            {
                                Id = counter,
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Terms = reader["Terms"].ToString(),
                                DateString = ((DateTime)reader["Date"]).ToString("dd-MM-yyyy"),
                                DueDateString = ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                                TotalAmount = (Decimal)reader["TotalAmount"],
                                Status = reader["Status"].ToString()
                            };

                            invoices.Add(invoice);
                        }
                    }
                }
                return invoices;
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
                List<InvoiceModel> invoices = new List<InvoiceModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT InvoiceNo, Status FROM Invoice ORDER BY Date DESC LIMIT 20", connection))
                {

                    await connection.OpenAsync();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            InvoiceModel invoice = new InvoiceModel()
                            {
                                Id = counter,
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Status = reader["Status"].ToString()
                            };

                            invoices.Add(invoice);
                        }
                    }
                }
                return invoices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to all invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceModel>> SearchInvoiceListAsync(string invoiceNo)
        {
            try
            {
                List<InvoiceModel> invoices = new List<InvoiceModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchInvoiceList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@p_InvoiceNo", invoiceNo);

                    await connection.OpenAsync();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            InvoiceModel invoice = new InvoiceModel()
                            {
                                Id = counter,
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Status = reader["Status"].ToString()
                            };

                            invoices.Add(invoice);
                        }
                    }
                }
                return invoices;
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
                InvoiceModel invoice = null;

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT * FROM Invoice WHERE InvoiceNo = @InvoiceNo", connection))
                {
                    command.Parameters.Add("@InvoiceNo", MySqlDbType.VarChar).Value = invoiceNo;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            invoice = new InvoiceModel()
                            {
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Terms = reader["Terms"] is DBNull ? null : reader["Terms"].ToString(),
                                DateString = reader["Date"] is DBNull ? null : ((DateTime)reader["Date"]).ToString("dd-MM-yyyy @ HH.mm"),
                                DueDateString = reader["DueDate"] is DBNull ? null : ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                                TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                                Status = reader["Status"] is DBNull ? null : reader["Status"].ToString(),
                                CustomerID = reader["CustomerID"] is DBNull ? null : reader["CustomerID"].ToString(),
                                RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString(),
                            };
                        }
                    }
                }
                return invoice;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public InvoiceModel GetByInvoiceNo(string invoiceNo)
        {
            try
            {
                InvoiceModel invoice = null;

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT * FROM Invoice WHERE InvoiceNo = @InvoiceNo", connection))
                {
                    command.Parameters.Add("@InvoiceNo", MySqlDbType.VarChar).Value = invoiceNo;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            invoice = new InvoiceModel()
                            {
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Terms = reader["Terms"] is DBNull ? null : reader["Terms"].ToString(),
                                DateString = reader["Date"] is DBNull ? null : ((DateTime)reader["Date"]).ToString("dd-MM-yyyy @ HH.mm"),
                                DueDateString = reader["DueDate"] is DBNull ? null : ((DateTime)reader["DueDate"]).ToString("dd-MM-yyyy"),
                                TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                                Status = reader["Status"] is DBNull ? null : reader["Status"].ToString(),
                                CustomerID = reader["CustomerID"] is DBNull ? null : reader["CustomerID"].ToString(),
                                RepID = reader["RepID"] is DBNull ? null : reader["RepID"].ToString(),
                            };
                        }
                    }
                }
                return invoice;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceItemModel>> GetInvoiceItems(string invoiceNo)
        {
            try
            {
                List<InvoiceItemModel> invoiceItems = new List<InvoiceItemModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("GetInvoiceItems", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_InvoiceNo", invoiceNo);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            InvoiceItemModel invoiceItem = new InvoiceItemModel()
                            {
                                No = (int)reader["No"],
                                PartNo = reader["PartNo"].ToString(),
                                BrandID = reader["BrandID"].ToString(),
                                Description = reader["Description"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (Decimal)reader["UnitPrice"],
                                Discount = (Decimal)reader["Discount"],
                                Amount = (Decimal)reader["Amount"],
                            };

                            invoiceItems.Add(invoiceItem);
                        }
                    }
                }
                return invoiceItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<IEnumerable<InvoiceItemModel>> GetInvoicesByPartNo(string partNo, int pageSize, int page)
        {
            try
            {
                List<InvoiceItemModel> invoiceItems = new List<InvoiceItemModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("GetInvoicesByPartNo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_PartNo", partNo);
                    command.Parameters.AddWithValue("@p_PageSize", pageSize);
                    command.Parameters.AddWithValue("@p_Offset", (page - 1) * pageSize);

                    int counter = pageSize * (page - 1);
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;
                            InvoiceItemModel invoiceItem = new InvoiceItemModel()
                            {
                                No = counter,
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (Decimal)reader["UnitPrice"],
                                Discount = (Decimal)reader["Discount"],
                                Amount = (Decimal)reader["Amount"],
                            };

                            invoiceItems.Add(invoiceItem);
                        }
                    }
                }
                return invoiceItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoices. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task CancelInvoice(string invoiceNo)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new MySqlCommand("CancelInvoice", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add("@p_InvoiceNo", MySqlDbType.VarChar).Value = invoiceNo;

                            await command.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to cancel the invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<string> SearchInvoiceNumber(string SearchText)
        {
            try
            {
                List<string> invoiceNumbers = new List<string>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchInvoiceNumber", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchText", SearchText);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string invoiceNo = reader["InvoiceNo"].ToString();
                            invoiceNumbers.Add(invoiceNo);
                        }
                    }
                }

                return invoiceNumbers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search invoice. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public List<string> GetPartNumbersByInvoice(string invoiceNo)
        {
            try
            {
                List<string> partNumbers = new List<string>();

                using (var connection = GetConnection())
                {
                    connection.Open();

                    using (var command = new MySqlCommand("GetPartNumbersByInvoice", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("p_InvoiceNo", invoiceNo);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string partNo = reader["PartNo"].ToString();
                                partNumbers.Add(partNo);
                            }
                        }
                    }
                }

                return partNumbers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get part numbers. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<InvoiceItemModel> GetInvoiceItem(string invoiceNo, string partNo)
        {
            try
            {
                InvoiceItemModel invoiceItem = null;

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("GetInvoiceItem", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@p_InvoiceNo", MySqlDbType.VarChar).Value = invoiceNo;
                    command.Parameters.Add("@p_PartNo", MySqlDbType.VarChar).Value = partNo;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            invoiceItem = new InvoiceItemModel()
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
                    }
                }
                return invoiceItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get invoice item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

    }
}
