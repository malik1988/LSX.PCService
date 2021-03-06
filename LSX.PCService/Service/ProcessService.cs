﻿using LSX.PCService.Controllers;
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
            NLog.Logger logger = NLog.LogManager.GetLogger("ProcessOrderMain");
            LightColor nextColor=LightColor.RED;

            while (!token.IsCancellationRequested)
            {
                //等待货物到达道口
                BoxInChannelMessage msg = boxQueue.Receive();
                string orderId = msg.orderId;
                EnumChannel channelId = DbHelper.GetCurrentOrderChannel(orderId);
                if ((int)channelId != msg.channelId)
                {//当前到达通道与期望目标不一致，货物走错通道！
                    string _err = string.Format("货物到达通道与目标不一致！ 到达通道：{0},目标通道：{1}", msg.channelId, (int)channelId);
                    logger.Error(_err);
                }



                // 是否正常道口
                if (msg.channelId==(int)EnumChannel.正常道口)
                {
                    //绑定并获取灯ID
                    //绑定09-灯表
                    int? lightId = DbHelper.GetBindedLightByOrder(orderId, ref nextColor);
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
                                nextColor = LightColor.GREEN;
                                break;
                            case LightColor.GREEN:
                                lightGreenQueue.Send(new LightMessage() { orderId = orderId, lightId = (int)lightId });
                                nextColor = LightColor.RED;
                                break;
                        }


                    }

                }
                else
                {//异常道口，货物到达
                    DbHelper.SetOrderState(msg.orderId, (int)OrderState.已完成);

                }


            }
        }
        /// <summary>
        /// 获取箱号（初始订单创建）
        /// </summary>
        /// <param name="token"></param>
        private static void ProcessChannelOrder(CancellationToken token)
        {
            ChannelController ch = ChannelController.Instance;
            InputQueueCaseNum que = new InputQueueCaseNum();
            OrderMessageInputQueue orderOkQueue = new OrderMessageInputQueue(EnumChannel.正常道口);
            OrderMessageInputQueue orderErrQueue = new OrderMessageInputQueue(EnumChannel.异常道口);
            NLog.Logger logger = NLog.LogManager.GetLogger("ProcessChannelOrder");
            while (!token.IsCancellationRequested)
            {//获取箱号
                InputMessageCaseNum msg = que.Receive();
                string _info = "获取箱号：" + msg.boxId + " 订单：" + msg.orderId;
                logger.Info(_info);

                string orderId = msg.orderId;
                EnumChannel targetChannel = EnumChannel.异常道口;

                if (orderId != null && orderId.StartsWith("D"))
                {//订单有效，且为正常订单

                    if (DbHelper.OrderIsFinished(orderId))
                    {//订单已完成，则返回失败
                        targetChannel = EnumChannel.异常道口;
                    }
                    else
                    {
                        targetChannel = EnumChannel.正常道口; //1. 发送订单消息
                    }

                }
                //   流水线发送目标通道
                ch.SendToChannel(targetChannel);
                //接受成功才设置这个值
                //DbHelper.SetOrderRealChannel(orderId, targetChannel);

                //发送订单队列
                if (targetChannel == EnumChannel.正常道口)
                {
                    orderOkQueue.Send(new OrderMessage() { orderId = orderId });
                }
                else
                {
                    //2. 否，不创建任务订单ID
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
            NLog.Logger logger = NLog.LogManager.GetLogger("ProcessLightOrder");

            while (!token.IsCancellationRequested)
            {
                var msg = lightQueue.Receive();
                if (null == msg) continue;


                //点亮当前灯
                ErrorCode err = lightManager.SetLight(msg.lightId, LightOnOffState.ON, lightColor);

                logger.Info(err.ToString());
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

                    //如果接收到人工集齐消息

                    //检查当前托盘是否集齐
                    //当前订单是否
                    if (DbHelper.CheckIsFullByLight(msg.lightId))
                    {
                        //暂时还不能释放当前灯，状态切换成闪烁状态
                        //DbHelper.SetLightUnOccupied(msg.lightId);

                        //设置为闪烁状态
                        lightManager.SetLight(msg.lightId, LightOnOffState.BLINK, lightColor);
                        DbHelper.SetLightState(msg.lightId, LightOnOffState.BLINK);

                        //设置当前灯号为不可添加箱号的状态，当用户灭闪烁的灯时才能解锁该灯。
                        DbHelper.SetLightLocked(msg.lightId);
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
