using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Messaging;
using LSX.PCService.Data;
namespace LSX.PCService.Controllers
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
            {
                queue = new MessageQueue(queuePath);
                queue.Purge();
            }
            //设置当应用程序向消息对列发送消息时默认情况下使用的消息属性值
            queue.DefaultPropertiesToSend.AttachSenderId = false;
            queue.DefaultPropertiesToSend.UseAuthentication = false;
            queue.DefaultPropertiesToSend.UseEncryption = false;
            queue.DefaultPropertiesToSend.AcknowledgeType = AcknowledgeTypes.None;
            queue.DefaultPropertiesToSend.UseJournalQueue = false;
            queue.Formatter = new BinaryMessageFormatter();

        }
        public virtual void Purge()
        {
            queue.Purge();
        }
        public virtual object Peek(int timeout=-1)
        {
            if (timeout == -1)
            {
                return queue.Peek();
            }
            else
            {
                TimeSpan sp = new TimeSpan(0, 0, 0, 0, timeout);
                return queue.Peek(sp);
            }
        }
        public virtual object Receive(int timeout=-1)
        {
            try
            {
                if (timeout == -1)
                {
                    return queue.Receive(MessageQueueTransactionType.Automatic);
                }
                else
                {
                    TimeSpan sp = new TimeSpan(0, 0, 0, 0, timeout);
                    return queue.Receive(sp, MessageQueueTransactionType.Automatic);
                }
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
