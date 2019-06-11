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
        正常道口 = 1,
        异常道口
    }


    class DbHelper
    {
        /// <summary>
        /// 获取当前订单对应的通道是否是正常通道
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <returns></returns>
        public static bool GetCurrentOrderChannel(int orderId) { return true; }
        /// <summary>
        /// 获取订单对应的灯ID
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static int? GetLightIdByOrder(int orderId) { return 0; }
        /// <summary>
        /// 获取未使用的灯
        /// </summary>
        /// <returns></returns>
        public static int? GetUnsedLightId() { return 0; }

        public static void BindLightIdToOrder(int orderId, int lightId) { }
        /// <summary>
        /// 设置当前灯为占用状态
        /// </summary>
        /// <param name="lightId"></param>
        public static void SetLightOccupied(int lightId) { }
        /// <summary>
        /// 设置当前灯未占用
        /// </summary>
        /// <param name="lightId"></param>
        public static void SetLightUnOccupied(int lightId) { }
        /// <summary>
        /// 通过OrderId获取订单绑定的灯编号
        /// 内部必须是原子性获取
        /// 1. 之前已经绑定过，直接返回灯ID
        /// 2. 从未绑定过
        ///    - 获取可用的灯编号
        ///    - 将可用的灯编号写入订单
        ///    - 返回灯编号
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns>灯编号</returns>
        public static int? GetBindedLightByOrder(int orderId) { return 0; }

        ///// <summary>
        ///// 开启事务
        ///// </summary>
        //public static void BeginTransaction() { }
        ///// <summary>
        ///// 关闭事务
        ///// </summary>
        //public static void EndTransaction() { }

        /// <summary>
        /// 订单异常信息记录
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <param name="error">异常信息</param>
        public static void OrderErrorLogAdd(int orderId, string error) { }


        public static void SetLightState(bool on,int color) { }

        public static void AddLightOrder() { }
        /// <summary>
        /// 根据灯号检查当前物料是否集齐
        /// - 灯对应的09码对应的箱对应的订单全部完成
        /// </summary>
        /// <param name="lightId"></param>
        /// <returns></returns>
        public static bool CheckIsFullByLight(int lightId) { return false; }

        /// <summary>
        /// 绑定LPN 09码 库位
        /// </summary>
        /// <param name="lpn">LPN码</param>
        /// <param name="c09">09码</param>
        /// <param name="loc">库位</param>
        public static void BindingLpnAndC09(string lpn, string c09, string loc) { }
        /// <summary>
        /// 通过09码获取目标库位
        /// - 联合订单表和目标库位表 
        /// - 查找09码匹配的目标库位
        /// </summary>
        /// <param name="c09"></param>
        /// <returns></returns>
        public static string GetTargetLocationByC09(string c09) { return ""; }
        
    }
}
