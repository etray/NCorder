using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCorder;
using System.Collections;
using System.IO;

namespace UnitTest
{
    /// <summary>
    /// Summary description for DataPersistorTest
    /// </summary>
    [TestClass]
    public class DataPersistorTest
    {
        IDictionary<string, string> Data { get; set; }

        public DataPersistorTest()
        {
            this.Data = new Dictionary<string, string>();
        }

        [TestMethod]
        public void SaveRestoreTest()
        {
            DataPersistor persistor = new DataPersistor();
            persistor.FullPath = Path.Combine(Path.GetTempPath(), "test1.json");
            persistor.SaveDataMethod = this.SaveMethod;
            persistor.RestoreDataMethod = this.RestoreMethod;
            this.Data["value1"] = "ORIGINAL DATA1";
            this.Data["value2"] = "ORIGINAL DATA2";
            persistor.Save();
            // change some values
            this.Data["value1"] = "New DATA1";
            this.Data["value2"] = "New DATA2";

            persistor.Restore();

            // check for restored values
            Assert.AreEqual("ORIGINAL DATA1", this.Data["value1"]);
            Assert.AreEqual("ORIGINAL DATA2", this.Data["value2"]);
            File.Delete(persistor.FullPath);
        }

        [TestMethod]
        public void MultipleSaveTest()
        {
            DataPersistor persistor = new DataPersistor();
            persistor.FullPath = Path.Combine(Path.GetTempPath(), "test2.json");
            persistor.SaveDataMethod = this.SaveMethod;
            persistor.RestoreDataMethod = this.RestoreMethod;
            this.Data["value1"] = "ORIGINAL DATA1";
            this.Data["value2"] = "ORIGINAL DATA2";
            persistor.Save();
            persistor.Restore();
            this.Data["value3"] = "ADDED DATA3";
            persistor.Save();
            persistor.Restore();
            this.Data["value2"] = "CHANGED DATA2";
            persistor.Save();
            persistor.Restore();
            // check for restored values
            Assert.AreEqual("ORIGINAL DATA1", this.Data["value1"]);
            Assert.AreEqual("CHANGED DATA2", this.Data["value2"]);
            Assert.AreEqual("ADDED DATA3", this.Data["value3"]);
            File.Delete(persistor.FullPath);
        }

        private IDictionary<string,string> SaveMethod()
        {
            return this.Data;
        }

        private void RestoreMethod(IDictionary<string, string> data)
        {
            this.Data = data;
        }
    }
}
