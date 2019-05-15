using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Messaging;

using LSX.PCService.Models;
namespace LSX.PCService.Data
{
    /// <summary>
    /// 主线程，接受输入消息，触发控制
    /// </summary>
    class MainThread : IDisposable
    {

        Thread thread;
        public MainThread()
        {

            this.thread = new Thread(new ThreadStart(Process));
            this.thread.IsBackground = true;
            this.thread.SetApartmentState(ApartmentState.STA);
        }
        public void Start()
        {
            InitDb();
            this.thread.Start();
        }
        void InitDb()
        {
            if (Config.InitDataBase)
            {
                DbHelper.PreAllocateChannel();
                DbHelper.EmptyTable("order_running");
                DbHelper.EmptyTable("order_log");
                DbHelper.InitChannel();
                DbHelper.InitOrderStatus();
            }
        }
        enum EnumJobState
        {
            WaitCaseNum = 0, //输入箱号
            WaitChannelNotify,
        }

        private static void Process()
        {
            InputQueueCaseNum queueCaseNum = new InputQueueCaseNum();
            InputQueueChannelNotify queueChannelNotify = new InputQueueChannelNotify();

            EnumJobState state = EnumJobState.WaitCaseNum;

            ChannelController chController = ChannelController.Instance;
            LightManager lightManager = LightManager.Instance;



            while (true)
            {

                switch (state)
                {
                    case EnumJobState.WaitCaseNum:
                        InputMessageCaseNum caseMsg = queueCaseNum.Receive();
                        if (null != caseMsg)
                        {
                           // if (!PrePick(caseMsg.CaseNum))
                            {
                                chController.Begin(caseMsg.CaseNum);

                                state = EnumJobState.WaitChannelNotify;
                            }
                      
                        }
                        break;
                    case EnumJobState.WaitChannelNotify:
                        InputMessageChannelNotify chMsg = queueChannelNotify.Receive();
                        if (null != chMsg)
                        {
                            //lightManager.AutoSetLight(chMsg.ChannelId, chController.Order);
                            state = EnumJobState.WaitCaseNum;
                        }
                        break;
                }

            }
        }

       


        public void Stop()
        {
            InputJob exit = new InputJob();
            exit.Send(new InputMessage() { cmd = MessageCmd.Exit });
        }
        public void Dispose()
        {
            if (this.thread.IsAlive)
            {
                this.thread.Join();
            }
        }
    }
}
