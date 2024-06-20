using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Repositories
{
    internal class RepositoryBase
    {
        private readonly string _connectionString;
        public RepositoryBase()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["kap-inventory-manager-connection-string"].ConnectionString; 
        }

        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        protected async Task ExecuteNonQueryAsync(string query, CommandType commandType, params MySqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        protected async Task<object> ExecuteScalarAsync(string query, CommandType commandType, params MySqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteScalarAsync();
                }
            }
        }

        protected async Task<MySqlDataReader> ExecuteReaderAsync(string query, CommandType commandType, params MySqlParameter[] parameters)
        {
            var connection = GetConnection();
            await connection.OpenAsync();
            try
            {
                var command = new MySqlCommand(query, connection)
                {
                    CommandType = commandType
                };
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch
            {
                await connection.CloseAsync();
                throw;
            }
        }
    }
}
