/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ai.pkr.bots.neytiri
{
    public class Buckets
    {
        public Buckets()
        {
        }

        public Buckets(int size, int initialCount)
        {
            Counts = new int[size];
            for(int i = 0; i < Counts.Length; ++i)
            {
                Counts[i] = initialCount;
                Total += initialCount;
            }
        }

        public void Update(int bucket, int count)
        {
            Counts[bucket] += count;
            Total += count;
        }

        public double GetBucketProbability(int bucket)
        {
            if (Total == 0)
                return 1.0/Counts.Length;
            return (double)Counts[bucket] / Total;
        }

        [XmlArrayItem("c")]
        public int[] Counts;

        public int Total;
    }
}
