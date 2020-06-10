/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.holdem.strategy.ahvo.lutgen
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int r = 1; r <= 3; ++r)
            {
                AHVO.Precalculate(r);
            }
        }
    }
}
