/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace ai.pkr.bots.neytiri
{
    // Trace switches. 
    // Todo: remove.
    class TRS
    {
        internal static TraceSwitch Default = new TraceSwitch("ai.pkr.bots.neytiri.TRS.Default", "Default trace for ai.pkr.bots.neytiri namespace");
        internal static BooleanSwitch OppBucketDistros = new BooleanSwitch("ai.pkr.bots.neytiri.TRS.OppBucketDistros", "Trace opponent bucket distributions");
    }
}
