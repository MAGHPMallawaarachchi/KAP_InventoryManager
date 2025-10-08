using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KAP_InventoryManager.Repositories
{
    internal class RepositoryBase
    {
        private readonly string _connectionString;
        protected static readonly SemaphoreSlim ConnectionSemaphore = new SemaphoreSlim(45, 45);

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
            await ConnectionSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = commandType;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                ConnectionSemaphore.Release();
            }
        }

        protected async Task<object> ExecuteScalarAsync(string query, CommandType commandType, params MySqlParameter[] parameters)
        {
            await ConnectionSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = commandType;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        return await command.ExecuteScalarAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                ConnectionSemaphore.Release();
            }
        }

        protected async Task<MySqlDataReader> ExecuteReaderAsync(string query, CommandType commandType, params MySqlParameter[] parameters)
        {
            await ConnectionSemaphore.WaitAsync().ConfigureAwait(false);
            var connection = GetConnection();
            var handlerAttached = false;
            try
            {
                await connection.OpenAsync().ConfigureAwait(false);
                connection.StateChange += OnConnectionStateChange;
                handlerAttached = true;

                var command = new MySqlCommand(query, connection)
                {
                    CommandType = commandType
                };
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
            }
            catch
            {
                if (handlerAttached)
                {
                    connection.StateChange -= OnConnectionStateChange;
                }

                await connection.CloseAsync().ConfigureAwait(false);
                ConnectionSemaphore.Release();
                throw;
            }
        }

        private static void OnConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed || e.CurrentState == ConnectionState.Broken)
            {
                if (sender is MySqlConnection connection)
                {
                    connection.StateChange -= OnConnectionStateChange;
                }

                ConnectionSemaphore.Release();
            }
        }
    }
}
