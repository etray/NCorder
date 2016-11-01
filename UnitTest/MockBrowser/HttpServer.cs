using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest
{
    public class HttpServer
    {
        public const string JsonResponsePattern = "[{{\"title\": \"{0}\",\"type\": \"page\",\"url\": \"{1}\",\"webSocketDebuggerUrl\": \"ws://localhost:{2}\"}}]";
        public const string CRLF = "\r\n";
        public const string HTTP_VERSION = "HTTP/1.1";

        public string CurrentPageTitle { get; set; }
        public string CurrentPageUrl { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public TcpListener Listener { get; set; }

        public Action<string> DataReceived { get; set; }

        public HttpServer(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }

        public async Task Run ()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            bool isIpV6 = ipAddress.AddressFamily == AddressFamily.InterNetworkV6;

            Listener = new TcpListener(isIpV6 ? IPAddress.IPv6Any : IPAddress.Any, Port);
            Listener.Start();

            while (true)
            {
                using (TcpClient client = await Listener.AcceptTcpClientAsync())
                {
                    using (Stream stream = client.GetStream())
                    {
                        IDictionary<string, string> headers = HttpUtil.ReceiveHeaders(stream);
                        if(headers.Keys.Contains("connect"))
                        {
                            StringBuilder response = new StringBuilder();
                            response.Append("HTTP/1.1 200 OK" + CRLF);
                            response.Append(CRLF);

                            byte[] message = Encoding.UTF8.GetBytes(response.ToString());

                            stream.Write(message, 0, message.Length);
                        }
                        else if (headers.Keys.Contains("Sec-WebSocket-Key"))
                        {
                            Handshake(client, headers, stream);
                            WebSocketClientHandler(client, headers, stream);
                        }
                        else
                        {
                            this.WriteJsonResponse(client, headers, stream);
                        }
                    }
                }
            }
        }

        public void Handshake(TcpClient client, IDictionary<string, string> headers, Stream stream)
        {
            string socketAcceptString = headers["Sec-WebSocket-Key"];
            socketAcceptString += "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

            StringBuilder response = new StringBuilder();
            response.Append("HTTP/1.1 101 Switching Protocols" + CRLF);
            response.Append("Connection: Upgrade" + CRLF);
            response.Append("Upgrade: websocket" + CRLF);
            response.Append("Sec-WebSocket-Accept: " + Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(socketAcceptString))) + CRLF);         
            response.Append(CRLF);

            byte[] message = Encoding.UTF8.GetBytes(response.ToString());

            stream.Write(message, 0, message.Length);
        }

        public void WriteJsonResponse(TcpClient client, IDictionary<string, string> headers, Stream stream)
        {
            string responseBody = string.Format(HttpServer.JsonResponsePattern, this.CurrentPageTitle, this.CurrentPageUrl, this.Port);

            StringBuilder response = new StringBuilder();
            response.Append("HTTP/1.1 200 OK" + CRLF);
            response.Append("Content-Length: " + responseBody.Length + CRLF);
            response.Append("Content-Type: text/html;charset=UTF-8" + CRLF);
            response.Append(CRLF);
            response.Append(responseBody);

            byte[] message = Encoding.UTF8.GetBytes(response.ToString());

            stream.Write(message, 0, message.Length);
        }

        private void WebSocketClientHandler(TcpClient client, IDictionary<string, string> headers, Stream stream)
        {
            if (client != null)
            {
                string message = string.Empty;
                IList<byte> responseBytes = new List<byte>();

                Thread.Sleep(250);

                using (StreamReader reader = new StreamReader(stream))
                {
                    while(reader.Peek() > -1)
                    {
                        responseBytes.Add((byte)reader.Read());
                    }
                }

                // TODO: Properly unmask response - right now we're observing that it returns data, and assuming it was valid.
                if (responseBytes.Count == 0 || responseBytes.Count != 0)
                {
                    byte[] response = responseBytes.ToArray<byte>();                    
                    this.DataReceived(System.Text.Encoding.UTF8.GetString(response, 0, response.Length));
                }
            }
        }

        public void Stop ()
        {
            Listener.Server.Close();
        }
    }
}
