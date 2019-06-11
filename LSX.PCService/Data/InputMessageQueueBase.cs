using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Messaging;
namespace LSX.PCService.Data
{
    abstract class InputMessageQueueBase : IDisposable
    {
        public static string DefaultQueuePath = Config.QueuePath;
        protected MessageQueue queue;
        protected MessageQueueTransactionType transactionType = MessageQueueTransactionType.Automatic;
        public InputMessageQueueBase(string queuePath = "")
        {

            if (!MessageQueue.Exists(queuePath))
                queue = MessageQueue.Create(queuePath, true);
            else
                queue = new MessageQueue(queuePath);

            //设置当应用程序向消息对列发送消息时默认情况下使用的消息属性值
            queue.DefaultPropertiesToSend.AttachSenderId = false;
            queue.DefaultPropertiesToSend.UseAuthentication = false;
            queue.DefaultPropertiesToSend.UseEncryption = false;
            queue.DefaultPropertiesToSend.AcknowledgeType = AcknowledgeTypes.None;
            queue.DefaultPropertiesToSend.UseJournalQueue = false;
            queue.Formatter = new BinaryMessageFormatter();
        }

        public virtual object Receive()
        {
            try
            {
                return queue.Receive(MessageQueueTransactionType.Automatic);
            }
            catch (MessageQueueException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }
        public virtual void Send(object msg)
        {
            queue.Send(msg, MessageQueueTransactionType.Single);
        }
        public void Dispose()
        {
            queue.Dispose();
        }
    }
}
