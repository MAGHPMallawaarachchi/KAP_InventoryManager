using KAP_InventoryManager.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ItemModel item = new ItemModel()
                        {
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

        public IEnumerable<InventoryItemModel> GetAllInventoryItems()
        {
            List<InventoryItemModel> items = new List<InventoryItemModel>();

            using (var connection = GetConnection())
            using (var command = new MySqlCommand("GetItemList", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        InventoryItemModel item = new InventoryItemModel()
                        {
                            AutoID = (Int32)reader["AutoID"],
                            PartNo = reader["PartNo"].ToString(),
                            QtyInHand = (Int32)reader["QtyInHand"]
                        };

                        items.Add(item);
                    }
                }
            }
            return items;
        }


        public ItemModel GetByPartNo(string partNo)
        {
            throw new NotImplementedException();
        }
    }
}
