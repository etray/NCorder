using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder.Recorder
{
    //
    // A collection of contiguous samples
    //
    class Segment<SampleType>
    {
        public IList<SampleType> Samples { get; set; }

        public Segment()
        {
            Samples = new List<SampleType>();
        }

        public void AddSample(SampleType sample)
        {
            Samples.Add(sample);
        }
    }
}
