using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class ItemRepository : RepositoryBase, IItemRepository
    {
        public void Add(ItemModel item)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new MySqlCommand("AddItem", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@p_PartNo", MySqlDbType.VarChar).Value = item.PartNo;
                        command.Parameters.Add("@p_OEMNo", MySqlDbType.VarChar).Value = item.OEMNo;
                        command.Parameters.Add("@p_Description", MySqlDbType.VarChar).Value = item.Description;
                        command.Parameters.Add("@p_BrandID", MySqlDbType.VarChar).Value = item.BrandID;
                        command.Parameters.Add("@p_Category", MySqlDbType.VarChar).Value = item.Category;
                        command.Parameters.Add("@p_SupplierID", MySqlDbType.VarChar).Value = item.SupplierID;
                        command.Parameters.Add("@p_BuyingPrice", MySqlDbType.Decimal).Value = item.BuyingPrice;
                        command.Parameters.Add("@p_UnitPrice", MySqlDbType.Decimal).Value = item.UnitPrice;

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }        
            }
        }

        public void Delete(string partNo)
        {
            throw new NotImplementedException();
        }

        public void Edit(ItemModel itemModel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ItemModel> GetAll()
        {
            List<ItemModel> items = new List<ItemModel>();

            using (var connection = GetConnection())
            using (var command = new MySqlCommand("SELECT * FROM Item", connection))
            {
                connection.Open();
                int counter = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        counter++;

                        ItemModel item = new ItemModel()
                        {
                            Id = counter,
                            PartNo = reader["PartNo"].ToString(),
                            OEMNo = reader["OemNo"].ToString(),
                            Description = reader["Description"].ToString(),
                            BrandID = reader["BrandID"].ToString(),
                            Category = reader["Category"].ToString(),
                            SupplierID = reader["SupplierID"].ToString(),
                            TotalQty = (Int32)reader["TotalQty"],
                            QtySold = (Int32)reader["QtySold"],
                            QtyInHand = (Int32)reader["QtyInHand"],
                            BuyingPrice = (Decimal)reader["BuyingPrice"],
                            UnitPrice = (Decimal)reader["UnitPrice"]
                        };

                        items.Add(item);
                    }
                }
            }
            return items;
        }

        public ItemModel GetByPartNo(string partNo)
        {
            ItemModel item = null;

            using (var connection = GetConnection())
            using (var command = new MySqlCommand("SELECT * FROM Item WHERE PartNo = @PartNo", connection))
            {
                command.Parameters.Add("@PartNo", MySqlDbType.VarChar).Value = partNo;

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item = new ItemModel()
                        {
                            PartNo = reader["PartNo"].ToString(),
                            OEMNo = reader["OEMNo"] is DBNull ? null : reader["OEMNo"].ToString(),
                            Description = reader["Description"] is DBNull ? null : reader["Description"].ToString(),
                            BrandID = reader["BrandID"] is DBNull ? null : reader["BrandID"].ToString(),
                            Category = reader["Category"] is DBNull ? null : reader["Category"].ToString(),
                            SupplierID = reader["SupplierID"] is DBNull ? null : reader["SupplierID"].ToString(),
                            TotalQty = reader["TotalQty"] is DBNull ? 0 : Convert.ToInt32(reader["TotalQty"]),
                            QtyInHand = reader["QtyInHand"] is DBNull ? 0 : Convert.ToInt32(reader["QtyInHand"]),
                            QtySold = reader["QtySold"] is DBNull ? 0 : Convert.ToInt32(reader["QtySold"]),
                            BuyingPrice = reader["BuyingPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["BuyingPrice"]),
                            UnitPrice = reader["UnitPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["UnitPrice"]),
                        };
                    }
                }
            }

            return item;
        }

        public List<string> SearchPartNo(string SearchText)
        {
            List<string> partNumbers = new List<string>();

            using (var connection = GetConnection())
            using (var command = new MySqlCommand("SearchPartNo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@SearchText", SearchText);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string partNo = reader["PartNo"].ToString();
                        partNumbers.Add(partNo);
                    }
                }
            }

            return partNumbers;
        }

        public async Task<int> GetItemCount()
        {
            int itemCount = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT COUNT(PartNo) AS ItemCount FROM Item", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string itemCountString = reader["ItemCount"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("ItemCount")))
                        {
                            itemCount = int.Parse(itemCountString);
                        }
                    }
                }
            }

            return itemCount;
        }


        public async Task<int> GetOutOfStockCount()
        {
            int itemCount = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT COUNT(PartNo) AS ItemCount FROM Item WHERE QtyInHand = 0", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string itemCountString = reader["ItemCount"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("ItemCount")))
                        {
                            itemCount = int.Parse(itemCountString);
                        }
                    }
                }
            }

            return itemCount;
        }

        public int GetLowInStockCount()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetCategoryCount()
        {
            int categoryCount = 0;

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT COUNT(*) AS CategoryCount FROM BrandCategory", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string categoryCountString = reader["CategoryCount"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("CategoryCount")))
                        {
                            categoryCount = int.Parse(categoryCountString);
                        }
                    }
                }
            }

            return categoryCount;
        }

    }
}
