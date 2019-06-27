using System;

using System.Net.Sockets;
using HenkTcp;
using Cave.Net;
using System.Net;
using LSX.PCService.Data;
namespace LSX.PCService.Controllers
{

    class LightController : IEquatable<LightController>
    {
        CustomTcpClient sender;
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        byte[] _byteOn = { 0xaa };
        byte[] _byteOff = { 0x55 };
        /// <summary>
        /// 灯状态
        /// </summary>
        private LightState _State;
        public LightState State
        {
            get { return _State; }
            private set
            {
                DbHelper.SetLightState(id, value);
                _State = value;
            }
        }
        private bool _LowVoltage;

        public bool LowVoltage
        {
            get { return _LowVoltage; }
            set
            {
                DbHelper.SetLightVoltage(this.id,value);
                _LowVoltage = value;
            }
        }

        private int id;
        public LightController(CustomTcpClient client)
            : base()
        {
            this.sender = client;
            this.id = GetLightIdFromIpAddr(client.Ip);
            this.sender.OnReceived += ClientOnReceived;
            State = LightState.OFF;
        }

        private void ClientOnReceived(object sender, string e)
        {
            if (e.StartsWith("AT+ROFF"))
            {//红灯灭
                this.sender.Write("OK");
                State = LightState.OFF;
            }
            else if (e.StartsWith("AT+GOFF"))
            {//绿灯灭
                this.sender.Write("OK");
                State = LightState.OFF;
            }
            else if (e.StartsWith("AT+YOFF"))
            {
                this.sender.Write("OK");
                State = LightState.OFF;

            }
            else if (e.StartsWith("AT+RBOFF"))
            {//红灯闪灭
                this.sender.Write("OK");
                State = LightState.OFF;

            }
            else if (e.StartsWith("AT+GBOFF"))
            {//绿灯闪灭
                this.sender.Write("OK");
                State = LightState.OFF;
            }
            else if (e.StartsWith("AT+YBOFF"))
            {//黄灯闪灭
                this.sender.Write("OK");
                State = LightState.OFF;
            }
            else if (e.StartsWith("AT+LOWBAT"))
            {//欠压消息
                this.sender.Write("OK");
                //欠压
            }
        }

        int GetLightIdFromIpAddr(string ip)
        {
            string[] tmp = ip.Split(new char[] { '.' });
            return int.Parse(tmp[3]);
        }

        #region 消息指令
        const string ON_RED = "AT+RON";
        const string ON_GREEN = "AT+GON";
        const string ON_YELLOW = "AT+YON";
        const string OFF = "AT+OFF";
        const string BLINK_RED = "AT+RBLINK";
        const string BLINK_GREEN = "AT+GBLINK";
        const string BLINK_YELLOW = "AT+YBLINK";
        #endregion
        public bool WriteAndCheckResponse(LightState state)
        {
            LightState tmp = LightState.OFF;
            string cmd = null;
            switch (state)
            {
                case LightState.OFF:
                    cmd = OFF;
                    tmp = LightState.OFF;
                    break;
                case LightState.ON_GREEN:
                    cmd = ON_GREEN;
                    tmp = LightState.ON_GREEN;
                    break;
                case LightState.ON_RED:
                    cmd = ON_RED;
                    tmp = LightState.ON_RED;
                    break;
                case LightState.BLINK_GREEN:
                    cmd = BLINK_GREEN;
                    tmp = LightState.BLINK_GREEN;
                    break;
                case LightState.BLINK_RED:
                    cmd = BLINK_RED;
                    tmp = LightState.BLINK_RED;
                    break;
            }
            bool ok = this.sender.WriteAndCheckResponse(cmd);
            if (ok)
            {
                State = tmp;
            }
            return ok;
        }





        #region IEquatable<LightSender> 成员

        public override int GetHashCode()
        {
            return this.id;
        }
        public bool Equals(LightController other)
        {
            if (other == null || this.sender == null)
            {
                return false;
            }
            return this.sender.Equals(other.sender);
        }

        #endregion


    }
}
