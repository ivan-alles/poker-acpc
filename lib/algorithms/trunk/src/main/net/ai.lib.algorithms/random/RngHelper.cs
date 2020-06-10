/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.random
{
    internal static class RngHelper
    {
        public static Random CreateDefaultRng()
        {
            return new System.Random();
        }

        public static Random CreateDefaultRng(int seed)
        {
            return new System.Random(seed);
        }
    }
}
