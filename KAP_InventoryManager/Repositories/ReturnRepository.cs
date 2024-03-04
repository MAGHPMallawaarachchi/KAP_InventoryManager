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
    }
}
