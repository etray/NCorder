using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder.Recorder
{
    // 
    // records a list of segments delimited by periods of audio silence
    //
    class SampleSession<SampleType>
    {
        private static int DEFAULT_SAMPLE_RATE = 44100;
        private static int DEFAULT_END_SEGMENT_TRIGGER_SECONDS = 2;
        private static int DEFAULT_END_SESSION_TRIGGER_SECONDS = 10;
        private static int MAX_SEGMENTS = 5;
        private static int MAX_SESSION_LENGTH = 10 * 60;

        public IList<Segment<SampleType>> Segments { get; set; }
        Segment<SampleType> CurrentSegment {get; set;}
        int SampleRate { get; set; }
        int EndSessionCountDown { get; set; }
        int EndSampleSilenceCount { get; set; }
        int EndSessionSilenceCount { get; set; }
        bool Sampling { get; set;}

        public SampleSession()
        {
            Segments = new List<Segment<SampleType>>();
            EndSampleSilenceCount = 0;
            EndSessionSilenceCount = 0;
            EndSessionCountDown = MAX_SESSION_LENGTH * DEFAULT_SAMPLE_RATE;
            Sampling = false;
            CurrentSegment = null;
        }

        public delegate SampleType SamplerDelegate();
        public delegate bool IsSilenceDelegate(SampleType value);
        public delegate bool IsStoppedDelegate();

        public void DoSampling(SamplerDelegate GetSample, IsSilenceDelegate IsSilence, IsStoppedDelegate IsStopped)
        {
            while (true)
            {   
                // stop immediately without further action if caller requests it.
                if (IsStopped())
                {
                    return;
                }

                // limit the number of segments as a sanity control
                if (Segments.Count >= MAX_SEGMENTS)
                {
                    throw new Exception("Limit of " + MAX_SEGMENTS + " segments exceeded");
                }

                // limit the length of a session as a sanity control
                EndSessionCountDown--;
                if (EndSessionCountDown <= 0)
                {
                    throw new Exception("Session limit of " + MAX_SESSION_LENGTH + " seconds exceeded.");
                }
                
                SampleType sample = GetSample();

                bool silence = IsSilence(sample);

                if (silence)
                {
                    EndSessionSilenceCount++;
                    EndSampleSilenceCount++;
                }
                else
                {
                    EndSessionSilenceCount = 0;
                    EndSampleSilenceCount = 0;
                }

                if (EndSampleSilenceCount >= DEFAULT_SAMPLE_RATE * DEFAULT_END_SEGMENT_TRIGGER_SECONDS)
                {
                    EndCurrentSegment();
                    continue;
                }

                if (EndSessionSilenceCount >= DEFAULT_SAMPLE_RATE * DEFAULT_END_SESSION_TRIGGER_SECONDS)
                {
                    EndCurrentSegment();
                    return;
                }

                if (!Sampling && !silence)
                {
                    CurrentSegment = new Segment<SampleType>();
                    Sampling = true;
                }

                if (Sampling)
                {
                    CurrentSegment.AddSample(sample);
                }
            }
        }

        private void EndCurrentSegment()
        {
            if (CurrentSegment != null)
            {
                Segments.Add(CurrentSegment);
            }

            CurrentSegment = null;
            Sampling = false;           
        }

        public Segment<SampleType> FindLikelyTrack()
        {
            Segment<SampleType> result = null;
            // Longest segment is probably the one we want
            result = Segments.OrderByDescending(x => x.Samples.Count).First();
            return result;
        }
    }
}
