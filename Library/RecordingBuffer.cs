using System;
using System.Collections.Generic;

namespace NCorder
{
    // Holds audio samples.
    public class RecordingBuffer
    {
        public IList<Sample> Samples { get; internal set; }

        public NAudio.Wave.WaveFormat WaveFormat { get; set; }

        public void AddSample(Sample sample)
        {
            Samples.Add(sample);
        }

        public void ClearSamples()
        {
            Samples.Clear();
        }

        public RecordingBuffer()
        {
            Samples = new List<Sample>();
        }
    }
}