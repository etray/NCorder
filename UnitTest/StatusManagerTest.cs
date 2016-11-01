using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCorder;

namespace UnitTest
{
    [TestClass]
    public class StatusManagerTest
    {
        string Status { get; set; }

        public StatusManagerTest()
        {
            this.Status = "uninitialized";
            StatusManager.SetText = setStatus;
        }
        
        private void setStatus(string value)
        {
            Status = value;
        }

        [TestMethod]
        public void TestClear()
        {
            Assert.AreNotEqual(string.Empty, this.Status);
            StatusManager.Clear();
            Assert.AreEqual(string.Empty, this.Status);
        }

        [TestMethod]
        public void TestSetIdle()
        {
            Assert.AreNotEqual("Idle.", this.Status);
            StatusManager.Idle();
            Assert.AreEqual("Idle.", this.Status);
        }

        [TestMethod]
        public void TestSetStatus()
        {
            Assert.AreNotEqual("new status", this.Status);
            StatusManager.Status("new status");
            Assert.AreEqual("new status", this.Status);
        }
    }
}
