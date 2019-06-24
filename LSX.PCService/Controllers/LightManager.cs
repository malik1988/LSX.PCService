using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;
using System.Net;
using Cave.Net;
using LSX.PCService.Data;

namespace LSX.PCService.Controllers
{


    class LightManager : SingletonBase<LightManager>, IDisposable
    {
        TcpServer server;
        Dictionary<int, LightController> lightSenders;

        NLog.Logger logger;


        /// <summary>
        /// 异常消息回调
        /// </summary>
        public EventHandler<string> OnError;
       

        private LightManager()
        {
            lightSenders = new Dictionary<int, LightController>();
            server = new TcpServer()
            {
                AcceptThreads = 1,
                AcceptBacklog = 1
            };

            server.ClientAccepted += server_ClientAccepted;
           
            server.Listen(Config.LightServerPort);

            logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Sever Start At " + Config.LightServerPort.ToString());
        }


        private void server_ClientAccepted(object sender, TcpServerClientEventArgs<TcpAsyncClient> e)
        {
            IPEndPoint ipEnd = (IPEndPoint)(e.Client.RemoteEndPoint);
            string ip = ipEnd.Address.ToString();
            int port = ipEnd.Port;
            int lightId = GetLightIdFromIpAddr(ip);

            logger.Info("Light Connected  IP=" + ip + " LightId: " + lightId.ToString());
            DbHelper.AddLight(lightId);
            lightSenders.Add(lightId, new LightController(new CustomTcpClient(e.Client)));
            e.Client.Disconnected += Client_Disconnected;
        }

        void Client_Disconnected(object sender, EventArgs e)
        {
            TcpAsyncClient client = sender as TcpAsyncClient;

            IPEndPoint ipEnd = (IPEndPoint)(client.RemoteEndPoint);
            string ip = ipEnd.Address.ToString();
            int lightId = GetLightIdFromIpAddr(ip);
            //设置灯为不可用状态
            DbHelper.SetLightIsGood(lightId, false);
            lightSenders.Remove(lightId);
        }


      

      

        int GetLightIdFromIpAddr(string ip)
        {
            string[] tmp = ip.Split(new char[] { '.' });
            return int.Parse(tmp[3]);
        }
        /// <summary>
        /// 点亮
        /// </summary>
        /// <param name="lightId"></param>
        /// <param name="flash">闪烁</param>
        /// <returns></returns>
        public ErrorCode SetLight(int lightId, LightOnOffState onOff, LightColor color)
        {
            if (!lightSenders.ContainsKey(lightId))
            {
                if (OnError != null)
                {
                    OnError.BeginInvoke(null, "无效灯ID", null, null);
                }
                return ErrorCode.按灯控制_无效灯ID;
            }



            LightState cmd=LightState.OFF;
            switch(color)
            {
                case LightColor.RED:
                    switch(onOff)
                    {
                        case LightOnOffState.ON:
                            cmd = LightState.ON_RED;
                            break;
                        case LightOnOffState.BLINK:
                            cmd = LightState.BLINK_RED;
                            break;
                        case LightOnOffState.OFF:
                            cmd = LightState.OFF;
                            break;
                    }
                    break;
                case LightColor.GREEN:
                    switch (onOff)
                    {
                        case LightOnOffState.ON:
                            cmd = LightState.ON_GREEN;
                            break;
                        case LightOnOffState.BLINK:
                            cmd = LightState.BLINK_GREEN;
                            break;
                        case LightOnOffState.OFF:
                            cmd = LightState.OFF;
                            break;
                    }
                    break;
                    

            }
        
            var client = lightSenders[lightId];
            int retry = 3;
            while (!client.WriteAndCheckResponse(cmd))
            {
                retry--;
                if (retry == 0)
                {
                    string err = string.Format("Light({0}) Retry Send 3 Times Failed", lightId);
                    if (OnError != null)
                    {
                        OnError.BeginInvoke(null, err, null, null);
                    }
                    //3次重发超时异常
                    NLog.LogManager.GetCurrentClassLogger().Error(err);
                    return ErrorCode.按灯控制_发送灯后超时未收到应答;
                }
            }
            return ErrorCode.成功;
        }
        /// <summary>
        /// 发送命令并接受应答，返回成功或失败。
        /// 成功条件：
        /// - 发送成功
        /// - 应答成功
        /// </summary>
        /// <param name="lightSock"></param>
        /// <param name="cmd"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool LightSend(ref Socket lightSock, string cmd, int timeout)
        {
            try
            {
                lightSock.Send(Encoding.ASCII.GetBytes(cmd));
                if (lightSock.Poll(timeout, SelectMode.SelectRead))
                {
                    byte[] data = new byte[lightSock.Available];
                    lightSock.Receive(data);
                    string err = Encoding.ASCII.GetString(data);
                    if (err.Contains("ERROR"))
                    {
                        //发送失败
                        return false;
                    }
                    else if (err.Contains("OK"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        //阻塞等待制定ID的灯灭
        public void WaitLightOff(int lightId)
        {
            if (!lightSenders.ContainsKey(lightId))
            {
                NLog.LogManager.GetCurrentClassLogger().Error(string.Format("LightId({0}) Not Exist", lightId));
                return;
            }
           
            

        }





        public void Dispose()
        {
            server.DisconnectAllClients();
            server.Dispose();
        }
    }
}
