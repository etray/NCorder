using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace UnitTest
{
    public class MockBrowser
    {
        private int DebugPort { get; set; }
        private bool KeepBrowserRunning { get; set; }
        private Thread MockBrowserThread { get; set; }

        public string PendingUrl { get; set; }
        public string PendingTitle { get; set; }
        public string CurrentUrl { get; set; }
        public string CurrentTitle { get; set; }

        HttpServer Server { get; set; }

        public MockBrowser(int debugPort)
        {
            this.DebugPort = debugPort;
        }
        
        public void Start()
        {
            this.KeepBrowserRunning = true;

            this.MockBrowserThread = new Thread(new ThreadStart(this.ServerThread));
            this.MockBrowserThread.Start();
        }

        public void ServerThread()
        {
            Server = new HttpServer("localhost", this.DebugPort);
            Server.CurrentPageTitle = "Google";
            Server.CurrentPageUrl = "http://www.google.com";
            Server.DataReceived = DataReceived;
            Task.Run(() => Server.Run());
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public void Stop()
        {
            if (this.MockBrowserThread != null && this.MockBrowserThread.IsAlive)
            {
                try
                {
                    Server.Listener.Stop();
                    MockBrowserThread.Interrupt();
                    MockBrowserThread.Abort();
                    MockBrowserThread.Join();
                }
                catch
                {
                    // Possible for thread to die between alive check and join
                }
            }
        }

        private void DataReceived(string data)
        {
            this.CurrentUrl = this.PendingUrl;
            this.CurrentTitle = this.PendingTitle;
        }
    }
}
