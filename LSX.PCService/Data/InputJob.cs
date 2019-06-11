using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Messaging;
namespace LSX.PCService.Data
{
    [Serializable]
    class InputMessageCaseNum
    {
        public string CaseNum { get; set; }
    }
    class InputQueueCaseNum : InputMessageQueueBase
    {
        public InputQueueCaseNum()
            : base(@".\Private$\LXS.PCService.InputQueueCaseNum")
        { }
        public void Send(InputMessageCaseNum msg)
        {
            base.Send(msg);
        }
        public InputMessageCaseNum Receive()
        {
            Message msg = base.Receive() as Message;
            return (InputMessageCaseNum)msg.Body;
        }
    }
    [Serializable]
    class InputMessageChannelNotify
    {
        public int ChannelId { get; set; }
    }
    class InputQueueChannelNotify : InputMessageQueueBase
    {
        public InputQueueChannelNotify()
            : base(@".\Private$\LXS.PCService.InputQueueChannelNotify")
        { }
        public void Send(InputMessageChannelNotify msg)
        {
            base.Send(msg);
        }
        public InputMessageChannelNotify Receive()
        {
            Message msg = base.Receive() as Message;
            return (InputMessageChannelNotify)msg.Body;
        }
    }
    class InputJob : InputMessageQueueBase
    {
        public InputJob()
            : base(InputMessageQueueBase.DefaultQueuePath)
        {
        }
        public void Send(InputMessage msg)
        {
            base.Send(msg);
        }
        public InputMessage Receive()
        {
            Message msg = base.Receive() as Message;
            return (InputMessage)msg.Body;
        }
    }

    [Serializable]
    class LightMessage
    {
        public int orderId { get; set; }
        public int lightId { get; set; }
    }
    class LightMessageQueue : InputMessageQueueBase
    {

        public LightMessageQueue()
            : base(@".\Private$\LXS.PCService.LightMessageQueue")
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
        public int channelId { get; set; }
    }
    class BoxInChannelInputQueue : InputMessageQueueBase
    {

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
            return (BoxInChannelMessage)msg.Body;
        }
    }
    [Serializable]
    class OrderMessage
    {
        public int orderId { get; set; }
    }
    class OrderMessageInputQueue : InputMessageQueueBase
    {

        public OrderMessageInputQueue()
            : base(@".\Private$\LXS.PCService.BoxInChannelInputQueue")
        {
        }
        public void Send(OrderMessage msg)
        {
            base.Send(msg);
        }
        public OrderMessage Receive()
        {
            Message msg = base.Receive() as Message;
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
