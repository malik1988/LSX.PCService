using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HenkTcp;
using System.Net.Sockets;
using System.Net;
using LSX.PCService.Models;
using System.Diagnostics;
namespace LSX.PCService.Data
{

    class LightSender:Light
    {
        object sender;

        public LightSender(TcpClient client):base()
        {
            this.sender = client;
        }


        public void Send(byte[] bs)
        { 
        }
    }

    class LightManager : IDisposable
    {
        HenkTcpServer server = null;

        Dictionary<TcpClient, Light> lightClientList;

        TcpClient curClient;
        Light curLight;

        object ack = null;





        public LightManager(int port)
        {
            lightClientList = new Dictionary<TcpClient, Light>();
            server = new HenkTcpServer();
            //server.DataReceived += server_DataReceived;
            server.ClientConnected += server_ClientConnected;
            server.ClientDisconnected += server_ClientDisconnected;
            server.Start(port, 20);

        }

        void server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            if (this.lightClientList.Keys.Contains(e))
                this.lightClientList.Remove(e);
        }

        void server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            var client = e;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            IPEndPoint ipEnd = (IPEndPoint)(client.Client.RemoteEndPoint);
            string ip = ipEnd.Address.ToString();
            int port = ipEnd.Port;
            if (!this.lightClientList.Keys.Contains(e))
            {
                Light light = Db.Context.Query<Light>().Where(a => a.Ip == ip).FirstOrDefault();
                if (null == light)
                {
                    logger.Info("Light Find Failed: IP=" + ip + "数据库中不存在！");
                }
                else
                    this.lightClientList.Add(e, light);
            }
        }

        private void server_DataReceived(object sender, Message e)
        {
            var client = e.TcpClient;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            IPEndPoint ipEnd = (IPEndPoint)(client.Client.RemoteEndPoint);
            string ip = ipEnd.Address.ToString();
            int port = ipEnd.Port;

            logger.Info("Client:" + ipEnd.ToString() + " Data:" + BitConverter.ToString(e.Data, 0));



            if (e.Data[0] == 0xbb)//设备上线注册
            {
                if (!this.lightClientList.Keys.Contains(client))
                {
                    Light light = Db.Context.Query<Light>().Where(a => a.Ip == ip).FirstOrDefault();
                    if (null == light)
                    {
                        logger.Info("Light Find Failed: IP=" + ip + "数据库中不存在！");
                    }
                    else
                        this.lightClientList.Add(client, light);
                }
            }

            if (!this.lightClientList.Keys.Contains(client))
            {//未注册设备，丢弃数据
                logger.Info("Client:" + ipEnd.ToString() + " 未注册！数据不处理");
                return;
            }

            if (this.curClient == client)
            {
                this.ack = new object();
            }


        }

        public ErrorCode SetLight(int channelId, OrderRunning order)
        {
            return ErrorCode.成功;
        }
        /// <summary>
        /// 根据当前货物量分配灯
        /// 
        /// </summary>
        /// <returns></returns>
        public Light AllocateLight(int channelId, OrderRunning order)
        {
            Light lightExist = Db.Context.Query<Light>().Where(a => a.Channel_id == channelId && a.C09码 == order.C09码).FirstOrDefault();
            if (null == lightExist)
            {
                var q = Db.Context.Query<Light>().Where(a => a.Channel_id == channelId && a.Inuse != true);
                return q.FirstOrDefault();
            }
            else
                return lightExist;
        }
        public ErrorCode SetLight(Light light, bool on, OrderRunning order)
        {
            string log = "";
            bool exist = false;
            ErrorCode retCode = ErrorCode.成功;

            int lightId = light.Id;
            int orderId = order.Id;

            foreach (var lc in this.lightClientList)
            {
                Light _light = lc.Value;
                TcpClient client = lc.Key;

                if (_light.Id == lightId)
                {
                    byte[] data = new byte[1] { 0x55 };
                    if (on)
                        data[0] = 0xaa;
                    this.curClient = client;
                    this.curLight = light;

                    Message msg = this.server.WriteAndGetReply(client, data, new TimeSpan(0, 0, 2));
                    if (null != msg)
                    {
                        if (msg.Data[0] == data[0])
                        {
                            log = "设置灯" + on.ToString() + "成功";
                            retCode = ErrorCode.成功;
                        }
                    }
                    else
                    {
                        log = "设置灯" + on.ToString() + "失败" + " :按灯控制_发送灯后超时未收到应答";
                        retCode = ErrorCode.按灯控制_发送灯后超时未收到应答;
                    }

                    Db.Context.Session.BeginTransaction();
                    Db.Context.Insert<LightLog>(new LightLog()
                    {
                        Light_id = lightId,
                        Order_id = orderId,
                        Time = DateTime.Now,
                        Log = log
                    });
                    if (retCode == ErrorCode.成功)
                    {
                        light.Inuse = true;
                        light.C09码 = order.C09码;
                        Db.Context.Update<Light>(light);
                    }
                    Db.Context.Session.CommitTransaction();

                    return retCode;

                }
            }
            if (!exist)
            {
                return ErrorCode.按灯控制_无效灯ID;
            }

            return ErrorCode.成功;

        }

        public void Dispose()
        {
            this.server.Stop();
        }
    }
}
