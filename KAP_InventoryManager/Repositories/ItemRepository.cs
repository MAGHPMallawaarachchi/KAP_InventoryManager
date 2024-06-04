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
            try
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
                            command.Parameters.Add("@p_VehicleBrand", MySqlDbType.VarChar).Value = item.VehicleBrand;
                            command.Parameters.Add("@p_SupplierID", MySqlDbType.VarChar).Value = item.SupplierID;
                            command.Parameters.Add("@p_BuyingPrice", MySqlDbType.Decimal).Value = item.BuyingPrice;
                            command.Parameters.Add("@p_UnitPrice", MySqlDbType.Decimal).Value = item.UnitPrice;

                            var p_ItemCount = new MySqlParameter("@p_ItemCount", MySqlDbType.Int32)
                            {
                                Direction = ParameterDirection.Output
                            };
                            command.Parameters.Add(p_ItemCount);

                            command.ExecuteNonQuery();

                            // Get the value of the output parameter
                            int itemCount = Convert.ToInt32(p_ItemCount.Value);

                            if (itemCount == 0)
                            {
                                MessageBox.Show("Item added successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Item already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        transaction.Commit();
                    }        
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public async Task<List<ItemModel>> GetAllAsync()
        {
            var items = new List<ItemModel>();

            try
            {
                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT PartNo, QtyInHand FROM Item ORDER BY BrandID DESC LIMIT 20", connection))
                {
                    await connection.OpenAsync();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            ItemModel item = new ItemModel()
                            {
                                Id = counter,
                                PartNo = reader["PartNo"].ToString(),
                                QtyInHand = (int)reader["QtyInHand"],
                            };

                            items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get all items. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return items;
        }

        public async Task<IEnumerable<ItemModel>> SearchItemListAsync(string partNo)
        {
            try
            {
                List<ItemModel> items = new List<ItemModel>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SearchItemList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_PartNo", partNo);

                    connection.Open();
                    int counter = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            counter++;

                            ItemModel item = new ItemModel()
                            {
                                Id = counter,
                                PartNo = reader["PartNo"].ToString(),
                                QtyInHand = (Int32)reader["QtyInHand"],
                            };

                            items.Add(item);
                        }
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<ItemModel> GetByPartNoAsync(string partNo)
        {
            try
            {
                ItemModel item = null;

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT * FROM Item WHERE PartNo = @PartNo", connection))
                {
                    command.Parameters.Add("@PartNo", MySqlDbType.VarChar).Value = partNo;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public ItemModel GetByPartNo(string partNo)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public List<string> SearchPartNo(string SearchText)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get search the part number. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<int> GetItemCount()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item count. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }


        public async Task<int> GetOutOfStockCount()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the out of stock count. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        public int GetLowInStockCount()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetCategoryCount()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item category count. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        public bool CheckQty(string partNo, int qty)
        {
            try
            {
                bool isAvailable;
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT CheckQty(@PartNo, @Quantity)", connection))
                    {
                        command.Parameters.AddWithValue("@PartNo", partNo);
                        command.Parameters.AddWithValue("@Quantity", qty);

                        isAvailable = Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
                return isAvailable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check the qunatity. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        public List<string> GetBrands()
        {
            try
            {
                List<string> brands = new List<string>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT BrandID FROM Brand", connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string brand = reader["BrandID"].ToString();
                            brands.Add(brand);
                        }
                    }
                }
                return brands;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get brand. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public string GetSupplierByBrand(string brand)
        {
            try
            {
                string supplier = null;
                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT SupplierID FROM Brand WHERE BrandID = @BrandID", connection))
                {
                    command.Parameters.Add("@BrandID", MySqlDbType.VarChar).Value = brand;
                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        supplier = result.ToString();
                    }
                }
                return supplier;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the supplier. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public List<string> GetCategories(string brandId)
        {
            try
            {
                List<string> categories = new List<string>();

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT Category FROM BrandCategory WHERE BrandID = @BrandID", connection))
                {
                    command.Parameters.Add("@BrandID", MySqlDbType.VarChar).Value = brandId;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string category = reader["Category"].ToString();
                            categories.Add(category);
                        }
                    }
                }
                return categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the categories. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
