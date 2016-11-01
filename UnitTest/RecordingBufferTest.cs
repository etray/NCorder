using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NCorder;

namespace UnitTest
{
    [TestClass]
    public class RecordingBufferTest
    {
        [TestMethod]
        public void CreateTest()
        {
            RecordingBuffer buffer = new RecordingBuffer();
            Assert.AreNotEqual(null, buffer.Samples);
            Assert.AreEqual(0, buffer.Samples.Count);
        }

        [TestMethod]
        public void ClearBufferTest()
        {
            RecordingBuffer buffer = new RecordingBuffer();
            buffer.AddSample(new Sample() {
                Id  = 1
            });
            buffer.AddSample(new Sample()
            {
                Id = 2
            });
            Assert.AreNotEqual(0, buffer.Samples.Count);
            buffer.ClearSamples();
            Assert.AreEqual(0, buffer.Samples.Count);
        }

        [TestMethod]
        public void AddSampleTest()
        {
            RecordingBuffer buffer = new RecordingBuffer();

            Assert.AreEqual(0, buffer.Samples.Count);
            
            buffer.AddSample(new Sample()
            {
                Id = 1
            });

            Assert.AreEqual(1, buffer.Samples.Count);

            buffer.AddSample(new Sample()
            {
                Id = 2
            });

            Assert.AreEqual(2, buffer.Samples.Count);

            buffer.AddSample(new Sample()
            {
                Id = 3
            });

            Assert.AreEqual(3, buffer.Samples.Count);

            buffer.ClearSamples();

            Assert.AreEqual(0, buffer.Samples.Count);

            buffer.AddSample(new Sample()
            {
                Id = 4
            });

            Assert.AreEqual(1, buffer.Samples.Count);
        }
    }
}
