/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;

namespace ai.dev.tools.benchmark
{
    class CommandLineParams
    {
        #region Options

        //[Argument(ArgumentType.AtMostOnce, LongName = "output", ShortName = "o",
        //    DefaultValue = "", HelpText = "Output file, if omitted, prints to console.")]
        //public string Output;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        [Argument(ArgumentType.AtMostOnce, LongName = "range-begin", ShortName = "b",
        DefaultValue = 1, HelpText = "Begin value for parameter range.")]
        public int RangeBegin = 1;

        [Argument(ArgumentType.AtMostOnce, LongName = "range-end", ShortName = "e",
        DefaultValue = -1, HelpText = "End value for parameter range.")]
        public int RangeEnd = -1;

        [Argument(ArgumentType.AtMostOnce, LongName = "rep-count", ShortName = "r",
        DefaultValue = 1000UL, HelpText = "Number of repetitions.")]
        public UInt64 RepetitionsCount = 1000UL;

        [DefaultArgument(ArgumentType.AtMostOnce, LongName = "test-name",
        DefaultValue = "*", HelpText = "Test name (*, exh-mem, thread-perf.")]
        public string TestName = "*";

        #endregion
    }
}
