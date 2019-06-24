using System;
using System.Text;
using System.Threading.Tasks;
using Cave.Net;
using System.Diagnostics;
using System.Net;

namespace LSX.PCService.Controllers
{
    class CustomTcpClient : IDisposable
    {
        TcpAsyncClient client;
        byte[] reply = null;
        object locker = new object();
        object lockerSend = new object();

        public EventHandler<string> OnReceived;
        public CustomTcpClient(TcpAsyncClient c)
        {
            client = c;
            client.Received += client_Received;
            client.Disconnected += client_Disconnected;
        }

        void client_Disconnected(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Client Disconnect:"+client.RemoteEndPoint.ToString());
            client = null;
        }

        public string Ip
        {
            get
            {
                IPEndPoint ipEnd = (IPEndPoint)(client.RemoteEndPoint);
                return ipEnd.Address.ToString();
            }
        }

        void client_Received(object sender, BufferEventArgs e)
        {
            lock (locker)
            { reply = e.Buffer; }
            if (OnReceived != null)
            {
                OnReceived.BeginInvoke(sender, Encoding.ASCII.GetString(e.Buffer), null, null);
            }
            e.Handled = true;
        }
        public bool Write(string cmd)
        {
            bool ret = false;
            lock (lockerSend)
            {
                try
                {
                    client.Send(Encoding.ASCII.GetBytes(cmd));
                    ret = true;
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Client Write Error:" + ex.Message);
                }
            }
            return ret;
        }
        public bool WriteAndCheckResponse(string cmd, int timeout = 2000)
        {
            try
            {
                if (!Write(cmd))
                {
                    return false;
                }

                Stopwatch sw = new Stopwatch();
                lock (locker)
                { reply = null; }
                sw.Start();

                while (reply == null && sw.Elapsed < new TimeSpan(0, 0, 0, 0, timeout))
                {
                    Task.Delay(5).Wait();
                }
                sw.Stop();
                if (reply != null)
                {
                    string resp = Encoding.ASCII.GetString(reply);
                    if (resp.Contains("ERROR"))
                    {
                        return false;
                    }
                    else if (resp.Contains("OK"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (client != null)
            {
                try
                {
                    client.Close();
                    client = null;
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        #endregion
    }
}
