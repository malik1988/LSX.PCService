using System;

using LSX.PCService.Data;
using Cave.Net;
using System.Net;
using System.Text;
namespace LSX.PCService.Controllers
{

    class CameraController : SingletonBase<CameraController>, IDisposable
    {
        TcpServer server;
        TcpAsyncClient client;
        public EventHandler<string> OnGetBoxId;
        NLog.Logger logger;

        private string _Ip;

        public string Ip
        {
            get { return _Ip; }
            set
            {
                _Ip = value;
                DbHelper.SetDeviceState(DeviceType.CAMERA, value);
            }
        }

        private string _Status;

        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                DbHelper.SetDeviceState(DeviceType.CAMERA, Ip, value);
            }
        }


        private CameraController()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            server = new TcpServer() { AcceptBacklog = 1, AcceptThreads = 1 };

            server.ClientAccepted += server_ClientAccepted;
            server.ClientException += server_ClientException;

            server.Listen(Config.CameraServerPort);

            Ip = "-";
            Status = DeviceState.初始化.ToString();

            logger.Info("Sever Start At " + Config.CameraServerPort.ToString());
        }

        void server_ClientException(object sender, TcpServerClientExceptionEventArgs<TcpAsyncClient> e)
        {
            Status = DeviceState.断开.ToString();
        }

        void server_ClientAccepted(object sender, TcpServerClientEventArgs<TcpAsyncClient> e)
        {
            this.client = e.Client;
            e.Client.Received += server_DataReceived;
            e.Client.Disconnected += Client_Disconnected;

            Ip = ((IPEndPoint)(e.Client.RemoteEndPoint)).Address.MapToIPv4().ToString();
            Status = DeviceState.已连接.ToString();

            logger.Info("Camera Client Connected:" + e.Client.RemoteEndPoint.ToString());
        }

        void Client_Disconnected(object sender, EventArgs e)
        {

            Status = DeviceState.断开.ToString();
        }

        private void server_DataReceived(object sender, BufferEventArgs e)
        {
            string cameraData = Encoding.ASCII.GetString(e.Buffer, 0, e.Length).Trim();
            //logger.Info("recv:" + str);
            if (null == cameraData) return;

            //数据解析匹配箱号


            string boxId = cameraData;
            ///1.如果解析正常，则箱号为实际箱号
            ///2.如果解析失败，则箱号为原始数据
            if (cameraData.Contains(","))
            {
                string[] tmp = cameraData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (tmp.Length >= 2)
                {
                    boxId = tmp[0];
                }
                else
                {
                    boxId = cameraData;
                }
            }
            else
            {
                boxId = cameraData;
            }




            //数据库中检查箱号是否为存在于 任务订单总表中
            string orderId = DbHelper.CreateOrderIdByBoxId(boxId);

            InputQueueCaseNum job = new InputQueueCaseNum();
            job.Send(new InputMessageCaseNum() { boxId = boxId, orderId = orderId });
            if (OnGetBoxId != null)
            {
                OnGetBoxId.BeginInvoke(null, orderId, null, null);
            }
        }


        public void Dispose()
        {
            this.server.DisconnectAllClients();
            this.server.Close();
        }
    }
}
