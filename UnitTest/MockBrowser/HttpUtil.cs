using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public static class HttpUtil
    {
        private const int MaxBodySize = 2048 * 1024;

        public static IDictionary<string,string> ReceiveHeaders(Stream stream)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            string headerField = string.Empty;

            // All headers up to first blank line
            while (!string.IsNullOrWhiteSpace(headerField = ReadLine(stream)))
            {
                string[] tokens = headerField.Split(':');
                if (tokens.Length == 2)
                {
                    result[tokens[0].Trim()] = tokens[1].Trim();
                }
            }

            return result;
        }

        public static int BodyLength(IDictionary<string, string> headers)
        {
            int length = 0;

            if (headers != null && headers.ContainsKey("content-length"))
            {
                int.TryParse(headers["content-length"], out length);
            }

            return length;
        }

        public static string ReceiveBody(Stream stream, int length)
        {
            byte[] response = null;

            // the number of bytes successfully read
            int readCount = 0;

            // we've reached the body, now read the bytes. 
            if (length <= MaxBodySize)
            {
                response = new byte[length];

                while (readCount < length)
                {
                    int bytesRead = 0;
                    try
                    {
                        bytesRead = stream.Read(response, readCount, length - readCount);
                    }
                    catch (IOException exception)
                    {
                        Console.Error.WriteLine(exception.Message);
                    }

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    readCount += bytesRead;
                }

                if (readCount < length)
                {
                    Console.Error.WriteLine("Number of bytes read was less than content length header specified.");
                }
            }

            return System.Text.Encoding.UTF8.GetString(response, 0, readCount);
        }


        // get bytes up to \r\n, discarding the \r\n
        // then convert to UTF8-encoded string
        public static string ReadLine(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            int prevByte = 0;
            int curByte = 0;
            while (true)
            {
                prevByte = curByte;
                curByte = stream.ReadByte();
                if (curByte == -1 || (curByte == 0xa && prevByte == 0xd))
                {
                    break;
                }
                else
                {
                    if (curByte != 0xd && curByte != 0xa)
                    {
                        bytes.Add((byte)curByte);
                    }
                }
            }

            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
