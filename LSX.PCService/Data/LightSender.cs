using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using HenkTcp;
using LSX.PCService.Models;
namespace LSX.PCService.Data
{

    class LightSender : Light, IEquatable<LightSender>
    {
        TcpClient sender;
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        byte[] _byteOn = { 0xaa };
        byte[] _byteOff = { 0x55 };

        public bool isOn = false;
        public LightSender(TcpClient client, Light l = null)
            : base()
        {
            this.sender = client;
            if (null != l)
            {
                this.Id = l.Id;
                this.Ip = l.Ip;
                this.C09码 = l.C09码;
                this.Channel_id = l.Channel_id;
                this.Inuse = l.Inuse;
            }
        }


        public TcpClient Sender { get { return this.sender as TcpClient; } }




        #region IEquatable<LightSender> 成员

        public override int GetHashCode()
        {
            return this.Id;
        }
        public bool Equals(LightSender other)
        {
            if (other == null || this.sender == null)
            {
                return false;
            }
            return this.sender.Equals(other.sender);
        }

        #endregion


        ErrorCode Send(HenkTcpServer server, bool on)
        {
            string log = "";
            ErrorCode retCode = ErrorCode.成功;
            byte[] data = new byte[]{this._byteOff[0]};
            if (on)
                data[0] = this._byteOn[0];
            Message msg = server.WriteAndGetReply(this.sender, data, new TimeSpan(0, 0, 1));
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
                log = "设置" + on.ToString() + "失败" + " :按灯控制_发送灯后超时未收到应答";
                retCode = ErrorCode.按灯控制_发送灯后超时未收到应答;
            }

            return retCode;
        }

        public ErrorCode Off(HenkTcpServer server)
        {
            return Send(server, false);
        }

        public ErrorCode On(HenkTcpServer server)
        {
            return Send(server, true);
        }
    }
}
