using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bucketizer_proto
{
    interface IRules
    {
        string[] Cards
        {
            get;
        }

        int[] CardCounts
        {
            get;
        }

        double Equity(int[] range1, int[] range2);
    }
}
