using GalaSoft.MvvmLight;
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
    internal class SalesRepRepository : RepositoryBase, ISalesRepRepository
    {
        public List<string> GetAllRepIds()
        {
            try
            {
                List<string> salesReps = new List<string>
                {
                    "None"
                };

                using (var connection = GetConnection())
                using (var command = new MySqlCommand("SELECT RepID FROM SalesRep", connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string repId = reader["RepID"].ToString();
                            salesReps.Add(repId);
                        }
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
    }
}
