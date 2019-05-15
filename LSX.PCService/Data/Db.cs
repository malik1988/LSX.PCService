using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dos.ORM;
using Dos.Common;

using System.Data;
using System.Configuration;

using MySql.Data.MySqlClient;
using Chloe;
using Chloe.MySql;
using Chloe.Infrastructure;
using Chloe.Extensions;

namespace LSX.PCService.Data
{
    class Db
    {
        public static readonly SqlDataContext Context = new SqlDataContext(Config.SqlServerConn);      

    }
    #region mysql
    /// <summary>
    /// 数据库服务类，屏蔽SQL库操作
    /// </summary>
    public class SqlDataContext : MySqlContext
    {
        // static string connectionString = "Server=localhost;Database=lh_db_test;Uid=root;Pwd=mysql!@#;";
        public SqlDataContext(string connection) :
            base(new SqlConnectionFactory(connection))
        {
        }

    }
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        string _connString = null;
        public SqlConnectionFactory(string connString)
        {
            this._connString = connString;
        }
        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new MySqlConnection(this._connString);

            return conn;

        }
    }
    #endregion
}
