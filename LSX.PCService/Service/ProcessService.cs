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
            int lightColor = 1;
            this.tasks = new[]{
            tf.StartNew(() => ProcessOrderMain(cts.Token)),
            tf.StartNew(() => ProcessLightOrder(cts.Token,lightColor++)),
            tf.StartNew(() => ProcessLightOrder(cts.Token,lightColor++)),
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
        private static void ProcessOrderMain(CancellationToken token)
        {
            OrderMessageInputQueue orderQueue = new OrderMessageInputQueue();
            BoxInChannelInputQueue boxQueue = new BoxInChannelInputQueue();
            LightMessageQueue lightQueue = new LightMessageQueue();
            LightManager lightManager = LightManager.Instance;

            while (!token.IsCancellationRequested)
            {
                //获取订单号
                var orderMsg = orderQueue.Receive();
                //等待货物到达道口
                BoxInChannelMessage msg = boxQueue.Receive();
                int orderId = orderMsg.orderId;

                // 是否正常道口
                if (DbHelper.GetCurrentOrderChannel(orderId))
                {
                    // 1. 是，
                    //  获取订单对应的灯号
                    //int? lightId = DbHelper.GetLightIdByOrder(orderId);
                    //if (lightId == null)
                    //{//未分配灯ID

                    //    lightId = DbHelper.GetUnsedLightId();
                    //    if (lightId == null)
                    //    {//异常无可用的灯
                    //        throw new Exception("无可用的灯");
                    //    }
                    //    else
                    //        DbHelper.BindLightIdToOrder(orderId, (int)lightId);
                    //}

                    int? lightId = DbHelper.GetBindedLightByOrder(orderId);
                    if (null == lightId)
                    {
                        throw new Exception("无可用的灯");
                    }
                    else
                    {
                        //设置当前灯为占用状态
                        DbHelper.SetLightOccupied((int)lightId);
                        //将lightId发送到灯消息队列
                        lightQueue.Send(new LightMessage() { orderId = orderId, lightId = (int)lightId });


                    }

                }
                else
                {
                    // 2. 否，异常道口数据表添加一条记录
                    // 记录所有操作
                    DbHelper.OrderErrorLogAdd(orderId, "异常口");

                }


            }
        }

        private static void ProcessChannelOrder(CancellationToken token)
        {
            ChannelController ch = ChannelController.Instance;
            InputQueueCaseNum que = new InputQueueCaseNum();
            OrderMessageInputQueue orderQueue = new OrderMessageInputQueue();
            while (!token.IsCancellationRequested)
            {//获取箱号
                InputMessageCaseNum msg = que.Receive();


                //数据库中检查箱号是否为存在于 任务订单总表中
                //1. 是, 创建任务订单ID
                int orderId = 0;
                orderQueue.Send(new OrderMessage() { orderId = orderId });
                //   是，流水线发送目标通道：正常
                //   
                ch.SendToChannel((int)EnumChannel.正常道口);
                // 创建09-灯记录，生成托盘ID，分配灯ID
                // 等待货物到达道口消息
                // 发送灯点亮消息
                // 等待用户灭灯消息



                //2. 否，不创建任务订单ID
                //   否，流水线发送目标通道：异常
                ch.SendToChannel((int)EnumChannel.异常道口);
            }
        }
        /// <summary>
        /// 点亮灯并阻塞等待灭灯
        /// </summary>
        /// <param name="token"></param>
        private static void ProcessLightOrder(CancellationToken token, int lightColor)
        {
            LightMessageQueue lightQueue = new LightMessageQueue();
            LightManager lightManager = LightManager.Instance;
            int curLightColor = lightColor;

            while (!token.IsCancellationRequested)
            {
                var msg = lightQueue.Receive();
                if (null == msg) continue;


                //点亮当前灯
                ErrorCode err = lightManager.SetLight(msg.lightId);
                //记录当前灯状态到数据库
                if (err == ErrorCode.成功)
                {
                    DbHelper.SetLightState(true, curLightColor);

                    //等待灯灭消息
                    lightManager.WaitLightOff(msg.lightId);

                    //灭灯消息写入数据库
                    DbHelper.SetLightState(false, curLightColor);

                    //检查当前托盘是否集齐
                    //当前订单是否
                    if (DbHelper.CheckIsFullByLight(msg.lightId))
                    {
                        //释放当前灯
                        DbHelper.SetLightUnOccupied(msg.lightId);
                    }

                }

                else
                    DbHelper.OrderErrorLogAdd(msg.orderId, err.ToString());


            }
        }



        private static void ProcessLpnBinding(CancellationToken token)
        {
            BindingLocAndLpnMessageInputQueue bindingQueue = new BindingLocAndLpnMessageInputQueue();
            AgvController agv = AgvController.Instance;
            while (!token.IsCancellationRequested)
            {
                //扫描LPN、库位、09码 提示用户绑定
                //侧边有个是否拉动AGV按钮
                //按钮点击后弹出当前库位 绑定的所有LPN信息，用户确认后提交AGV转运

                //获取到用户绑定消息（包含库位、LPN字段）
                var bindingMsg = bindingQueue.Receive();


                //1. 将当前LPN和正在进行的09物料订单绑定
                //2. 将当前LPN


                //生成AGV转运表记录

                //获取AGV调度目标库位
                string target = null;
                //调用AGV发送转运订单
                agv.SendOrder(bindingMsg.location, target);

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
