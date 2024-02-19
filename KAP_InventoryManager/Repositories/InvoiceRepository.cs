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
    internal class InvoiceRepository : RepositoryBase, IInvoiceRepository
    {
        public void AddInvoice(InvoiceModel invoice)
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
                        command.Parameters.Add("@p_TotalAmount", MySqlDbType.Decimal).Value = invoice.TotalAmount;
                        command.Parameters.Add("@p_CustomerID", MySqlDbType.VarChar).Value = invoice.CustomerID;
                        command.Parameters.Add("@p_RepID", MySqlDbType.VarChar).Value = invoice.RepID;

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public void AddInvoiceItem(InvoiceItemModel invoiceItem)
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

        public string GetNextInvoiceNumber()
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

        public async Task<IEnumerable<InvoiceModel>> GetInvoiceByCustomerAsync(string customerId, int pageSize, int page)
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
                            Status = reader["status"].ToString()
                        };

                        invoices.Add(invoice);
                    }
                }
            }
            return invoices;
        }

        public async Task<IEnumerable<InvoiceModel>> SearchCustomerInvoiceListAsync(string invoiceNo, string customerId, int pageSize, int page)
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
                            Status = reader["status"].ToString()
                        };

                        invoices.Add(invoice);
                    }
                }
            }
            return invoices;
        }
    }
}
