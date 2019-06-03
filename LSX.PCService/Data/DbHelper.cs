using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LSX.PCService.Models;
using Chloe;
namespace LSX.PCService.Data
{
    enum EnumChannel : int
    {
        道口1 = 1,
        道口2,
        异常道口
    }

    enum EnumOrderStatus : int
    {
        创建订单 = 1,
        已发送,

        已完成
    }
    class DbHelper
    {
        public static void InitLights()
        {
            EmptyTable("light");
        }
        //<summary>
        //将枚举类型 EnumChannel写入数据库Channel表中
        //</summary>
        public static void InitChannel()
        {
            EmptyTable("Channel");
            foreach (var x in Enum.GetValues(typeof(EnumChannel)))
            {
                Channel c = new Channel()
                {
                    Id = (int)x,
                    Name = ((EnumChannel)x).ToString(),
                    Special = false
                };
                if ((EnumChannel)x == EnumChannel.异常道口)
                    c.Special = true;

                Db.Context.Insert<Channel>(c);
            }
        }

        public static void InitOrderStatus()
        {
            EmptyTable("Order_Status");
            foreach (var x in Enum.GetValues(typeof(EnumOrderStatus)))
            {
                OrderStatus c = new OrderStatus()
                {
                    Id = (int)x,
                    Status = ((EnumOrderStatus)x).ToString(),
                };

                Db.Context.Insert<OrderStatus>(c);
            }
        }

        public static int EmptyTable(string name)
        {
            return Db.Context.Session.ExecuteNonQuery(string.Format("SET FOREIGN_KEY_CHECKS = 0;truncate {0}; SET FOREIGN_KEY_CHECKS = 1; ", name));
        }



        /// <summary>
        /// 预先分配通道
        /// 平分原则(每个通道中箱子的总数相同)
        /// </summary>
        /// <returns></returns>
        public static void PreAllocateChannel()
        {
            var c09List = Db.Context.Query<OrderRawAnalyzed>().Where(a => a.整托 != true).Select(a => new { qz = Sql.Count(), c09 = a.C09码 }).GroupBy(a => a.c09).Select(a => a.c09).ToList();
            //Random rnd = new Random();
            int i = 0;
            foreach (var c09 in c09List)
            {

                Db.Context.Update<OrderRawAnalyzed>(a => a.C09码 == c09, a => new OrderRawAnalyzed()
                {
                    Channel_id = (i % 2 + 1)
                    // rnd.Next(1, (int)EnumChannel.异常道口)
                });
                i++;
            }
        }

        public static bool? IsSinglePallet(string palletId)
        {
            return Db.Context.Query<AwmsRawData>().LeftJoin<OrderRawAnalyzed>((a, o) => a.Id == o.Raw_id).Where((a, o) => a.栈板号 == palletId).Select((a, o) => o.整托).FirstOrDefault();

        }

        public static List<LpnC09> GetAllLpnC09OrderByTimeLastFirst()
        {
            return Db.Context.Query<LpnC09>().ToList();
        }

        public static bool IsCarIdExist(string carId)
        {
            return false;
        }
    }
}
