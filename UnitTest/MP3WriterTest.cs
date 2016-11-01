using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCorder;
using NAudio.Wave;
using System.IO;
using System.Runtime.InteropServices;

namespace UnitTest
{
    /// <summary>
    /// Summary description for MP3WriterTest
    /// </summary>
    [TestClass]
    public class MP3WriterTest
    {
        WaveFormat WaveFormat { get; set; }

        IList<Sample> Samples { get; set; }

        public MP3WriterTest()
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            Samples = new List<Sample>();
            for (int i = 0; i < 100; i++)
            {
                Samples.Add(new Sample() { 
                    Id = i,
                    Value = new byte[2]
                });
            }
        }

        [TestMethod]
        public void TestMP3Writer()
        {
            // private MemoryStream ConvertToWave(IList<Sample> samples, WaveFormat waveFormat)
            // private void WriteMP3(MemoryStream waveMemoryStream, string outputFile)
            MP3Writer writer = new MP3Writer();
            PrivateObject privateObject = new PrivateObject(writer);
            string outFile = Path.GetTempFileName();
            File.Delete(outFile); // this method creates the file, but that won't happen in real life.
            object result = privateObject.Invoke("ConvertToWave", null, new object[] { Samples, WaveFormat} );

            Assert.AreEqual(result.GetType(), typeof(byte[]));

            privateObject.Invoke("WriteMP3", null, new object[] { result, outFile });

            Assert.AreEqual(File.Exists(outFile), true);
            Assert.AreEqual(new FileInfo(outFile).Length > 0, true);

            // Clean up temp file.
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
        }

        [TestMethod]
        public void GetAlternatePathTest()
        {
            MP3Writer writer = new MP3Writer();
            PrivateObject privateObject = new PrivateObject(writer);

            string outFile = Path.GetTempFileName() + ".mp3";
            File.Delete(outFile); // this method creates the file, but that won't happen in real life.

            string alternateFile = (string)privateObject.Invoke("GetAlternatePath", null, new object[] { outFile });
            Assert.AreEqual(true, alternateFile == outFile);

            System.IO.File.WriteAllText(outFile, "GetAlternatePathTest()");
            Assert.AreEqual(true, outFile.Length > 2);

            alternateFile = (string)privateObject.Invoke("GetAlternatePath", null, new object[] { outFile });
            Assert.AreNotEqual(true, string.IsNullOrWhiteSpace(alternateFile));
            Assert.AreEqual(true, alternateFile.Length == outFile.Length + 2);
            Assert.AreNotEqual(alternateFile, outFile);

            File.Delete(outFile);
        }

        // 
        // libmp3lame does not appear to get cleanly unloaded, so need to perform this voodoo in order to avoid
        // access denied errors if vstest engine stays running and you rebuild ...
        //
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            foreach (System.Diagnostics.ProcessModule mod in System.Diagnostics.Process.GetCurrentProcess().Modules)
            {
                if (mod.FileName.Contains("lame"))
                {
                    FreeLibrary(mod.BaseAddress);
                }
            }
        }
    }
}
