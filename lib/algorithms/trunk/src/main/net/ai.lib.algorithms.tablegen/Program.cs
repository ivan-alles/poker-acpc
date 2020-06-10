/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.tablegen
{
    class Program
    {
        static void Main(string[] args)
        {
            LutBitCountGenerator bc = new LutBitCountGenerator();
            bc.Generate("LutBitCount.cs");
        }
    }
}
