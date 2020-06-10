/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace ai.pkr.holdem.strategy.ca.pfkm
{
    class PocketData
    {
        public PocketData(int dim)
        {
            Value = new double[dim];
        }

        public double[] Value;
        public int Center;

        public void Print(TextWriter w, bool printCenter)
        {
            for (int i = 0; i < Value.Length; ++i)
            {
                w.Write("{0,-14} ", Value[i].ToString(CultureInfo.InvariantCulture));
            }
            if (printCenter)
            {
                w.Write("{0,-5} ", Center);
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("v: ( ");
            for (int i = 0; i < Value.Length; ++i)
            {
                sb.AppendFormat("{0:0.00000} ", Value[i]);
            }
            sb.AppendFormat(" ) c: {0}", Center);
            return sb.ToString();
        }
    }
}
