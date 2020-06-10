/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.core;

namespace ai.pkr.holdem.strategy.hand_value.lut_gen
{
    class Program
    {
        static void Main(string[] args)
        {
            PocketEquity.Precalculate();
        }
    }
}
