using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Messaging;
using LSX.PCService.Data;
namespace LSX.PCService.Controllers
{
    [Serializable]
    class InputMessageCaseNum
    {
        public string boxId { get; set; }
    }
    class InputQueueCaseNum : InputMessageQueueBase
    {
        int count = 0;
        public InputQueueCaseNum()
            : base(@".\Private$\LXS.PCService.InputQueueCaseNum")
        {
        }
        public void Send(InputMessageCaseNum msg)
        {
            base.Send(msg);
        }
        public InputMessageCaseNum Receive()
        {
            Message msg = base.Receive() as Message;
            count++;

            NLog.LogManager.GetCurrentClassLogger().Info("InputQueueCaseNum :" + count.ToString());
            return (InputMessageCaseNum)msg.Body;
        }
    }


    [Serializable]
    class LightMessage
    {
        public string orderId { get; set; }
        public int lightId { get; set; }
    }
    class LightMessageQueue : InputMessageQueueBase
    {
        int count = 0;
        public LightMessageQueue(string section)
            : base(string.Format(@".\Private$\LXS.PCService.LightMessageQueue_{0}", section))
        {
        }
        public void Send(LightMessage msg)
        {
            base.Send(msg);
        }
        public LightMessage Receive()
        {
            Message msg = base.Receive() as Message;
            return (LightMessage)msg.Body;
        }
        public LightMessage Peek()
        {
            Message msg = base.Receive() as Message;
            count++;

            NLog.LogManager.GetCurrentClassLogger().Info("LightMessageQueue :" + count.ToString());
            return (LightMessage)msg.Body;
        }
    }
    [Serializable]
    class PalletMessage
    {
        public string pallet { get; set; }
    }
    class PalletInputQueue : InputMessageQueueBase
    {

        public PalletInputQueue()
            : base(@".\Private$\LXS.PCService.LightMessageQueue")
        {
        }
        public void Send(PalletMessage msg)
        {
            base.Send(msg);
        }
        public PalletMessage Receive()
        {
            Message msg = base.Receive() as Message;
            return (PalletMessage)msg.Body;
        }
    }
    [Serializable]
    class BoxInChannelMessage
    {
        public string orderId { get; set; }
        public int channelId { get; set; }
    }
    class BoxInChannelInputQueue : InputMessageQueueBase
    {
        int count = 0;
        public BoxInChannelInputQueue()
            : base(@".\Private$\LXS.PCService.BoxInChannelInputQueue")
        {

        }
        public void Send(BoxInChannelMessage msg)
        {
            base.Send(msg);
        }
        public BoxInChannelMessage Receive()
        {
            Message msg = base.Receive() as Message;
            count++;

            NLog.LogManager.GetCurrentClassLogger().Info("BoxInChannelInputQueue :" + count.ToString());
            return (BoxInChannelMessage)msg.Body;
        }
    }
    [Serializable]
    class OrderMessage
    {
        public string orderId { get; set; }
    }
    class OrderMessageInputQueue : InputMessageQueueBase
    {
        int count = 0;
        public OrderMessageInputQueue(EnumChannel channel)
            : base(string.Format(@".\Private$\LXS.PCService.OrderMessageInputQueue_{0}",channel.ToString("D")))
        {
        }
        public void Send(OrderMessage msg)
        {
            base.Send(msg);
        }
        public OrderMessage Receive(int timeout)
        {
            Message msg = base.Receive(timeout) as Message;
            count++;

            NLog.LogManager.GetCurrentClassLogger().Info("OrderMessage :" + count.ToString());
            if (msg == null)
            {
                return null;
            }
            return (OrderMessage)msg.Body;
        }
        public OrderMessage Peek(int timeout)
        {
            Message msg = base.Peek(timeout) as Message;
            if (null == msg)
            {
                return null;
            }
            return (OrderMessage)msg.Body;
        }
    }
    class OrderMessageArrivedInputQueue : InputMessageQueueBase
    {
        int count = 0;
        public OrderMessageArrivedInputQueue()
            : base(@".\Private$\LXS.PCService.OrderMessageArrivedInputQueue")
        {
        }
        public void Send(OrderMessage msg)
        {
            base.Send(msg);
        }
        public OrderMessage Receive(int timeout)
        {
            Message msg = base.Receive(timeout) as Message;
            count++;

            NLog.LogManager.GetCurrentClassLogger().Info("OrderMessage :" + count.ToString());
            if (msg == null)
            {
                return null;
            }
            return (OrderMessage)msg.Body;
        }
        public OrderMessage Peek(int timeout)
        {
            Message msg = base.Peek(timeout) as Message;
            if (null == msg)
            {
                return null;
            }
            return (OrderMessage)msg.Body;
        }
    }
    [Serializable]
    class BindingLocAndLpnMessage
    {
        public string location { get; set; }
        public string lpn { get; set; }
        public string c09 { get; set; }
    }
    class BindingLocAndLpnMessageInputQueue : InputMessageQueueBase
    {

        public BindingLocAndLpnMessageInputQueue()
            : base(@".\Private$\LXS.PCService.BindingLocAndLpnMessageInputQueue")
        {
        }
        public void Send(BindingLocAndLpnMessage msg)
        {
            base.Send(msg);
        }
        public BindingLocAndLpnMessage Receive()
        {
            Message msg = base.Receive() as Message;
            return (BindingLocAndLpnMessage)msg.Body;
        }
    }
}
