using System;
using Cave.Net;
using LSX.PCService.Data;
using System.Net;
namespace LSX.PCService.Controllers
{
    class ChannelCount
    {
        public int Hardware;
        public int Software;
    }
    /// <summary>
    /// 通道控制器
    /// </summary>
    class ChannelController : SingletonBase<ChannelController>, IDisposable
    {
        TcpServer server;
        CustomTcpClient client;

        static object locker = new object();
        OrderMessageInputQueue orderOkQueue;
        OrderMessageInputQueue orderErrQueue;
        BoxInChannelInputQueue boxQueue;
        OrderMessageArrivedInputQueue orderArrivedOkQueue;
        OrderMessageArrivedInputQueue orderArrivedErrQueue;

        public ChannelCount CountOkArrived { get; private set; }
        public ChannelCount CountOkTake { get; private set; }
        public ChannelCount CountErr { get; private set; }


        public EventHandler OnOkUpdate;
        public EventHandler OnErrUpdate;
        public EventHandler<string> OnEmergency;


        private string _Ip;

        public string Ip
        {
            get { return _Ip; }
            set
            {
                _Ip = value;
                DbHelper.SetDeviceState(DeviceType.CHANNEL, value);
            }
        }

        private string _Status;

        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                DbHelper.SetDeviceState(DeviceType.CHANNEL, Ip, value);
            }
        }
        private ChannelController()
        {
            server = new TcpServer() { AcceptBacklog = 1, AcceptThreads = 1 };
            
            server.ClientAccepted += server_ClientAccepted;
            server.ClientException += server_ClientException;
            server.Listen(Config.ChannelServerPort);

            Ip = "-";
            Status = DeviceState.初始化.ToString();

            NLog.LogManager.GetCurrentClassLogger().Info("Sever Start At " + Config.ChannelServerPort.ToString());
            orderOkQueue = new OrderMessageInputQueue(EnumChannel.正常道口);
            orderErrQueue = new OrderMessageInputQueue(EnumChannel.异常道口);
            boxQueue = new BoxInChannelInputQueue();
            orderArrivedOkQueue = new OrderMessageArrivedInputQueue();
            CountOkArrived = new ChannelCount();
            CountOkTake = new ChannelCount();
            CountErr = new ChannelCount();
        }

        void server_ClientException(object sender, TcpServerClientExceptionEventArgs<TcpAsyncClient> e)
        {
            this.client.Dispose();
            Status = DeviceState.断开.ToString();
        }

        void server_ClientAccepted(object sender, TcpServerClientEventArgs<TcpAsyncClient> e)
        {

            this.client = new CustomTcpClient(e.Client);
            this.client.OnReceived += client_Received;
            e.Client.Disconnected += Client_Disconnected;
            Ip = ((IPEndPoint)(e.Client.RemoteEndPoint)).Address.MapToIPv4().ToString();
            Status = DeviceState.已连接.ToString();      
            NLog.LogManager.GetCurrentClassLogger().Info("Channel Client Connected:" + e.Client.RemoteEndPoint.ToString());
        }

        void Client_Disconnected(object sender, EventArgs e)
        {
            Status = DeviceState.断开.ToString();
        }



        void client_Received(object sender, string e)
        {
            int count = 0;
            //NLog.LogManager.GetCurrentClassLogger().Info("Recv:" + e);

            if (e.StartsWith("AT+OKCHA_") && e.Length >= "AT+OKCHA_".Length + 5)
            {//正常通道货物到达
                string tmp = e.Substring("AT+OKCHA_".Length, 5);
                count = int.Parse(tmp);
                client.Write("OK");
                //if (OnCountOk!=null)
                //{
                //    OnCountOk.BeginInvoke(null, CountOk, null, null);
                //}


                //获取订单号
                var orderMsg = orderOkQueue.Receive(100);
                if (null != orderMsg)
                {
                    DbHelper.SetOrderState(orderMsg.orderId, (int)OrderState.货物到达道口);
                    orderArrivedOkQueue.Send(orderMsg);
                    CountOkArrived.Software++;
                    CountOkArrived.Hardware = count;
                    if (OnOkUpdate != null)
                    {
                        OnOkUpdate.BeginInvoke(null, null, null, null);
                    }
                }
            }

            else if (e.StartsWith("AT+TAKE_") && e.Length >= "AT+TAKE_".Length + 5)
            {//正常通道，货物被取走

                string tmp = e.Substring("AT+TAKE_".Length, 5);
                count = int.Parse(tmp);
                client.Write("OK");


                //获取订单号
                var orderMsg = orderArrivedOkQueue.Receive(100);
                if (null != orderMsg)
                {
                    DbHelper.SetOrderState(orderMsg.orderId, (int)OrderState.货物被取走);
                    DbHelper.SetOrderRealChannel(orderMsg.orderId, EnumChannel.正常道口);
                    boxQueue.Send(new BoxInChannelMessage() { orderId = orderMsg.orderId, channelId = (int)EnumChannel.正常道口 });
                    CountOkTake.Software++;
                    CountOkTake.Hardware = count;
                    if (OnOkUpdate != null)
                    {
                        OnOkUpdate.BeginInvoke(null, null, null, null);
                    }
                }
            }
            else if (e.StartsWith("AT+ERCHA_") && e.Length >= "AT+ERCHA_".Length + 5)
            {//异常通道货物到达

                string tmp = e.Substring("AT+ERCHA_".Length, 5);
                count = int.Parse(tmp);
                client.Write("OK");



                //获取订单号
                var orderMsg = orderErrQueue.Receive(100);
                if (null != orderMsg)
                {
                    DbHelper.SetOrderState(orderMsg.orderId, (int)OrderState.货物被取走);
                    DbHelper.SetOrderRealChannel(orderMsg.orderId, EnumChannel.异常道口);

                    boxQueue.Send(new BoxInChannelMessage() { orderId = orderMsg.orderId, channelId = (int)EnumChannel.异常道口 });
                    CountErr.Software++;
                    CountErr.Hardware = count;
                    if (OnErrUpdate != null)
                    {
                        OnErrUpdate.BeginInvoke(null, null, null, null);
                    }
                }
            }
            else if (e.StartsWith("AT+FULLOKCHA"))
            {//正常通道，货物堆满
                client.Write("OK");
                if (OnEmergency != null)
                {
                    OnEmergency.BeginInvoke(null, "正常通道，货物堆满", null, null);
                }

            }
            else if (e.StartsWith("AT+FULLERCHA"))
            {//异常通道，货物堆满
                client.Write("OK");
                if (OnEmergency != null)
                {
                    OnEmergency.BeginInvoke(null, "异常通道，货物堆满", null, null);
                }
            }
            else if (e.StartsWith("AT+START"))
            {//流水线启动
                client.Write("OK");
                if (OnEmergency != null)
                {
                    OnEmergency.BeginInvoke(null, "流水线启动", null, null);
                }
            }
            else if (e.StartsWith("AT+STOP"))
            {//流水线停止
                client.Write("OK");
                if (OnEmergency != null)
                {
                    OnEmergency.BeginInvoke(null, "流水线停止", null, null);
                }
            }
        }

        /// <summary>
        /// 清除所有道口计数器（正常+异常）
        /// </summary>
        public void ResetCount()
        {
            if (null==client)
            {
                //设备未连接
                return;
            }
            CountOkArrived = new ChannelCount();
            CountOkTake = new ChannelCount();
            CountErr = new ChannelCount();
            
            client.WriteAndCheckResponse("AT+CLEARALL");
        }
        public void ResetCount(EnumChannel channel)
        {
            switch (channel)
            {
                case EnumChannel.正常道口:
                    CountOkArrived = new ChannelCount();
                    CountOkTake = new ChannelCount();
                    client.WriteAndCheckResponse("AT+CLEAROKCHA");
                    break;
                case EnumChannel.异常道口:
                    CountErr = new ChannelCount();
                    client.WriteAndCheckResponse("AT+CLEARERCHA");
                    break;
            }
        }



        public ErrorCode SendToChannel(EnumChannel channelId)
        {
            if (this.client == null)
            {
                return ErrorCode.通道控制_设备离线;
            }
            int retry = 3;
            while (!Send(channelId, 2000))
            {
                retry--;
                if (retry == 0)
                {
                    //3次重发超时异常
                    NLog.LogManager.GetCurrentClassLogger().Error(string.Format("Channel({0}) Retry Send 3 Times Failed", channelId));
                    return ErrorCode.通道控制_重试3次后发送失败或超时未收到应答;
                }
            }
            return ErrorCode.成功;
        }


        bool Send(EnumChannel channel, int timeout)
        {
            string cmd = "";
            switch (channel)
            {
                case EnumChannel.正常道口:
                    cmd = "AT+TOOKCHA";
                    OnOkUpdate.BeginInvoke(null, null, null, null);
                    break;
                case EnumChannel.异常道口:
                    cmd = "AT+TOOKCHA";
                    OnErrUpdate.BeginInvoke(null, null, null, null);
                    break;
                default:
                    cmd = "AT+TOOKCHA";
                    break;
            }
            return client.WriteAndCheckResponse(cmd);
        }





        #region IDisposable 成员

        public void Dispose()
        {
            client.Dispose();
            server.Dispose();
        }

        #endregion
    }
}
