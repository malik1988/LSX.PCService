using System;

using System.Collections.ObjectModel;
namespace LSX.PCService.Controllers
{
    enum EnumChannel : int
    {
        正常道口 = 1,
        异常道口
    }
    /// <summary>
    /// 按颜色分区，虚拟通道
    /// - 一个颜色只能有一个灯亮
    /// </summary>
    enum LightColor : int
    {
        RED = 0,
        GREEN,

    }
    enum LightOnOffState : int
    {
        /// <summary>
        /// 亮
        /// </summary>
        ON = 0,
        /// <summary>
        /// 灭
        /// </summary>
        OFF,
        /// <summary>
        /// 闪烁
        /// </summary>
        BLINK
    }

    enum LightState
    {
        OFF = 0,
        ON_RED,
        ON_GREEN,
        BLINK_RED,
        BLINK_GREEN
    }

    enum OrderState
    {
        未开始=0,
        已开始,
        货物到达道口,
        货物被取走,
        已完成

    }

    enum DeviceType
    {
        CHANNEL=1,
        CAMERA,
        LIGHT
    }

    /// <summary>
    /// 数据获取接口
    /// 所有函数都是原子性、线程安全型
    /// </summary>
    class DbHelper
    {
        #region 进行中的订单表


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
        public static int? GetBindedLightByOrder(int orderId) { return 123; }



        /// <summary>
        /// 订单异常信息记录
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <param name="error">异常信息</param>
        public static void OrderErrorLogAdd(int orderId, string error) { }
        /// <summary>
        /// 通过箱号获取当前订单编号
        /// - 如果当前订单中存在该箱号，则返回箱号对应的ID
        /// - 如果不存在，则返回空
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static int? GetOrderIdByBoxId(string box)
        {
            return null;
        }
        /// <summary>
        /// 创建订单号
        /// - 如果箱号在当前任务总表中不存在，则返回空
        /// - 如果箱号在当前任务总表中存在
        ///     - 如果箱号对应的实时任务订单已完成（重复扫描相同的箱号），返回已存在的箱号
        ///     - 其他，创建并返回订单号
        /// </summary>
        /// <param name="box">箱号</param>
        /// <returns></returns>
        public static int? CreateOrderIdByBoxId(string box) { return 12; }
        /// <summary>
        /// 检查订单状态是否已完成（防止重复扫描相同的箱号）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool OrderIsFinished(int orderId) { return false; }

        public static void FinishOrderById(int orderId) { }
        /// <summary>
        /// 设置订单状态
        /// </summary>
        /// <param name="orderId"></param>
        public static void SetOrderState(int orderId,int state) { }
        public static void SetOrderRealChannel(int orderId, EnumChannel channel) { }
        #endregion
        /// <summary>
        /// 根据订单号获取对应的09码，并判定09码对应的任务总数是否已经集齐完成
        /// - 多表联合查询
        /// - 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool CheckIsLightOrderFinished(int orderId)
        { return false; }
        /// <summary>
        /// 设置灯状态
        /// </summary>
        /// <param name="on">点亮</param>
        /// <param name="color">颜色（红色、绿色）</param>
        public static void SetLightState(int lightId, LightState state) { }
        /// <summary>
        /// 设置灯状态（亮、灭、闪烁）
        /// - 灯的颜色由灯ID已经决定了
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="onOff"></param>
        public static void SetLightState(int lightId, LightOnOffState onOff) { }
        /// <summary>
        /// 设置灯颜色，写入灯表
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="color"></param>
        public static void SetLightColor(int lightId, LightColor color) { }
        public static LightColor GetLightColor(int lightId) { return LightColor.RED; }

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

        #region 各种任务表相关
        /// <summary>
        /// 从发车明细Excel文件导入数据到AWMS原始数据表中
        /// - 相同的箱号对应的数量合并
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <param name="processHandler">进度回调函数</param>
        /// <returns></returns>
        public static ErrorCode ImportExcelToAwms(string fileName, EventHandler<int> processHandler)
        {
            int i = 0;
            while (i++ < 100)
            {
                if (processHandler != null)
                {
                    processHandler.Invoke(null, i);
                }
                System.Threading.Thread.Sleep(100);

            }
            return ErrorCode.成功;
        }
        /// <summary>
        /// 开启所有订单
        /// </summary>
        /// <returns>成功/失败</returns>
        public static bool StartOrders() { return true; }
        /// <summary>
        /// 所有订单是否都已经完成
        /// </summary>
        /// <returns>是/否</returns>

        public static bool IsAllOrdersFinished() { return true; }
        /// <summary>
        /// 添加发货单号到发货单任务表
        /// - 发货单任务表中存在
        ///     - 如果发货单状态为已完成/进行中，则不处理
        /// - 发货单任务表中不存在则添加，并标记发货单状态为正在进行中
        /// </summary>
        /// <param name="orders">发车单号列表</param>
        public static void AddTrafficOrderToTaskTable(Collection<string> orders) { }
        /// <summary>
        /// 检查发货单号是否存在于原始AWMS数据中
        /// - 原始数据中存在该发货单号，返回真
        /// </summary>
        /// <param name="order">发货单号</param>
        /// <returns></returns>
        public static bool CheckIsTrafficOrderExistInAwms(string order) { return true; }
        /// <summary>
        /// 从AWMS表中查询栈板是否整托（栈板号对应的09码唯一）
        /// - 如果栈板号不存在则返回空
        /// - 栈板存在则返回是否整托
        /// </summary>
        /// <param name="palletId">栈板号</param>
        /// <returns></returns>
        public static bool? IsPalletSingle(string palletId) { return true; }
        /// <summary>
        /// 当前栈板是否需要质检
        /// </summary>
        /// <param name="palletId">栈板编号</param>
        /// <returns></returns>
        public static bool? IsPalletNeedQuantityCheck(string palletId) { return false; }
        /// <summary>
        /// 获取当前栈板需求灯数量
        /// - 返回当前栈板中不同的09码的数量
        /// - 如果当前栈板在系统中不存在则返回为空
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public static int? GetPalletNeedLightsCount(string palletId) { return 10; }
        /// <summary>
        /// 添加栈板编号到栈板任务表中(开启栈板任务)
        /// </summary>
        /// <param name="palletId"></param>
        public static void AddPalletToRunningTask(string palletId) { }
        #endregion

        public static string GetC09ByBoxId(string boxId) { return "C0901991"; }
        public static object GetC09Info(string c09) { return new { 
            C09="09码",
            已完成数量=3,
            正常道口完成数量=1,
            异常道口完成数量=2,

            总数=10            
        }; }
        public static ObservableCollection<string> GetTrafficOrderList() { return new ObservableCollection<string>() { "112", "2223", "1122" }; }
        /// <summary>
        /// 获取所有订单信息
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<object> GetAllOrderInfomation()
        {
            var o=new ObservableCollection<object>();

            for (int i = 0; i < 1000;i++ )
            {
                o.Add(new { ID = i, NAME = i.ToString() });
            }
            return o;
        }
        /// <summary>
        /// 添加灯到 灯表
        /// </summary>
        /// <param name="lightId">灯ID</param>

        public static void AddLight(int lightId) { }
        public static void RemoveLight(int lightId) { }

        public static object GetDeviceState(DeviceType dev) { return new {IP="192.168.1.100"};}
        /// <summary>
        /// 通过箱号复位订单
        /// </summary>
        /// <param name="boxId"></param>
        /// <returns>复位成功、失败</returns>
        public static ErrorCode ResetOrderByBoxId(string boxId) { return ErrorCode.成功; }

    }
}
