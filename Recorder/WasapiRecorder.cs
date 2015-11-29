using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder.Recorder
{
    public class WasapiMP3Recorder : IDisposable
    {
        private IWaveIn waveIn;
        private WaveFileWriter writer;
        private string outputFile;
        private string tmpFile;

        public WasapiMP3Recorder(string mp3Output)
        {
            outputFile = mp3Output;
            tmpFile = Path.GetTempFileName();
        }

        public void StartRecording()
        {
            waveIn = new WasapiLoopbackCapture();
            writer = new WaveFileWriter(tmpFile, waveIn.WaveFormat);

            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.StartRecording();
        }

        public void StopRecording()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
            }
        }

        public static void ConvertWavFileToMp3(string inputFile, string outputFile)
        {
            using (WaveFileReader reader = new WaveFileReader(inputFile))
            {
                using (LameMP3FileWriter writer = new LameMP3FileWriter(outputFile, reader.WaveFormat, LAMEPreset.VBR_90))
                {
                    reader.CopyTo(writer);
                }
            }
        }

        private void Cleanup()
        {
            if (waveIn != null)
            {
                waveIn.Dispose();
                waveIn = null;
            }
            if (writer != null)
            {
                writer.Close();
                writer = null;

                ConvertWavFileToMp3(tmpFile, outputFile);

                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            Cleanup();
        }

        public void Dispose()
        {
        }
    }
}
