using LSX.PCService.Controllers;
using LSX.PCService.Data;
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
        ChannelController channel;
        LightManager lightManager;


        private ProcessService()
        {
            cts = new CancellationTokenSource();
            tf = new TaskFactory(cts.Token);
        }

        public void Start()
        {
            //启动所有服务器
            camera = CameraController.Instance;
            channel = ChannelController.Instance;
            lightManager = LightManager.Instance;
            //开启任务线程
            this.tasks = new[]{
                tf.StartNew(() => ProcessOrderMain(cts.Token)),
                tf.StartNew(() => ProcessChannelOrder(cts.Token)),
                tf.StartNew(() => ProcessLightOrder(cts.Token,LightColor.RED)),
                tf.StartNew(() => ProcessLightOrder(cts.Token,LightColor.GREEN))         
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
        private static void ProcessOrderMain(CancellationToken token)
        {
            BoxInChannelInputQueue boxQueue = new BoxInChannelInputQueue();
            LightMessageQueue lightRedQueue = new LightMessageQueue(LightColor.RED.ToString("D"));
            LightMessageQueue lightGreenQueue = new LightMessageQueue(LightColor.GREEN.ToString("D"));

            LightManager lightManager = LightManager.Instance;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            while (!token.IsCancellationRequested)
            {
                //等待货物到达道口
                BoxInChannelMessage msg = boxQueue.Receive();
                string orderId = msg.orderId;
                // 是否正常道口
                if (DbHelper.GetCurrentOrderChannel(orderId))
                {
                    //绑定并获取灯ID
                    //绑定09-灯表
                    int? lightId = DbHelper.GetBindedLightByOrder(orderId);
                    if (null == lightId)
                    {
                        logger.Error("数据库中无可用的灯！");
                    }
                    else
                    {
                        //设置当前灯为占用状态
                        DbHelper.SetLightOccupied((int)lightId);
                        LightColor color = DbHelper.GetLightColor((int)lightId);
                        //将lightId发送到灯消息队列
                        switch (color)
                        {
                            case LightColor.RED:
                                lightRedQueue.Send(new LightMessage() { orderId = orderId, lightId = (int)lightId });
                                break;
                            case LightColor.GREEN:
                                lightGreenQueue.Send(new LightMessage() { orderId = orderId, lightId = (int)lightId });
                                break;
                        }


                    }

                }
                else
                {
                    // 2. 否，异常道口数据表添加一条记录
                    // 记录所有操作
                    //DbHelper.OrderErrorLogAdd(orderId, "异常口");

                }


            }
        }

        private static void ProcessChannelOrder(CancellationToken token)
        {
            ChannelController ch = ChannelController.Instance;
            InputQueueCaseNum que = new InputQueueCaseNum();
            OrderMessageInputQueue orderOkQueue = new OrderMessageInputQueue(EnumChannel.正常道口);
            OrderMessageInputQueue orderErrQueue = new OrderMessageInputQueue(EnumChannel.异常道口);
            while (!token.IsCancellationRequested)
            {//获取箱号
                InputMessageCaseNum msg = que.Receive();

                NLog.LogManager.GetCurrentClassLogger().Info("ProcessChannelOrder " + msg.boxId.ToString());

                //数据库中检查箱号是否为存在于 任务订单总表中
                string orderId = DbHelper.CreateOrderIdByBoxId(msg.boxId);

                if (orderId != null)
                {
                    bool finished = DbHelper.OrderIsFinished(orderId);
                    if (finished)
                    {
                        ch.SendToChannel(EnumChannel.异常道口);
                    }
                    else
                    {

                        //1. 发送订单消息
                        orderOkQueue.Send(new OrderMessage() { orderId = orderId });
                        //   是，流水线发送目标通道：正常
                        //   
                        ch.SendToChannel(EnumChannel.正常道口);
                        // 创建09-灯记录，生成托盘ID，分配灯ID
                        // 等待货物到达道口消息
                        // 发送灯点亮消息
                        // 等待用户灭灯消息
                    }
                }

                else
                {
                    //2. 否，不创建任务订单ID
                    //   否，流水线发送目标通道：异常
                    ch.SendToChannel(EnumChannel.异常道口);
                    orderErrQueue.Send(new OrderMessage() { orderId = orderId });
                }

            }
        }
        /// <summary>
        /// 点亮灯并阻塞等待灭灯
        /// </summary>
        /// <param name="token"></param>
        private static void ProcessLightOrder(CancellationToken token, LightColor lightColor)
        {
            LightMessageQueue lightQueue = new LightMessageQueue(lightColor.ToString("D"));
            LightManager lightManager = LightManager.Instance;
            LightColor curLightColor = lightColor;

            while (!token.IsCancellationRequested)
            {
                var msg = lightQueue.Receive();
                if (null == msg) continue;


                //点亮当前灯
                ErrorCode err = lightManager.SetLight(msg.lightId, LightOnOffState.ON, lightColor);

                NLog.LogManager.GetCurrentClassLogger().Info("ProcessLightOrder :" + err.ToString());
                //记录当前灯状态到数据库
                if (err == ErrorCode.成功)
                {
                    DbHelper.SetLightState(msg.lightId, LightOnOffState.ON);

                    //等待灯灭消息
                    lightManager.WaitLightOff(msg.lightId);

                    //灭灯消息写入数据库
                    DbHelper.SetLightState(msg.lightId, LightOnOffState.OFF);
                    //设置当前订单已完成
                    DbHelper.FinishOrderById(msg.orderId);

                    //检查当前托盘是否集齐
                    //当前订单是否
                    if (DbHelper.CheckIsFullByLight(msg.lightId))
                    {
                        //暂时还不能释放当前灯，状态切换成闪烁状态
                        //DbHelper.SetLightUnOccupied(msg.lightId);

                        //设置为闪烁状态
                        lightManager.SetLight(msg.lightId, LightOnOffState.BLINK, lightColor);
                        DbHelper.SetLightState(msg.lightId, LightOnOffState.BLINK);
                    }

                }

                else
                {
                    //日志记录
                    //TODO
                    //DbHelper.OrderErrorLogAdd(msg.orderId, err.ToString());
                }

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
