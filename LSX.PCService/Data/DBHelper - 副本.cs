using System;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// using LSX.PCService.Models;
// using Chloe;
using System.Collections.ObjectModel;
using MySql.Data;
using MySql.Data.MySqlClient;
using LSX.PCService.Controllers;
using System.ComponentModel;

namespace LSX.PCService.Data
{
    enum EnumChannel : int
    {
        正常道口 = 1,
        异常道口 = 0
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
        未开始 = 0,
        已开始,
        货物到达道口,
        货物被取走,
        已完成

    }

    enum DeviceType
    {
        CHANNEL = 1,
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
        /// 根据excle的路径把第一个sheel中的内容放入datatable
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="processHandler"></param>
        /// <returns></returns>
        public static ErrorCode ImportExcelToAwms(string filePath, EventHandler<int> processHandler)
        {
            /// <summary>
            /// 检查发货单号是否存在于原始AWMS数据中
            /// - 原始数据中存在该发货单号，返回真
            /// </summary>
            /// <param name="order">发货单号</param>
            /// <returns></returns>
            if (!File.Exists(filePath))
            {
                return ErrorCode.文件导入_无效的Excel文件;
            }
            string fileType = System.IO.Path.GetExtension(filePath);

            Console.WriteLine(fileType);

            string strConn = "";
            if (fileType == ".xls")
            {
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            }
            else
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            }

            OleDbConnection excelConn = new OleDbConnection(strConn);
            excelConn.Open();
            DataTable sheetsName = excelConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
            string sheetName = sheetsName.Rows[0][2].ToString();
            Console.WriteLine(sheetName);
            string sql = string.Format("SELECT * FROM [{0}]", sheetName);


            OleDbDataAdapter ada = new OleDbDataAdapter(sql, strConn);

            DataSet dsItem = new DataSet();
            ada.Fill(dsItem);
            DataTable dt = dsItem.Tables[0];

            MySqlConnection sqlConn = new MySqlConnection(Config.SqlServerConn);

            sqlConn.Open();
            int maxCount = dt.Rows.Count;

            for (int i = 0; i < maxCount; i++)
            {
                string tOrder = dt.Rows[i][1].ToString();
                string vehicle = dt.Rows[i][2].ToString();
                string destination = dt.Rows[i][4].ToString();
                string pallet = dt.Rows[i][5].ToString();
                string carton = dt.Rows[i][6].ToString();
                string materiel = dt.Rows[i][11].ToString();
                int number = Convert.ToInt32(dt.Rows[i][14].ToString());
                string receipt = dt.Rows[i][15].ToString();
                string inspection = dt.Rows[i][16].ToString();
                string zncode = string.Format("09{0}1{1}", materiel, receipt);
                int finished = 0;
                DateTime currTime = DateTime.Now;
                string update_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string selCmd = string.Format("SELECT number FROM awms_source_dhl WHERE carton='{0}';", carton);
                Console.WriteLine(selCmd);
                MySqlCommand selSqlCmd = new MySqlCommand(selCmd, sqlConn);
                MySqlDataReader selReader = null;
                selReader = selSqlCmd.ExecuteReader();

                if (selReader.Read())
                {
                    if (i == 0)
                    {//重复导入相同的Excel文件，2次或多次导入文件
                        return ErrorCode.文件导入_文件内容已经导入过;
                    }

                    int selNumber = Convert.ToInt32(selReader["number"].ToString());
                    selReader.Close();
                    number = number + selNumber;
                    Console.WriteLine(number);

                    string updateCmd = "UPDATE awms_source_dhl SET number=" + number + ",update_time='" + update_time + "' WHERE carton='" + carton + "';";
                    Console.WriteLine(updateCmd);
                    MySqlCommand insSqlComm = new MySqlCommand(updateCmd, sqlConn);
                    int val = insSqlComm.ExecuteNonQuery();
                    Console.WriteLine(val);
                }
                else
                {
                    selReader.Close();
                    string create_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string insertCmd = "INSERT INTO awms_source_dhl(torder, vehicle, destination, pallet, carton, materiel, number, receipt, inspection, zncode, finished, create_time, update_time) VALUES ('" +
                        tOrder + "', '" + vehicle + "', '" + destination + "', '" + pallet + "', '" +
                        carton + "', '" + materiel + "', " + number + ", '" + receipt + "', '" +
                        inspection + "', '" + zncode + "', " + finished + ", '" + create_time + "', '" + update_time + "');";
                    Console.WriteLine(insertCmd);
                    MySqlCommand insSqlComm = new MySqlCommand(insertCmd, sqlConn);
                    int val = insSqlComm.ExecuteNonQuery();
                    Console.WriteLine(val);

                }

                if (processHandler != null)
                {
                    processHandler.Invoke(null, (i + 1) * 100 / maxCount);
                }

            }
            sqlConn.Close();

            return ErrorCode.成功;
        }
        /// <summary>
        /// 栈板是否整托，整托判定逻辑：
        /// - 栈板号内只存在唯一09码物料，则为整托
        /// - 否则，为散托
        /// </summary>
        /// <param name="palletId">栈板编号</param>
        /// <returns></returns>
        public static bool? IsPalletSingle(string palletId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(DISTINCT(zncode)) FROM awms_source_dhl WHERE pallet='" + palletId + "';", conn);
            MySqlDataReader reader = null;
            int num = 0;
            try
            {
                conn.Open();
                Console.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    num = reader.GetInt32(0);
                    reader.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                conn.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (reader == null)
                {
                    cmd.Dispose();
                    conn.Close();
                }
            }

            if (num != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 是否检验
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public static bool? IsPalletNeedQuantityCheck(string palletId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT(inspection) FROM awms_source_dhl WHERE pallet='" + palletId + "';", conn);
            MySqlDataReader reader = null;
            string inspection = "Y";
            try
            {
                conn.Open();
                Console.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    inspection = reader["inspection"].ToString();
                    Console.WriteLine(inspection);
                    reader.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                conn.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (reader == null)
                {
                    cmd.Dispose();
                    conn.Close();
                }
            }

            if (inspection != "Y")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static int GetPalletDistinct09Num(string palletId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(DISTINCT(zncode)) FROM awms_source_dhl WHERE pallet='" + palletId + "';", conn);
            MySqlDataReader reader = null;
            int num = 0;
            try
            {
                conn.Open();
                Console.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    num = reader.GetInt32(0);
                    reader.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                conn.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (reader == null)
                {
                    cmd.Dispose();
                    conn.Close();
                }
            }

            return num;
        }
        /// <summary>
        /// 检查发货单号是否存在
        /// </summary>
        /// <param name="order">发货单号</param>
        /// <returns></returns>
        public static bool CheckIsTrafficOrderExistInAwms(string order)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM awms_source_dhl WHERE torder='" + order + "';", conn);
            MySqlDataReader reader = null;
            int num = 0;
            try
            {
                conn.Open();
                Console.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    num = reader.GetInt32(0);
                    reader.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                conn.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (reader == null)
                {
                    cmd.Dispose();
                    conn.Close();
                }
            }

            if (num == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 添加发货单号到发货单任务表
        /// - 发货单任务表中存在
        ///     - 如果发货单状态为已完成/进行中，则不处理
        /// - 发货单任务表中不存在则添加，并标记发货单状态为正在进行中
        /// </summary>
        /// <param name="orders">发车单号列表</param>
        public static void AddTrafficOrderToTaskTable(Collection<string> orders)
        {
            MySqlConnection sqlConn = new MySqlConnection(Config.SqlServerConn);
            sqlConn.Open();
            for (int i = 0; i < orders.Count; i++)
            {
                string torder = orders[i];
                Console.WriteLine(torder);
                string insertCmd = "INSERT INTO awms_orders_tasks_dhl(torder, `status`) VALUES ('" + torder + "'," + 0 + ");";
                Console.WriteLine(insertCmd);
                MySqlCommand insSqlComm = new MySqlCommand(insertCmd, sqlConn);
                int val = insSqlComm.ExecuteNonQuery();
                Console.WriteLine(val);
            }
            sqlConn.Close();
        }
        /// <summary>
        /// 获取整个数据表（表名）中的所有字段数据，并封装成DataTable/DataSet
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllFromTableByName(string tableName) { return new DataTable(); }

        /// <summary>
        /// 获取设备状态（设备状态表）
        /// </summary>
        /// <param name="dev">设备类型</param>
        /// <returns></returns>
        public static object GetDeviceState(DeviceType dev) { return new { IP = "192.168.1.100",desc="desc" }; }

        /// <summary>
        /// 通过箱号获取09码
        /// </summary>
        /// <param name="boxId"></param>
        /// <returns></returns>
        public static string GetC09ByBoxId(string boxId) { return "C0901991"; }
        /// <summary>
        /// 创建订单号
        /// - 如果箱号在当前任务总表中不存在，则返回空
        /// - 如果箱号在当前任务总表中存在
        ///     - 如果箱号对应的实时任务订单已完成（重复扫描相同的箱号），返回已存在的箱号
        ///     - 其他，创建并返回订单号
        /// </summary>
        /// <param name="box">箱号</param>
        /// <returns></returns>
        public static string CreateOrderIdByBoxId(string box) { return "12"; }
        /// <summary>
        /// 检查订单状态是否已完成（防止重复扫描相同的箱号）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool OrderIsFinished(string orderId) { return false; }

        /// <summary>
        /// 获取当前订单对应的通道是否是正常通道
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <returns></returns>
        public static bool GetCurrentOrderChannel(string orderId) { return true; }

        /// <summary>
        /// 通过OrderId获取订单绑定的灯编号
        /// 内部必须是原子性获取
        /// 1. 之前已经绑定过，直接返回灯ID
        /// 2. 从未绑定过
        ///    - 获取可用的灯编号
        ///    - 将可用的灯编号写入订单
        ///    - 返回灯编号
        ///    - 如果一个09码只有个箱号，全部固定的某个灯（业务保留）
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns>灯编号</returns>
        public static int? GetBindedLightByOrder(string orderId) { return 123; }
        /// <summary>
        /// 设置当前灯为占用状态（inuse为1）
        /// </summary>
        /// <param name="lightId"></param>
        public static void SetLightOccupied(int lightId) { }
        /// <summary>
        /// 获取灯的颜色
        /// </summary>
        /// <param name="lightId"></param>
        /// <returns></returns>
        public static LightColor GetLightColor(int lightId) { return LightColor.RED; }
   
        /// <summary>
        /// 设置灯状态（亮、灭、闪烁）（数据库中灯表 中添加状态字段）
        /// - 灯的颜色由灯ID已经决定了
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="onOff"></param>
        public static void SetLightState(int lightId, LightOnOffState onOff) { }
        /// <summary>
        /// 设置灯状态
        /// </summary>
        /// <param name="on">点亮</param>
        /// <param name="color">颜色（红色、绿色）</param>
        public static void SetLightState(int lightId, LightState state) { }
        /// <summary>
        /// 订单完成
        /// </summary>
        /// <param name="orderId"></param>
        public static void FinishOrderById(string orderId) { }
        /// <summary>
        /// 根据灯号检查当前物料是否集齐
        /// - 灯对应的09码对应的箱对应的订单全部完成
        /// </summary>
        /// <param name="lightId"></param>
        /// <returns></returns>
        public static bool CheckIsFullByLight(int lightId) { return false; }
        /// <summary>
        /// 设置订单状态（订单状态扩充添加，货物到达正常道口、货物正常口被取走
        /// </summary>
        /// <param name="orderId"></param>
        public static void SetOrderState(string orderId, int state) { }
        /// <summary>
        /// 设置订单货物实际到达的道口
        /// </summary>
        /// <param name="orderId">订单</param>
        /// <param name="channel">通道编号</param>
        public static void SetOrderRealChannel(string orderId, EnumChannel channel) { }
        /// <summary>
        /// 如果扫描的数据无法解析orders表创建一个以E为开始的订单，扫描的信息记录到箱号字段中
        /// </summary>
        public static void CreateErrorOrder(string cameraCode) { }


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
        /// <summary>
        /// 通过09码设置对应的目标库位
        /// </summary>
        /// <param name="c09"></param>
        public static void SetTargetLocationByC09(string c09) { }

        /// <summary>
        /// 通过箱号复位订单
        /// </summary>
        /// <param name="boxId"></param>
        /// <returns>复位成功、失败</returns>
        public static ErrorCode ResetOrderByBoxId(string boxId) { return ErrorCode.成功; }



        /// <summary>
        /// 人工强制结束订单（订单无法正常完成的情况下）
        /// </summary>
        /// <param name="orderId"></param>
        public static void SetForceOrderFinish(string orderId) { }
        /// <summary>
        /// 以发货单维度进行强制结束订单（）
        /// </summary>
        /// <param name="tOrder">发货单号</param>

        public static void SetForceTOrderFinish(string tOrder) { }
        /// <summary>
        /// 设置灯是否好用（是否在线，在线为可用，否则不可用）
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="good"></param>
        public static void SetLightIsGood(int lightId, bool good) { }
        /// <summary>
        /// 灯表添加灯号
        /// - 如果不存在则添加
        /// - 如果存在则更新为可用（isgood 改为可用）
        /// </summary>
        /// <param name="lightId"></param>
        public static void AddLight(int lightId) { }



        /// <summary>
        /// 所有订单是否都已经完成
        /// </summary>
        /// <returns>是/否</returns>

        public static bool IsAllOrdersFinished() { return true; }
        /// <summary>
        /// 获取数据表里特定页的所有数据
        /// </summary>
        /// <param name="curPageId">页编号</param>
        /// <param name="pageSize">一页对应的数据行数</param>
        /// <returns></returns>
        public static ObservableCollection<object> GetDataByPage(string tableName,int curPageId, int pageSize) { return new ObservableCollection<object>(); }
        public static int GetTabelRecordCount(string tableName,int pageSize=0)
        {
            if (pageSize==0)
            {//获取所有记录行数
            }
            else
            {//按照PageSize作为一页内容数量进行分页后的总页数
                
            }
            return 1000;
        }
        /// <summary>
        /// 设置灯电压状态
        /// </summary>
        /// <param name="lowVoltage">是否欠压</param>
        public static void SetLightVoltage(int lightId,bool lowVoltage) { }
        #endregion

    }



}
