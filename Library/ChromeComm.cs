using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace NCorder
{
    // Uses chrome devtools remote debugging protocol to control the browser from an external process.
    public class ChromeComm
    {
        private static string NavigateJson = "{{\"id\":{0},\"method\":\"Page.navigate\",\"params\":{{\"url\":\"{1}\"}}}}";
        private static string PageDataUrl = "http://localhost:{0}/json";

        private static long _sequence = 0;
        private static long Sequence
        {
            get
            {
                _sequence++;
                return _sequence;
            }
        }

        private int Port { get; set; }
        private WebSocket Socket { get; set; }

        public ChromeComm(int port)
        {
            this.Port = port;
        }

        // Calls debugger endpoint to get data on opened pages.
        private IList<IDictionary<string, string>> GetPageData()
        {
            IList<IDictionary<string, string>> result = null;
            JsonSerializer serializer = new JsonSerializer();
            
            string url = String.Format(PageDataUrl, this.Port);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        result = (IList<IDictionary<string, string>>)serializer.Deserialize(reader, typeof(IList<IDictionary<string, string>>));
                    }
                }
            }

            return result;
        }

        // Gets a value from a page's metatadata retrieved from the debugger.
        // Based on observations that the active tab comes first, 
        // we're assuming that the first document of type "page" is the relevant one.
        public string GetValue(string name)
        {
            string type = string.Empty;
            string value = string.Empty;
            IList<IDictionary<string, string>> data = this.GetPageData();

            foreach (var page in data)
            {
                page.TryGetValue("type", out type);
                if (type == "page")
                {
                    page.TryGetValue(name, out value);
                    break;
                }
            }

            return value;
        }

        // Public method to tell the browser to load a page.
        public void NavigateToUrl(string url)
        {
            if (this.Socket == null)
            {
                this.OpenSocket();

                for (int i = 0; i < 500; i++)
                {
                    if (this.Socket != null)
                    {
                        break;
                    }
                    Thread.Sleep(20);
                }

                if (this.Socket == null)
                {
                    throw new TimeoutException("Socket not created in a timely fashion.");
                }
            }

            this.SendNavigateCommand(url);
        }

        // Connect to chrome debugger.
        private void OpenSocket()
        {
            string socketUrl = this.GetValue("webSocketDebuggerUrl");
            WebSocket websocket = new WebSocket(socketUrl);
            websocket.Opened += new EventHandler(SetSocket);
            websocket.Open();
        }

        private void SetSocket(object sender, EventArgs e)
        {
            this.Socket = (WebSocket)sender;
        }

        private void SendNavigateCommand(string url)
        {
            this.Socket.Send(String.Format(NavigateJson, Sequence, url));
        }
    }
}
