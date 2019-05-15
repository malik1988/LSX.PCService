using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HenkTcp;
using System.Net.Sockets;
namespace LSX.PCService.Data
{

    class CameraController : SingletonBase<CameraController>, IDisposable
    {
        HenkTcpServer server = null;

        private CameraController()
        {
            server = new HenkTcpServer();
            server.DataReceived += server_DataReceived;
            server.Start(Config.CameraServerPort, 1);
        }

        void server_DataReceived(object sender, Message e)
        {
            string str = e.MessageString;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("recv:" + str);
            if (null == str) return;

            //数据解析
            string[] tmp = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tmp.Length != 2) return;


            string caseNum = tmp[0];
            int count = int.Parse(tmp[1]);
            InputQueueCaseNum job = new InputQueueCaseNum();
            job.Send(new InputMessageCaseNum() { CaseNum = tmp[0] });
        }


        public void Dispose()
        {
            if (this.server.IsRunning)
                this.server.Stop();
        }
    }
}
