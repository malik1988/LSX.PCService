using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dos.ORM;
using Dos.Common;
namespace LSX.PCService.Data
{
    class Db
    {
        public static readonly DbSession Context = new DbSession("SqlServerConn");
        static Db()
        {
            Context.RegisterSqlLogger(delegate (string sql){
                LogHelper.Debug(sql, "SQL日志");
            });
        }
    }
}
