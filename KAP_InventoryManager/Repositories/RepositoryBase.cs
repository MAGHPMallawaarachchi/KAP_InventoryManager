using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
            _connectionString = "Server=kap-inventory-manager-do-user-14603616-0.c.db.ondigitalocean.com;Port=25060;Database=defaultdb;User Id=doadmin;Password=AVNS_bxluT9_-51uiEsqD2Ll;sslmode=Required;";
        }

        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
