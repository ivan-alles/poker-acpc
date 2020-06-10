/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.holdem.strategy.hs.lut_gen
{
    class CommandLineParams : StandardCmdLine
    {
        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        [Argument(ArgumentType.AtMostOnce, LongName = "output-dir", ShortName = "o",
        DefaultValue = "${bds.DataDir}", HelpText = "Output directory.")]
        public PropString OutputDir = "";
    }
}
