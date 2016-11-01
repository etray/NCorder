using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCorder
{
    // Records audio samples asynchronously, and stores them in a buffer.
    public class Recorder : IDisposable
    {
        public long CurrentTrack { get; set; }
        public IList<string> Titles { get; set; }

        private static long _sequence = 0;
        public static long Sequence
        {
            get
            {
                _sequence++;
                return _sequence;
            }
        }
        public bool SilenceDetected { get; set; }
        public bool SoundDetected { get; set; }
        private bool KeepRecording { get; set; }
        public RecordingBuffer Buffer { get; set; }
        private WasapiCapture CaptureDevice { get; set; }
        public Thread RecordingThread { get; set; }

        public void Start()
        {
            this.KeepRecording = true;
            this.RecordingThread = new Thread(new ThreadStart(this.Run));
            this.RecordingThread.Start();
        }

        public void Stop()
        {
            if (this.RecordingThread != null && this.RecordingThread.IsAlive)
            {
                this.KeepRecording = false;
                this.RecordingThread.Join();
            }
        }

        public void Reset()
        {
            this.Buffer.ClearSamples();
        }

        private void Run()
        {
            this.Reset();
            StatusManager.Status("Recorder starting...");
            StatusManager.Indicator(true);
            this.CaptureDevice.StartRecording();
 	        while(this.KeepRecording)
            {
                Thread.Sleep(120);
            }
            StatusManager.Status("Recorder stopping...");
            this.CaptureDevice.StopRecording();
            StatusManager.Indicator(false);
        }

        private void SaveSample(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            Sample sample = new Sample()
            {
                Id = CurrentTrack,
                Value = new Byte[e.BytesRecorded]
            };

            Array.Copy(e.Buffer, sample.Value, e.BytesRecorded);

            this.SilenceDetected = this.IsSilent(sample.Value);
            this.SoundDetected = !this.SilenceDetected;

            this.Buffer.AddSample(sample);
        }

        public bool IsSilent(byte[] sample)
        {
            for (int i = 0; i < sample.Length; i++)
            {
                if (sample[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public Recorder()
        {
            this.SilenceDetected = true;
            this.SoundDetected = false;

            this.CaptureDevice = new WasapiLoopbackCapture();
            this.Buffer = new RecordingBuffer();
            this.Buffer.WaveFormat = this.CaptureDevice.WaveFormat;

            this.CaptureDevice.DataAvailable += this.SaveSample;
            this.CaptureDevice.RecordingStopped += this.RecordingStopped;
            this.KeepRecording = false;
        }

        public void SaveTrack(long trackNumber, string title)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), title + ".mp3");
            RecordingBuffer trackBuffer = this.PopTrack(trackNumber);
            MP3Writer writer = new MP3Writer();
            writer.WriteOutBuffer(trackBuffer, path);
        }

        private RecordingBuffer PopTrack(long trackNumber)
        {
            RecordingBuffer result = new RecordingBuffer();
            result.WaveFormat = this.Buffer.WaveFormat;
            var samples = this.Buffer.Samples;
            while (samples.Count > 0 && samples[0].Id <= trackNumber)
            {
                result.AddSample(samples[0]);
                samples.RemoveAt(0);
            }

            return result;
        }

        private void RecordingStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            StatusManager.Status("WSAPI Recorder stopped.");
        }

        public void Dispose()
        {
            this.Stop();
            this.CaptureDevice.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
