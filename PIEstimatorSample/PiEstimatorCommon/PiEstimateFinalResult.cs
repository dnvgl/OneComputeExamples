using System;
using System.Collections.Generic;
using System.Text;

namespace PiEstimatorCommon
{
    public class PiEstimateFinalResult
    {
        public long TotalNumberOfSamples { get; set; }
        public long TotalNumberWithinUnitCircle { get; set; }
        public double PI { get; set; }

        public double StandardDeviation { get; set; }
    }
}
