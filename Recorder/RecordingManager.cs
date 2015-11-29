using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCorder.Recorder
{
    class RecordingManager
    {
        private bool Stopped { get; set; }

        public void StartProcessing(IList<object> workItems)
        {
            new Thread(() => 
            {
                Thread.CurrentThread.IsBackground = true;
                ProcessWorkItems(workItems);
            }).Start();
        }

        public void StopProcessing()
        {
            Stopped = true;
        }

        object GetSample()
        {
            object result = default(object);
            return result;
        }

        bool IsSilence(object sample)
        {
            bool result = default(bool);
            return result;
        }

        bool IsStopped()
        {
            return Stopped;
        }

        public void ProcessWorkItems(IList<object> workItems)
        {
            Stopped = false;
            while (!Stopped && workItems.Count > 0)
            {
                var item = workItems.First();
                // 1.) load Url

                // N.) get track name
                string trackName = "default";

                // 2.) begin recording
                SampleSession<object> session = new SampleSession<object>();
                session.DoSampling(GetSample, IsSilence, IsStopped);
                Segment<object> track = session.FindLikelyTrack();

                // 3.) load neutral Url

                // N.) post-processing?

                // 4.) Save The Track

                SaveTrack(track, trackName);
                workItems.RemoveAt(0);
            }
        }

        private void SaveTrack(Segment<object> track, string trackName)
        {

        }
    }
}
