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


    class LightManager : SingletonBase<LightManager>, IDisposable
    {
        HenkTcpServer server = null;

        List<LightSender> lightSenders;

        int lastLightId = 0;
        int lastOrderId;
        string lastC09;

        private LightManager()
        {
            lightSenders = new List<LightSender>();
            server = new HenkTcpServer();
            server.ClientConnected += server_ClientConnected;
            server.ClientDisconnected += server_ClientDisconnected;
            server.DataReceived += server_DataReceived;
            server.Start(Config.LightServerPort, 20);

        }


        void server_DataReceived(object sender, Message e)
        {
            //获取灯ID，结束灯ID对应的订单
        }

        void server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            this.lightSenders.Remove(new LightSender(e));
        }

        void server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            var client = e;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            IPEndPoint ipEnd = (IPEndPoint)(client.Client.RemoteEndPoint);
            string ip = ipEnd.Address.ToString();
            int port = ipEnd.Port;

            logger.Info("Light Connected  IP=" + ip);

            if (!this.lightSenders.Contains(new LightSender(e)))
            {
                Light light = Db.Context.Query<Light>().Where(a => a.Ip == ip).FirstOrDefault();
                if (null == light)
                {
                    logger.Info("Light Find Failed: IP=" + ip + "数据库中不存在！");
                }
                else
                {
                    this.lightSenders.Add(new LightSender(e, light));
                }
            }
        }


        public ErrorCode SetLight(int lightId)
        {
            return ErrorCode.成功;
        }

        public void WaitLightOff(int lightId)
        {
            //阻塞等待制定ID的灯灭
        }
    

        public ErrorCode AutoSetLight(int channelId, int orderId)
        {
            //1. 获取订单ID对应的
            return ErrorCode.成功;
        }

        public ErrorCode AutoSetLight(int channelId, int orderId, string c09)
        {
            ErrorCode retCode = ErrorCode.成功;
            int? lightId = AllocateLight(channelId, c09);
            if (lightId == null)
            {
                retCode = ErrorCode.按灯控制_无可用灯;
            }
            else
            {
                if (lastLightId != 0 && lastOrderId != 0 && lastC09 != null)
                {
                    retCode = SetLight(lastLightId, false, lastOrderId, lastC09);
                }

                retCode = SetLight((int)lightId, true, orderId, c09);
                lastLightId = (int)lightId;
                lastC09 = c09;
                lastOrderId = orderId;
            }
            return ErrorCode.成功;
        }
        /// <summary>
        /// 根据当前货物量分配灯
        /// 
        /// </summary>
        /// <returns></returns>
        public int? AllocateLight(int channelId, string c09)
        {
            int? lightId = Db.Context.Query<OrderRunning>().InnerJoin<OrderRawAnalyzed>((o, r) => o.Raw_id == r.Raw_id).Where((o, r) => r.C09码 == c09).Select((o, r) => o.Light_id).FirstOrDefault();
            if (null == lightId)
            {
                var q = Db.Context.Query<Light>().Where(a => a.Channel_id == channelId && a.Inuse != true);
                return q.Select(a => a.Id).FirstOrDefault();
            }
            else
                return lightId;
        }
        public ErrorCode SetLight(int lightId, bool on, int orderId, string c09)
        {
            bool exist = false;
            Light light = null;
            string err = "";
            ErrorCode retCode = ErrorCode.成功;

            foreach (var lc in this.lightSenders)
            {
                if (lc.Id == lightId)
                {
                    light = lc;
                    if (on)
                    {
                        retCode = lc.On(this.server);

                    }
                    else
                        retCode = lc.Off(this.server);
                    exist = true;
                    break;
                }
            }
            if (!exist)
            {
                retCode = ErrorCode.按灯控制_无效灯ID;
            }


            err = "设置灯" + on.ToString() + ":" + retCode;
            Db.Context.Session.BeginTransaction();
            Db.Context.Insert<LightLog>(new LightLog()
            {
                Light_id = lightId,
                Order_id = orderId,
                Time = DateTime.Now,
                Log = err
            });
            if (light != null && retCode == ErrorCode.成功)
            {
                light.Inuse = true;
                light.C09码 = c09;
                Db.Context.Update<Light>(light);
            }
            Db.Context.Session.CommitTransaction();
            return retCode;

        }

        public void Dispose()
        {
            this.server.Stop();
        }
    }
}
