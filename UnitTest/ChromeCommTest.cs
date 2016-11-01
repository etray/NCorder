using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading;

namespace UnitTest
{
    /// <summary>
    /// Summary description for ChromeCommTest
    /// </summary>
    [TestClass]
    public class ChromeCommTest
    {
        static MockBrowser Browser { get; set; }
        static NCorder.ChromeComm Comm { get; set; }

        public ChromeCommTest()
        {
        }

        [TestMethod]
        public void SequenceTest()
        {
            PropertyInfo propertyInfo = typeof(NCorder.ChromeComm).GetProperty("Sequence", BindingFlags.NonPublic | BindingFlags.Static);
            long prevSequence = (long)propertyInfo.GetValue(null);
            Assert.AreEqual(true, (long)propertyInfo.GetValue(null) > prevSequence);
        }

        [TestMethod]
        public void GetValueGetJsonTest()
        {
            Assert.AreEqual(false, string.IsNullOrWhiteSpace(Comm.GetValue("url")));
        }

        [TestMethod]
        public void NavigateToUrlTest()
        {
            Browser.CurrentUrl = string.Empty;
            Browser.PendingUrl = "about:blank";

            Comm.NavigateToUrl("about:blank");
            int count = 0;
            while(Browser.CurrentUrl != "about:blank")
            {
                Thread.Sleep(50);
                count++;
                if (count > 200)
                {
                    Assert.Fail();
                }
            }
        }

        [ClassInitialize()]
        public static void TestInitialize(TestContext context)
        {
            Browser = new MockBrowser(9997);
            Browser.Start();
            Comm = new NCorder.ChromeComm(9997);
        }

        [ClassCleanup()]
        public static void TestCleanup()
        {
            Browser.Stop();
        }
    }
}
