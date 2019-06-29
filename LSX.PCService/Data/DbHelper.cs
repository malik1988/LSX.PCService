using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;


namespace LSX.PCService.Data
{

    public static class MyClipBoard
    {
        public static void SetText(string p_Text)
        {
            Thread STAThread = new Thread(
                delegate()
                {
                    Clipboard.SetText(p_Text);
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        public static string GetText()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate()
                {
                    ReturnValue = Clipboard.GetText();
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }
    }
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

    enum DeviceState
    {
        初始化 = 0,
        已连接,
        断开
    }


    /// <summary>
    /// 数据获取接口
    /// 所有函数都是原子性、线程安全型
    /// </summary>
    class DbHelper
    {
        #region 进行中的订单表



        public static void ExportExcel(string tableName, string savePath)
        {
            System.Data.DataTable dt = GetAllFromTableByName(tableName);

            StringBuilder strbu = new StringBuilder();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                strbu.Append(dt.Columns[i].ColumnName.ToString() + "\t");
            }

            strbu.Append(Environment.NewLine);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    strbu.Append(dt.Rows[i][j].ToString() + "\t");
                }
                strbu.Append(Environment.NewLine);
            }

            MyClipBoard.SetText(strbu.ToString());

            Excel.Application excelApp = new Excel.Application();
            if (excelApp == null)
                return;

            excelApp.Visible = false;
            Excel.Workbooks workbooks = excelApp.Workbooks;
            Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            Excel.Worksheet worksheet = workbook.Worksheets.Add();

            worksheet.Paste();


            FileStream file = new FileStream(savePath, FileMode.CreateNew);
            file.Close();
            file.Dispose();
            workbook.Saved = true;
            workbook.SaveCopyAs(savePath);
        }

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

            System.Diagnostics.Debug.WriteLine(fileType);

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
            System.Diagnostics.Debug.WriteLine(sheetName);
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
                System.Diagnostics.Debug.WriteLine(selCmd);
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
                    System.Diagnostics.Debug.WriteLine(number);

                    string updateCmd = "UPDATE awms_source_dhl SET number=" + number + ",update_time='" + update_time + "' WHERE carton='" + carton + "';";
                    System.Diagnostics.Debug.WriteLine(updateCmd);
                    MySqlCommand insSqlComm = new MySqlCommand(updateCmd, sqlConn);
                    int val = insSqlComm.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine(val);
                }
                else
                {
                    selReader.Close();
                    string create_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string insertCmd = "INSERT INTO awms_source_dhl(torder, vehicle, destination, pallet, carton, materiel, number, receipt, inspection, zncode, finished, create_time, update_time) VALUES ('" +
                        tOrder + "', '" + vehicle + "', '" + destination + "', '" + pallet + "', '" +
                        carton + "', '" + materiel + "', " + number + ", '" + receipt + "', '" +
                        inspection + "', '" + zncode + "', " + finished + ", '" + create_time + "', '" + update_time + "');";
                    System.Diagnostics.Debug.WriteLine(insertCmd);
                    MySqlCommand insSqlComm = new MySqlCommand(insertCmd, sqlConn);
                    int val = insSqlComm.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine(val);

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
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
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
            MySqlCommand cmd = new MySqlCommand("SELECT inspection FROM awms_source_dhl WHERE pallet='" + palletId + "';", conn);
            MySqlDataReader reader = null;
            string inspection = "Y";
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    inspection = reader["inspection"].ToString();
                    System.Diagnostics.Debug.WriteLine(inspection);
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
        /// <summary>
        /// 从原始数据表+发货单表 中查找栈板号对应的09码数量
        /// - 如果栈板号不存在，返回0
        /// - 否则返回栈板号对应的09码个数
        ///  
        /// </summary>
        /// <param name="palletId">栈板号</param>
        /// <returns></returns>
        public static int GetPalletDistinct09Num(string palletId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(DISTINCT(zncode)) FROM awms_source_dhl WHERE pallet='" + palletId + "';", conn);
            MySqlDataReader reader = null;
            int num = 0;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
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
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
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
                System.Diagnostics.Debug.WriteLine(torder);
                string insertCmd = "INSERT INTO awms_orders_tasks_dhl(torder, `status`) VALUES ('" + torder + "'," + 0 + ");";
                MySqlCommand insSqlComm = new MySqlCommand(insertCmd, sqlConn);
                try
                {
                    int val = insSqlComm.ExecuteNonQuery();
                    if (val == 1)
                    {
                        System.Diagnostics.Debug.WriteLine("Insert data successfully");
                    }

                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine("Insert data failed, maybe data already exists.");
                }
            }
            sqlConn.Close();
        }
        /// <summary>
        /// 获取整个数据表（表名）中的所有字段数据，并封装成DataTable
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllFromTableByName(string tableName)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            conn.Open();

            MySqlCommand dataCmd = new MySqlCommand("select * from " + tableName + "; ", conn);
            System.Diagnostics.Debug.WriteLine("select * from " + tableName + "; ");
            MySqlDataReader dataReader = null;
            DataTable dataTable = new DataTable();

            dataReader = dataCmd.ExecuteReader();

            int colNum = dataReader.FieldCount;

            for (int col = 0; col < colNum; col++)
            {
                dataTable.Columns.Add(dataReader.GetName(col), dataReader.GetFieldType(col));
            }

            dataTable.BeginLoadData();
            object[] values = new object[colNum];

            while (dataReader.Read())
            {
                dataReader.GetValues(values);
                dataTable.LoadDataRow(values, true);
            }
            dataReader.Close();
            dataTable.EndLoadData();
            return dataTable;
        }
        /// <summary>
        /// 按页获取表中数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="pageId">当前页数</param>
        /// <param name="pageSize">每页显示的行数</param>
        /// <returns></returns>
        public static DataTable GetAllFromTableByName(string tableName, int pageId, int pageSize) { return new DataTable(); }
        /// <summary>
        /// 获取设备状态（设备状态表）
        /// </summary>
        /// <param name="dev">设备类型</param>
        /// <returns></returns>
        public static ObservableCollection<object> GetAllDeviceState()
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT dtype,ip,`status`,`desc`,`update_time` FROM awms_device_dhl ", conn);
            System.Diagnostics.Debug.WriteLine("SELECT `dtype,ip,status`,desc FROM awms_device_dhl ");
            MySqlDataReader reader = null;
            ObservableCollection<object> result = null;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                int colNum = reader.FieldCount;

                result = new ObservableCollection<object>();

                while (reader.Read())
                {
                    string dType = reader["dtype"].ToString();
                    string dIP = reader["ip"].ToString();
                    string dStatus = reader["status"].ToString();
                    string dDesc = reader["desc"].ToString();
                    string dUpdateTime = reader["update_time"].ToString();
                    result.Add(new { Type = dType, Ip = dIP, Status = dStatus, Description = dDesc, UpdateTime = dUpdateTime });

                }
                reader.Close();
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

            return result;

        }
        /// <summary>
        /// 获取设备状态（设备状态表）
        /// </summary>
        /// <param name="dev">设备类型</param>
        /// <returns></returns>
        public static object GetDeviceState(string dev)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT ip,`status`,`desc` FROM awms_device_dhl WHERE dtype='" + dev + "';", conn);
            System.Diagnostics.Debug.WriteLine("SELECT `status`,desc FROM awms_device_dhl WHERE dtype='" + dev + "';");
            MySqlDataReader reader = null;
            object result = null;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string dIP = reader["ip"].ToString();
                    string dStatus = reader["status"].ToString();
                    string dDesc = reader["desc"].ToString();
                    result = new { Ip = dIP, Status = dStatus, Description = dDesc };
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

            return result;

        }

        /// <summary>
        /// 通过箱号获取09码
        /// </summary>
        /// <param name="boxId"></param>
        /// <returns></returns>
        public static string GetC09ByBoxId(string boxId)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT zncode FROM awms_orders_dhl WHERE carton='" + boxId + "';", conn);

            MySqlDataReader reader = null;
            string zncode = null;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    zncode = reader["zncode"].ToString();
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

            return zncode;
        }



        /// <summary>
        /// 创建订单号
        /// - 如果箱号在当前任务总表（原始数据表+发货单号表+栈板表）中不存在
        ///     - 则创建异常订单（Exxx），并设置常订单初始 通道号为异常通道
        /// - 如果箱号在当前任务总表中存在
        ///     - 如果箱号对应的实时任务表(orders)中订单已完成（重复扫描相同的箱号），返回已存在的箱号
        ///     - 其他，创建并返回订单号（Dxxxx）
        /// </summary>
        /// <param name="box">箱号</param>
        /// <returns></returns>
        public static string CreateOrderIdByBoxId(string box)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT id FROM awms_orders_dhl WHERE carton='" + box + "';", conn);

            MySqlDataReader reader = null;
            int bID = 0;
            conn.Open();
            System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                bID = reader.GetInt16(0);
                reader.Close();
            }
            else
            {
                return null;
            }
            cmd.Dispose();
            conn.Close();

            string orderID = string.Format("D{0}{1}", DateTime.Now.ToString("yyyyMMdd"), String.Format("{0:0000000}", bID));
            return orderID;
        }
        /// <summary>
        /// 检查订单状态是否已完成（防止重复扫描相同的箱号）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool OrderIsFinished(string orderId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT `status` FROM awms_orders_tasks_dhl WHERE torder='" + orderId + "';", conn);

            MySqlDataReader reader = null;
            int oStatus = 0;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    oStatus = reader.GetInt16(0);
                    reader.Close();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Order not exists.");
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
            if (oStatus == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 获取当前订单对应的通道是否是正常通道
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <returns></returns>
        public static bool GetCurrentOrderChannel(string orderId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT channel FROM awms_orders_dhl WHERE `order`='" + orderId + "';", conn);
            MySqlDataReader reader = null;

            int channel = 0;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    channel = reader.GetInt16(0);
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

            if (channel == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

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
        public static void SetLightOccupied(int lightId)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_lights_dhl SET inuse=1 WHERE ip=" + lightId + ";", conn);
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
        /// <summary>
        /// 获取灯的颜色
        /// </summary>
        /// <param name="lightId"></param>
        /// <returns></returns>
        public static LightColor GetLightColor(int lightId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT color FROM awms_lights_dhl WHERE ip=" + lightId + ";", conn);
            MySqlDataReader reader = null;

            int color = 0;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    color = reader.GetInt16(0);
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
            System.Diagnostics.Debug.WriteLine(color);

            if (color >= 100 & color < 150)
            {
                return LightColor.RED;
            }
            else if (color >= 150 & color <= 200)
            {
                return LightColor.GREEN;
            }
            else
            {
                throw new Exception("lightId is illegal.");
            }
        }

        /// <summary>
        /// 设置灯状态（亮、灭、闪烁）（数据库中灯表 中添加状态字段）
        /// - 灯的颜色由灯ID已经决定了
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="onOff"></param>
        public static void SetLightState(int lightId, LightOnOffState onOff)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_lights_dhl SET onoff='" + onOff + "' WHERE ip=" + lightId + ";", conn);
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
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
        public static void FinishOrderById(string orderId)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_orders_tasks_dhl SET `status`=1 WHERE torder='" + orderId + "';", conn);
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
        /// <summary>
        /// 根据灯号检查当前物料是否集齐
        /// - 灯对应的09码对应的箱对应的订单全部完成
        /// </summary>
        /// <param name="lightId"></param>
        /// <returns></returns>
        public static bool CheckIsFullByLight(int lightId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            string jCmd = "SELECT DISTINCT(carton_status) FROM awms_lights_tasks_dhl AS a LEFT JOIN awms_orders_dhl AS b ON a.zncode=b.zncode WHERE a.ip=" + lightId + ";";
            MySqlCommand cmd = new MySqlCommand(jCmd, conn);
            MySqlDataReader reader = null;
            int num = 0;
            bool isFull = true;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    num = reader.GetInt32(0);
                    if (num == 0)
                    {
                        isFull = false;
                    }
                    System.Diagnostics.Debug.WriteLine(num);
                }
                reader.Close();
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

            return isFull;
        }
        /// <summary>
        /// 设置订单状态（订单状态扩充添加，货物到达正常道口、货物正常口被取走
        /// </summary>
        /// <param name="orderId"></param>
        public static void SetOrderState(string orderId, int state)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_orders_dhl SET carton_status=" + state + " WHERE `order`='" + orderId + "';", conn);
            System.Diagnostics.Debug.WriteLine("UPDATE awms_orders_dhl SET carton_status=" + state + " WHERE order='" + orderId + "';");
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
        /// <summary>
        /// 设置订单货物实际到达的道口
        /// </summary>
        /// <param name="orderId">订单</param>
        /// <param name="channel">通道编号</param>
        public static void SetOrderRealChannel(string orderId, EnumChannel channel)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_orders_dhl SET channel=" + channel + " WHERE `order`='" + orderId + "';", conn);
            System.Diagnostics.Debug.WriteLine("UPDATE awms_orders_dhl SET channel=" + channel + " WHERE order='" + orderId + "';");
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
        /// <summary>
        /// 如果扫描的数据无法解析orders表创建一个以E为开始的订单，扫描的信息记录到箱号字段中
        /// </summary>
        public static void CreateErrorOrder(string cameraCode)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM awms_orders_dhl;", conn);

            MySqlDataReader reader = null;
            int eID = 0;
            conn.Open();
            System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                eID = reader.GetInt32(0);
                reader.Close();
            }

            string orderID = string.Format("E{0}{1}", DateTime.Now.ToString("yyyyMMdd"), String.Format("{0:0000000}", eID + 1));
            System.Diagnostics.Debug.WriteLine(orderID);

            string insertCmd = "INSERT INTO awms_orders_dhl(`order`,zncode,pallet,carton,channel,total,carton_status) VALUES ('" + orderID + "','NULL','NULL','" + cameraCode + "',0,0,0);";
            MySqlCommand insSqlComm = new MySqlCommand(insertCmd, conn);
            try
            {
                int val = insSqlComm.ExecuteNonQuery();
                if (val == 1)
                {
                    System.Diagnostics.Debug.WriteLine("Insert data successfully");
                }

            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Insert data failed, maybe data already exists.");
            }

            conn.Close();
        }


        /// <summary>
        /// 绑定LPN 09码 库位
        /// </summary>
        /// <param name="lpn">LPN码</param>
        /// <param name="c09">09码</param>
        /// <param name="loc">库位</param>
        public static void BindingLpnAndC09(string lpn, string c09, string loc)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);

            string insertCmd = "INSERT INTO awms_lpn_dhl(end_storge,zncode,lpn) VALUES ('" + loc + "','" + c09 + "','" + lpn + "');";
            MySqlCommand insSqlComm = new MySqlCommand(insertCmd, conn);
            conn.Open();
            System.Diagnostics.Debug.WriteLine(insertCmd);
            try
            {
                int val = insSqlComm.ExecuteNonQuery();
                if (val == 1)
                {
                    System.Diagnostics.Debug.WriteLine("Insert data successfully");
                }

            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Insert data failed, maybe data already exists.");
            }

            conn.Close();
        }
        /// <summary>
        /// 通过09码获取目标库位
        /// - 联合订单表和目标库位表 
        /// - 查找09码匹配的目标库位
        /// </summary>
        /// <param name="c09"></param>
        /// <returns></returns>
        public static string GetTargetLocationByC09(string c09)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT(destination) FROM awms_source_dhl WHERE zncode='" + c09 + "';", conn);
            System.Diagnostics.Debug.WriteLine("SELECT DISTINCT(destination) FROM awms_source_dhl WHERE zncode='" + c09 + "';");
            MySqlDataReader reader = null;
            string destination = null;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    destination = reader["destination"].ToString();
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
            return destination;
        }
        /// <summary>
        /// 通过09码设置对应的目标库位
        /// </summary>
        /// <param name="c09"></param>
        public static void SetTargetLocationByC09(string c09, string tarLoc)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_lpn_dhl SET end_storge='" + tarLoc + "' WHERE zncode='" + c09 + "';", conn);
            System.Diagnostics.Debug.WriteLine("UPDATE awms_lpn_dhl SET end_storge=" + tarLoc + " WHERE zncode='" + c09 + "';");
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }

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
        public static void SetForceOrderFinish(string orderId)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_orders_dhl SET carton_status=1 WHERE `order`='" + orderId + "';", conn);
            System.Diagnostics.Debug.WriteLine("UPDATE awms_orders_dhl SET carton_status=1 WHERE `order`='" + orderId + "';");
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
        /// <summary>
        /// 以发货单维度进行强制结束订单（）
        /// </summary>
        /// <param name="tOrder">发货单号</param>

        public static void SetForceTOrderFinish(string tOrder)
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_orders_tasks_dhl SET `status`=1 WHERE torder='" + tOrder + "';", conn);
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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
        }
        /// <summary>
        /// 设置灯是否好用（是否在线，在线为可用，否则不可用）
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="good"></param>
        public static void SetLightIsGood(int lightId, bool good)
        {
            int isGood = 0;
            if (good) { isGood = 1; }

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("UPDATE awms_lights_dhl SET isgood=" + isGood + " WHERE ip=" + lightId + ";", conn);
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
                int val = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine(val);
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

        }
        public static LightColor LightColorPaser(int lightId)
        {
            if (lightId >= 100 & lightId < 150)
            {
                return LightColor.RED;
            }
            else if (lightId >= 150 & lightId <= 200)
            {
                return LightColor.GREEN;
            }
            else
            {
                throw new Exception(string.Format("lightId is {0} Invalid,Id must in [100,200]", lightId));
            }
        }
        /// <summary>
        /// 灯表添加灯号
        /// - 如果不存在则添加
        /// - 如果存在则更新为可用（isgood 改为可用）
        /// </summary>
        /// <param name="lightId"></param>
        public static void AddLight(int lightId)
        {

            LightColor color = LightColorPaser(lightId);


            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);

            string insertCmd = "INSERT INTO awms_lights_dhl(ip,color) VALUES (" + lightId + ",'" + color.ToString("D") + "');";
            MySqlCommand insSqlComm = new MySqlCommand(insertCmd, conn);
            conn.Open();
            System.Diagnostics.Debug.WriteLine(insertCmd);
            try
            {
                int val = insSqlComm.ExecuteNonQuery();
                if (val == 1)
                {
                    System.Diagnostics.Debug.WriteLine("Insert data successfully");
                }

            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Insert data failed, maybe data already exists.");
            }

            conn.Close();

        }


        /// <summary>
        /// 所有订单是否都已经完成
        /// </summary>
        /// <returns>是/否</returns>

        public static bool IsAllOrdersFinished()
        {

            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(`status`) FROM awms_orders_tasks_dhl;", conn);
            MySqlDataReader reader = null;
            int num = 2;
            try
            {
                conn.Open();
                System.Diagnostics.Debug.WriteLine("Connect Database successfully.");
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

        #endregion


        internal static void SetLightVoltage(int p, bool value)
        {
            throw new NotImplementedException();
        }

        internal static int GetTabelRecordCount(string tableName)
        {
            return 1000;
        }

        public static ObservableCollection<object> GetDataByPage(string tableName, int curPageId, int pageSize) { return new ObservableCollection<object>(); }
        /// <summary>
        /// 设置设备状态中的IP和Statu信息
        /// - 其他信息字段初始配置好，之后固定不变
        /// - 每种类型只有一个主设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <param name="ip">IP地址</param>
        /// <param name="status">状态</param>
        public static void SetDeviceState(DeviceType type, string ip, string status = null)
        {
            MySqlConnection conn = new MySqlConnection(Config.SqlServerConn);
            string sql = "";
            if (status != null)
                sql = "UPDATE awms_device_dhl SET ip='" + ip + "',status='" + status + "' WHERE dtype='" + type.ToString() + "';";
            else
                sql = "UPDATE awms_device_dhl SET ip='" + ip + "' WHERE dtype='" + type.ToString() + "';";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = null;

            try
            {
                conn.Open();
                int val = cmd.ExecuteNonQuery();
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

        }



        /// <summary>
        /// 添加栈板号到栈板表中
        /// </summary>
        /// <param name="PalletId"></param>
        /// <returns></returns>
        internal static ErrorCode AddPalletToPalletTable(string PalletId)
        {
            return ErrorCode.成功;
        }
        /// <summary>
        /// 获取所有灯的状态信息
        ///
        /// </summary>
        /// <returns>返回object对象列表包括以下内容：
        ///  - Id 灯编号
        ///  - State 灯状态（LightState类型，On/off 及颜色）
        ///  - Voltage 欠压状态(电压正常，欠压)
        ///  - IsGood 是否正常
        ///  - Description 其他描述（是否占用）
        /// </returns>
        internal static ObservableCollection<object> GetAllLightStates()
        {
            return new ObservableCollection<object>(){
                new { Id = 1, State = LightState.ON_GREEN,Voltage="电压正常",IsGood=true, Description = "good" }
            };
        }

        /// <summary>
        /// 根据通道获取订单列表
        /// </summary>
        /// <param name="enumChannel">通道值</param>
        /// <returns>返回object对象列表，object中包含以下内容：
        /// -Order 订单编号
        /// -Carton 箱号
        /// -ZnCode 09码
        /// -CurCount 当前箱数
        /// -TotalCount 总箱数
        /// -LightId  关联的灯编号
        /// -LightColor 关联的灯颜色 
        /// -Status 订单状态
        /// </returns>
        internal static ObservableCollection<object> GetOrderListByChannel(EnumChannel enumChannel)
        {
            return new ObservableCollection<object>()
            {
                new {Order="111",Carton="carton",ZnCode="zncode",CurCount=1,TotalCount=12,LightId=101,LightColor=LightColor.RED,Status=OrderState.货物到达道口}
            };

        }
    }

}
