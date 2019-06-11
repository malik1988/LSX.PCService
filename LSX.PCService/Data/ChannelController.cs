using System;

using LSX.PCService.Models;
using HenkTcp;
using System.Net.Sockets;
namespace LSX.PCService.Data
{
    class ChannelController : SingletonBase<ChannelController>, IDisposable
    {
        HenkTcpServer server = null;
        TcpClient client = null;

        OrderRunning order;

        private ChannelController()
        {
            server = new HenkTcpServer();
            server.ClientConnected += server_ClientConnected;
            server.ClientDisconnected += server_ClientDisconnected;
            server.DataReceived += server_DataReceived;
            server.Start(Config.ChannelServerPort, 1);
        }

        void server_ClientDisconnected(object sender, TcpClient e)
        {
            this.client = null;
        }

        void server_ClientConnected(object sender, TcpClient e)
        {
            this.client = e;
        }

        private void server_DataReceived(object sender, HenkTcp.Message e)
        {
            string str = e.MessageString;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("server_DataReceived:" + str);

            if (null == str) return;

            //TODO 
            //接收到货物到达通道消息
            BoxInChannelInputQueue boxQueue = new BoxInChannelInputQueue();
            boxQueue.Send(new BoxInChannelMessage() { channelId = 1 });
        }

        public void SendToChannel(int channelId)
        {
            this.server.Write(this.client, new byte[] { (byte)channelId });
        }



        Channel GetChannelByCaseNum(string caseNum)
        {
            var q = Db.Context.Query<AwmsRawData>().InnerJoin<OrderRawAnalyzed>((a, o) => a.Id == o.Raw_id).LeftJoin<Channel>((a, o, c) => o.Channel_id == c.Id);
            return q.Where((a, o, c) => a.箱号 == caseNum).Select<Channel>((a, o, c) => c).FirstOrDefault();
        }

        /// <summary>
        /// 箱号和数量判定去哪个通道
        /// </summary>
        /// <param name="caseNum">箱号</param>
        /// <param name="num">数量</param>
        public void Begin(string caseNum)
        {
            string errlog = "";

            //获取通道

            Channel ch = GetChannelByCaseNum(caseNum);
            //当前订单数量已满
            //bool full = Db.Context.SqlQuery<bool>(string.Format("SELECT  count(*)>0 from view_count_finished WHERE view_count_finished.`箱号`='{0}'", caseNum)).FirstOrDefault();

            bool full = false;

            if (null == ch || full)//已经满了也设置为异常通道
            {//没有空闲的通道，将货物送到异常通道


                if (full)
                    errlog = "物料数量已满！";
                else
                    errlog = "无效箱号";
                //发送异常通道消息

                ch = new Channel()
                {
                    Id = (int)EnumChannel.异常道口,
                    Name = EnumChannel.异常道口.ToString(),
                    Special = true
                };
            }
            //检查通道是否被占用，如果是则 



            //发送 ch对应的通道消息
            SendToChannel(ch.Id);
            
            this.order.Final_channel = ch.Id;
           

            AddLogToDb("Set Channel " + ch.Name + errlog);

            InputJob channelInput = new InputJob();
            channelInput.Send(new InputMessage()
            {
                cmd = MessageCmd.Add,
                type = InputType.ChannelController,
                channelId = ch.Id
            });


        }

        public OrderRunning CreateOrder(string caseNum)
        {
            AwmsRawData raw = Db.Context.Query<AwmsRawData>().Where(a => a.箱号 == caseNum).FirstOrDefault();
            if (null == raw)
                return null;


            OrderRunning run = Db.Context.Insert<OrderRunning>(new OrderRunning()
            {
                Raw_id = raw.Id,
                Update_time = DateTime.Now,
                //State_id = (int)EnumOrderStatus.创建订单
            });
            AddLogToDb("CreateOrder");
            this.order = run;
            return run;
        }

        void AddLogToDb(string logStr)
        {
            if (null == this.order) return;
            Db.Context.Insert<OrderLog>(new OrderLog()
            {
                Order_id = this.order.Id,
                Log = logStr,
                Time = DateTime.Now
            });
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (this.server.IsRunning)
                this.server.Stop();
        }

        #endregion
    }
}
