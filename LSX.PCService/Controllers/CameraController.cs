using System;

using HenkTcp;
using LSX.PCService.Data;
namespace LSX.PCService.Controllers
{

    class CameraController : SingletonBase<CameraController>, IDisposable
    {
        HenkTcpServer server = null;
       public EventHandler<string> OnGetBoxId;

        private CameraController()
        {
            server = new HenkTcpServer();
            server.DataReceived += server_DataReceived;
            server.Start(Config.CameraServerPort, 1);
            NLog.LogManager.GetCurrentClassLogger().Info("Sever Start At " + Config.CameraServerPort.ToString());
        }

        void server_DataReceived(object sender, Message e)
        {
            string str = e.MessageString;
            NLog.LogManager.GetCurrentClassLogger().Info("recv:" + str);
            if (null == str) return;

            //数据解析匹配箱号

     

            string[] tmp = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tmp.Length != 2) return;
            
            //////////////////////////////////////////////////////////////////////////
            //1.如果扫描的数据无法解析 orders表创建一个以E为开始的订单，扫描的信息记录到箱号字段中
            DbHelper.CreateErrorOrder(str);


            //2.如果解析正常
            //////////////////////////////////////////////////////////////////////////


            string boxId = tmp[0];
            int count = int.Parse(tmp[1]);
            InputQueueCaseNum job = new InputQueueCaseNum();
            job.Send(new InputMessageCaseNum() { boxId = boxId });
            if (OnGetBoxId != null)
            {
                OnGetBoxId.BeginInvoke(null, boxId, null, null);
            }
        }


        public void Dispose()
        {
            if (this.server.IsRunning)
                this.server.Stop();
        }
    }
}
