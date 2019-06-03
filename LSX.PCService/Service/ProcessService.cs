using LSX.PCService.Data;
using LSX.PCService.Models;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace LSX.PCService.Service
{
    class ProcessService : SingletonBase<ProcessService>, IDisposable
    {
        CancellationTokenSource cts;
        TaskFactory tf;
        Task[] tasks;

        CameraController camera;
        private ProcessService()
        {
            cts = new CancellationTokenSource();
            tf = new TaskFactory(cts.Token);

        }

        public void Start()
        {
            camera = CameraController.Instance;
            this.tasks = new[]{
            tf.StartNew(() => ProcessChannelOrder(cts.Token)),
            tf.StartNew(() => ProcessLightOrder(cts.Token)),
            tf.StartNew(() => ProcessLpnBinding(cts.Token))
            };

        }
        public void Stop()
        {
            cts.Cancel();
            if (null != this.tasks)
            {
                foreach (var t in this.tasks)
                {
                    t.Wait();
                }
            }
        }
        private static void ProcessChannelOrder(CancellationToken token)
        {
            ChannelController ch = ChannelController.Instance;
            InputQueueCaseNum que = new InputQueueCaseNum();
            LightMessageQueue lightQueue = new LightMessageQueue();
            while (!token.IsCancellationRequested)
            {//获取箱号
                InputMessageCaseNum msg = que.Receive();
                if (null == msg)
                {
                    continue;
                }

                string caseNum = msg.CaseNum;
                if (DbHelper.IsSinglePallet(caseNum)==true)
                {//直接进入绑定环节

                }
                else
                {//流水线控制


                    OrderRunning order = ch.CreateOrder(caseNum);

                    if (order == null)
                    {
                        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                        logger.Info("CreateOrder Failed,无效箱号：" + caseNum);
                        return;
                    }

                    ch.Begin(msg.CaseNum);

                    //发送亮灯消息
                    lightQueue.Send(new LightMessage()
                    {
                        channelId=2,
                        orderId = order.Id
                    });
                }
            }
        }

        private static void ProcessLightOrder(CancellationToken token)
        {
            LightMessageQueue lightQueue = new LightMessageQueue();
            LightManager lightManager = LightManager.Instance;
            while (!token.IsCancellationRequested)
            {
                var msg = lightQueue.Receive();
                if (null == msg) continue;

                //检查数据库中 orderId 对应的订单状态
            

                lightManager.AutoSetLight(msg.channelId, msg.orderId);

            }
        }



        private static void ProcessLpnBinding(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {

            }
        }


        #region IDisposable 成员

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
}
