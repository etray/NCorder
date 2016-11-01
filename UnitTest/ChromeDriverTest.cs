using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCorder;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace UnitTest
{
    /// <summary>
    /// Summary description for ChromeDriverTest
    /// </summary>
    [TestClass]
    public class ChromeDriverTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            KillChromesAndWait();
        }

        [TestMethod]
        public void GetChromePathTest()
        {
            ChromeDriver driver = new ChromeDriver(0);
            Assert.AreEqual(false, string.IsNullOrWhiteSpace(driver.GetChromePath()));
            Assert.AreEqual(true, File.Exists(driver.GetChromePath()));
        }

        [TestMethod]
        public void CloseAllChromesTest()
        {
            int port = NextAvailablePort();
            int preCount = CountChromes();
            ChromeDriver driver = new ChromeDriver(port);
            driver.Start();
            Assert.AreEqual(true, this.WaitForMoreChromesThan(preCount));
            preCount = CountChromes();

            PrivateObject privateObject = new PrivateObject(driver);
            privateObject.Invoke("CloseAllChromes", new object[] { });
            Assert.AreEqual(true, WaitForNChromes(0));
            driver.Stop();
        }

        [TestMethod]
        public void LaunchChromeTest()
        {
            int port = NextAvailablePort();
            int preCount = CountChromes();
            ChromeDriver driver = new ChromeDriver(port);
            driver.Start();
            driver.NavigateToUrl("about:blank");

            Assert.AreEqual(true, this.WaitForMoreChromesThan(preCount));
            driver.Stop();
        }

        [TestMethod]
        public void ScrapeTitleTest()
        {
            int port = NextAvailablePort();
            ChromeDriver driver = new ChromeDriver(port);
            driver.Start();
            driver.NavigateToUrl("about:blank");
            Assert.AreEqual(true, driver.WaitForUrl("about:blank"));
            Assert.AreEqual(true, driver.WaitForTitle("about:blank"));
            Assert.AreEqual(true, driver.ScrapeTitle().ToLower().Contains("blank"));
            driver.Stop();
        }

        [TestMethod]
        public void ScrapeUrlTest()
        {
            int port = NextAvailablePort();
            ChromeDriver driver = new ChromeDriver(port);
            driver.Start();
            driver.NavigateToUrl("about:blank");
            Assert.AreEqual(true, driver.WaitForUrl("about:blank"));
            Assert.AreEqual(true, driver.ScrapeUrl().StartsWith("about"));
            driver.Stop();
        }

        [TestMethod]
        public void NavigateToUrlTest()
        {
            int port = NextAvailablePort();
            ChromeDriver driver = new ChromeDriver(port);
            driver.Start();
            driver.NavigateToUrl("about:blank");
            Assert.AreEqual(true, driver.WaitForUrl("about:blank"));
            driver.Stop();
        }

        private static bool WaitForNChromes(int n)
        {
            bool result = false;
            int count = -1;
            for (int i = 0; i < 500; i++)
            {
                Thread.Sleep(20);
                count = CountChromes();

                if (count >= n)
                {
                    break;
                }
            }

            if (count >= n)
            {
                result = true;
            }

            return result;
        }

        private static int CountChromes()
        {
            int count = 0;
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.ToLower().Contains("chrome"))
                {
                    count++;
                }
            }

            return count;
        }

        private bool WaitForMoreChromesThan(int preCount)
        {
            return WaitForNChromes(preCount + 1);
        }

        private static void KillChromesAndWait()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.ToLower().Contains("chrome"))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // might have been closed by user
                    }
                }
            }
            WaitForNChromes(0);
        }

        private bool UrlEventHandlerCalled { get; set; }

        private void UrlEventHandler(object sender, EventArgs e)
        {
            this.UrlEventHandlerCalled = true;
        }

        private bool TitleEventHandlerCalled { get; set; }

        private void TitleEventHandler(object sender, EventArgs e)
        {
            this.TitleEventHandlerCalled = true;
        }

        [TestMethod]
        public void TestUrlChangeDetection()
        {
            int port = NextAvailablePort();
            ChromeDriver driver = new ChromeDriver(port);
            UrlEventHandlerCalled = false;
            driver.UrlChangeEvent += UrlEventHandler;
            driver.Start();
            Thread.Sleep(5000);
            driver.NavigateToUrl("about:blank");
            Assert.AreEqual(true, driver.WaitForUrl("about:blank"));
            Assert.AreEqual(true, driver.ScrapeUrl().StartsWith("about"));
            Assert.AreEqual(true, UrlEventHandlerCalled);
            driver.Stop();
        }

        [TestMethod]
        public void TestTitleChangeDetection()
        {
            int port = NextAvailablePort();
            ChromeDriver driver = new ChromeDriver(port);
            TitleEventHandlerCalled = false;
            driver.TitleChangeEvent += TitleEventHandler;
            driver.Start();
            Thread.Sleep(5000);
            driver.NavigateToUrl("about:blank");
            Assert.AreEqual(true, driver.WaitForUrl("about:blank"));
            Assert.AreEqual(true, driver.WaitForTitle("about:blank"));
            Assert.AreEqual(true, TitleEventHandlerCalled);

            driver.Stop();
        }

        [TestMethod]
        public void CleanUpURLTest()
        {
            ChromeDriver driver = new ChromeDriver(-1);
            PrivateObject privateObject = new PrivateObject(driver);

            string result = (string)privateObject.Invoke("CleanUpURL", new object[] { "http://website.com&list=RDJEgVI-IKpqk&index=30" });
            Assert.AreEqual("http://website.com", result);

            result = (string)privateObject.Invoke("CleanUpURL", new object[] { "http://website.com&index=0&list=RDJEgVI-IKpqk&other=new" });
            Assert.AreEqual("http://website.com&other=new", result);

            result = (string)privateObject.Invoke("CleanUpURL", new object[] { "http://website.com" });
            Assert.AreEqual("http://website.com", result);

            driver.Stop();
        }

        private object portGetterSync = new object();

        private int NextAvailablePort()
        {
            int port = -1;
            lock (portGetterSync)
            {
                TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 0);
                listener.Start();
                port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
            }
            return port;
        }
    }
}
