using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.CoreAudioApi;
using NCorder;
using NAudio.Wave;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace UnitTest 
{

    /// <summary>
    /// Summary description for RecorderTest
    /// </summary>
    [TestClass]
    public class RecorderTest
    {       
        [TestMethod]
        public void RecordTest()
        {
            Recorder recorder = new Recorder();
            PrivateObject privateObject = new PrivateObject(recorder);

            Assert.AreEqual(0, recorder.Buffer.Samples.Count);
            recorder.Start();
            for (int i = 0; i < 10; i++ )
            {
                Assert.AreEqual(recorder.Buffer.Samples.Count, i);
                privateObject.Invoke("SaveSample", null, new WaveInEventArgs(new byte[1], 1));
            }
            Assert.AreEqual(recorder.Buffer.Samples.Count, 10);
            recorder.Stop();

            recorder.Reset();
            Assert.AreEqual(recorder.Buffer.Samples.Count, 0);
        }

        [TestMethod]
        public void DisposeTest()
        {
            Recorder recorder = new Recorder();
            recorder.Start();
            while(!recorder.RecordingThread.ThreadState.Equals(System.Threading.ThreadState.Running))
            {
                Thread.Sleep(10);
            }
            recorder.Dispose();

            Assert.AreEqual(!recorder.RecordingThread.ThreadState.Equals(System.Threading.ThreadState.Running), true);
        }

        [TestMethod]
        public void TestIsSilent()
        {
            Recorder recorder = new Recorder();
            byte[] silent = new byte[] { 0,0,0,0,0,0,0,0,0 };
            byte[] notSilent = new byte[] { 9,9,9,9,9,9,9,9,9 };

            Assert.AreEqual(true, recorder.IsSilent(silent));
            Assert.AreEqual(false, recorder.IsSilent(notSilent));
        }

        [TestMethod]
        public void TestSaveTrack()
        {
            Recorder recorder = new Recorder();
            for (int i = 0; i < 50; i++)
            {
                recorder.Buffer.Samples.Add(new Sample() { Id = 100, Value = new byte[] { 10, 10, 10, 10, 10 } });
            }
            for (int i = 0; i < 50; i++)
            {
                recorder.Buffer.Samples.Add(new Sample() { Id = 200, Value = new byte[] { 20, 20, 20, 20, 20 } });
            }

            string destTrack = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "TestTrack" + ".mp3");;
            if (File.Exists(destTrack))
            {
                File.Delete(destTrack);
            }

            Assert.AreEqual(100, recorder.Buffer.Samples.Count);

            recorder.SaveTrack(100, "TestTrack");

            Assert.AreEqual(true, File.Exists(destTrack));
            
            Assert.AreEqual(50, recorder.Buffer.Samples.Count);
        }
    }
}
