/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.metastrategy.pkrbr
{
    [CommandLine(HelpText =
@"Computes a best response for a heads-up game.")]
    public class CommandLineParams : StandardCmdLine
    {
        [Argument(ArgumentType.Required, ShortName = "", LongName = "chance-tree",
            HelpText = "Chance tree file.")]
        public PropString ChanceTree = "";

        [Argument(ArgumentType.Required, ShortName = "", LongName = "action-tree",
        HelpText = "Action tree file.")]
        public PropString ActionTree = null;

        [Argument(ArgumentType.Required, ShortName = "", LongName = "opp-strategy",
        HelpText = "Opponent stategy tree file.")]
        public PropString OppStrategy = null;

        [Argument(ArgumentType.Required, LongName = "hero-pos", ShortName = "",
            HelpText = "Position of the hero.")]
        public int HeroPosition = 0;

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;

        [Argument(ArgumentType.AtMostOnce, LongName = "diag-unmanaged-memory", ShortName = "",
        DefaultValue = false, HelpText = "Turns on diagnostics of unmanaged memory.")]
        public bool DiagUnmanagedMemory = false;


        #endregion
    }
}
