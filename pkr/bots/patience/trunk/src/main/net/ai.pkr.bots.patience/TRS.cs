/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace ai.pkr.bots.patience
{
    // Trace switches. 
    // Todo: remove.
    class TRS
    {
        internal static TraceSwitch Default = new TraceSwitch("ai.pkr.bots.patience.TRS.Default", "Default trace for ai.pkr.bots.patience namespace");
        internal static BooleanSwitch OppBucketDistros = new BooleanSwitch("ai.pkr.bots.patience.TRS.OppBucketDistros", "Trace opponent bucket distributions");
    }
}
