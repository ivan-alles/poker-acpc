/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.kmeans.nunit;

namespace ai.lib.kmeans.nunit_dbg
{
    class Program
    {
        static void Main(string[] args)
        {
            Kml_Test t = new Kml_Test();
            t.Test_Hybrid();
        }
    }
}
