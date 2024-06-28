using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace KAP_InventoryManager.Repositories
{
    internal class ItemRepository : RepositoryBase, IItemRepository
    {
        public async Task AddAsync(ItemModel item)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", item.PartNo),
                    new MySqlParameter("@p_OEMNo", item.OEMNo),
                    new MySqlParameter("@p_Description", item.Description),
                    new MySqlParameter("@p_BrandID", item.BrandID),
                    new MySqlParameter("@p_Category", item.Category),
                    new MySqlParameter("@p_VehicleBrand", item.VehicleBrand),
                    new MySqlParameter("@p_BuyingPrice", item.BuyingPrice),
                    new MySqlParameter("@p_UnitPrice", item.UnitPrice),
                    new MySqlParameter("@p_ItemCount", MySqlDbType.Int32) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("AddItem", CommandType.StoredProcedure, parameters);

                int itemCount = Convert.ToInt32(parameters[9].Value);

                if (itemCount == 0)
                {
                    MessageBox.Show("Item added successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Item already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task EditAsync(ItemModel item)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", item.PartNo),
                    new MySqlParameter("@p_OEMNo", item.OEMNo),
                    new MySqlParameter("@p_Description", item.Description),
                    new MySqlParameter("@p_BrandID", item.BrandID),
                    new MySqlParameter("@p_Category", item.Category),
                    new MySqlParameter("@p_VehicleBrand", item.VehicleBrand),
                    new MySqlParameter("@p_BuyingPrice", item.BuyingPrice),
                    new MySqlParameter("@p_UnitPrice", item.UnitPrice),
                    new MySqlParameter("@p_AffectedRows", MySqlDbType.Int32) { Direction = ParameterDirection.Output }
                };

                await ExecuteNonQueryAsync("EditItem", CommandType.StoredProcedure, parameters);

                int affectedRows = Convert.ToInt32(parameters[9].Value);

                if (affectedRows > 0)
                {
                    MessageBox.Show("Item updated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Item does not exist or no changes were made.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to edit the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<List<ItemModel>> GetAllAsync()
        {
            var items = new List<ItemModel>();
            try
            {
                using (var reader = await ExecuteReaderAsync("SELECT PartNo, QtyInHand FROM Item ORDER BY BrandID DESC LIMIT 20", CommandType.Text))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        items.Add(new ItemModel
                        {
                            Id = counter,
                            PartNo = reader["PartNo"].ToString(),
                            QtyInHand = (int)reader["QtyInHand"]
                        });
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
            var items = new List<ItemModel>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", partNo)
                };

                using (var reader = await ExecuteReaderAsync("SearchItemList", CommandType.StoredProcedure, parameters))
                {
                    int counter = 0;
                    while (await reader.ReadAsync())
                    {
                        counter++;
                        items.Add(new ItemModel
                        {
                            Id = counter,
                            PartNo = reader["PartNo"].ToString(),
                            QtyInHand = (int)reader["QtyInHand"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return items;
        }

        public async Task<ItemModel> GetByPartNoAsync(string partNo)
        {
            ItemModel item = null;

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@PartNo", partNo)
                };

                using (var reader = await ExecuteReaderAsync("SELECT * FROM Item WHERE PartNo = @PartNo", CommandType.Text, parameters))
                {
                    if (await reader.ReadAsync())
                    {
                        item = new ItemModel
                        {
                            PartNo = reader["PartNo"].ToString(),
                            OEMNo = reader["OEMNo"] is DBNull ? null : reader["OEMNo"].ToString(),
                            Description = reader["Description"] is DBNull ? null : reader["Description"].ToString(),
                            BrandID = reader["BrandID"] is DBNull ? null : reader["BrandID"].ToString(),
                            Category = reader["Category"] is DBNull ? null : reader["Category"].ToString(),
                            TotalQty = reader["TotalQty"] is DBNull ? 0 : Convert.ToInt32(reader["TotalQty"]),
                            QtyInHand = reader["QtyInHand"] is DBNull ? 0 : Convert.ToInt32(reader["QtyInHand"]),
                            QtySold = reader["QtySold"] is DBNull ? 0 : Convert.ToInt32(reader["QtySold"]),
                            BuyingPrice = reader["BuyingPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["BuyingPrice"]),
                            UnitPrice = reader["UnitPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["UnitPrice"]),
                            VehicleBrand = reader["VehicleBrand"] is DBNull ? null : reader["VehicleBrand"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return item;
        }

        public async Task<int> GetItemCountAsync()
        {
            int itemCount = 0;
            try
            {
                var result = await ExecuteScalarAsync("SELECT COUNT(PartNo) AS ItemCount FROM Item", CommandType.Text);
                if (result != null && result != DBNull.Value)
                {
                    itemCount = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item count. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return itemCount;
        }

        public async Task<int> GetOutOfStockCountAsync()
        {
            int itemCount = 0;
            try
            {
                var result = await ExecuteScalarAsync("SELECT COUNT(PartNo) AS ItemCount FROM Item WHERE QtyInHand = 0", CommandType.Text);
                if (result != null && result != DBNull.Value)
                {
                    itemCount = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the out of stock count. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return itemCount;
        }

        public async Task<int> GetCategoryCountAsync()
        {
            int categoryCount = 0;
            try
            {
                var result = await ExecuteScalarAsync("SELECT COUNT(*) AS CategoryCount FROM BrandCategory", CommandType.Text);
                if (result != null && result != DBNull.Value)
                {
                    categoryCount = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get the item category count. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return categoryCount;
        }

        public async Task<decimal> CalculateCurrentMonthRevenueByItemAsync(string partNo)
        {
            decimal currentMonthRevenue = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", partNo)
                };

                var result = await ExecuteScalarAsync("SELECT CalculateCurrentMonthRevenueByItem(@p_PartNo)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    currentMonthRevenue = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate current month revenue. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return currentMonthRevenue;
        }

        public async Task<decimal> CalculateLastMonthRevenueByItemAsync(string partNo)
        {
            decimal lastMonthRevenue = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", partNo)
                };

                var result = await ExecuteScalarAsync("SELECT CalculateLastMonthRevenueByItem(@p_PartNo)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    lastMonthRevenue = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate last month revenue. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lastMonthRevenue;
        }

        public async Task<decimal> CalculateTodayRevenueByItemAsync(string partNo)
        {
            decimal todayRevenue = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_PartNo", partNo)
                };

                var result = await ExecuteScalarAsync("SELECT CalculateTodayRevenueByItem(@p_PartNo)", CommandType.Text, parameters);
                if (result != null && result != DBNull.Value)
                {
                    todayRevenue = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate today's revenue. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return todayRevenue;
        }

        public async Task<decimal> CalculatePercentageChangeAsync(decimal currentMonthRevenue, decimal lastMonthRevenue)
        {
            decimal percentageChange = 0;
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_CurrentMonthRevenue", currentMonthRevenue),
                    new MySqlParameter("@p_LastMonthRevenue", lastMonthRevenue)
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

        public bool CheckQty(string partNo, int qty)
        {
            bool isAvailable = false;

            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check the qunatity. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return isAvailable;
        }

        public async Task<List<string>> SearchPartNoAsync(string searchText)
        {
            var partNumbers = new List<string>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@SearchText", searchText)
                };

                using (var reader = await ExecuteReaderAsync("SearchPartNo", CommandType.StoredProcedure, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        partNumbers.Add(reader["PartNo"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search part numbers. Error: {ex.Message}", ex);
            }

            return partNumbers;
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            var brands = new List<string>();

            try
            {
                using (var reader = await ExecuteReaderAsync("SELECT BrandID FROM Brand", CommandType.Text))
                {
                    while (await reader.ReadAsync())
                    {
                        brands.Add(reader["BrandID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get brands. Error: {ex.Message}", ex);
            }

            return brands;
        }

        public async Task<string> GetSupplierByBrandAsync(string brand)
        {
            string supplier = null;

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@BrandID", brand)
                };

                var result = await ExecuteScalarAsync("SELECT SupplierID FROM Brand WHERE BrandID = @BrandID", CommandType.Text, parameters);
                if (result != null)
                {
                    supplier = result.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get supplier by brand. Error: {ex.Message}", ex);
            }

            return supplier;
        }

        public async Task<List<string>> GetCategoriesAsync(string brandId)
        {
            var categories = new List<string>();

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@BrandID", brandId)
                };

                using (var reader = await ExecuteReaderAsync("SELECT Category FROM BrandCategory WHERE BrandID = @BrandID", CommandType.Text, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(reader["Category"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories. Error: {ex.Message}", ex);
            }

            return categories;
        }

        public async Task ExportDataToCSVAsync(string brandId)
        {
            string query = @"
                SELECT PartNo, OEMNo, Category, Description, VehicleBrand, QtyInHand, DamagedQty, BuyingPrice, UnitPrice
                FROM Item
                WHERE BrandID = @BrandID
                ORDER BY Category;";

            string folderPath = @"C:/Users/Hasini/OneDrive/Documents/Kamal Auto Parts/stock reports/";
            string currentDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{brandId}_{currentDateTime}.csv";
            string filePath = Path.Combine(folderPath, fileName);

            MySqlParameter brandIdParameter = new MySqlParameter("@BrandID", MySqlDbType.VarChar)
            {
                Value = brandId
            };

            try
            {
                using (var reader = await ExecuteReaderAsync(query, CommandType.Text, brandIdParameter))
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        // Write CSV header
                        writer.WriteLine("PartNo,OEMNo,Category,Description,VehicleBrand,QtyInHand,DamagedQty,BuyingPrice,UnitPrice");

                        // Write CSV rows
                        while (await reader.ReadAsync())
                        {
                            string[] row = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[i] = QuoteValue(reader[i].ToString());
                            }
                            writer.WriteLine(string.Join(",", row));
                        }
                    }
                }

                Console.WriteLine($"Data exported to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private string QuoteValue(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }
    }
}
