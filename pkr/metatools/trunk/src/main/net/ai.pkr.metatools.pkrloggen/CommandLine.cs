/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;

namespace ai.pkr.metatools.pkrloggen
{
    /// <summary>
    /// Command line. 
    /// </summary>
    class CommandLine : StandardCmdLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Required, LongName = "config-file",
        HelpText = "Config file (game log with deal actions only, use ? for random cards)")]
        public string ConfigFile = "";

        #endregion

        #region Other parameters

        [Argument(ArgumentType.Required, LongName = "deck", ShortName = "",
        HelpText = "Deck descriptor file (expandable).")]
        public string DeckDescriptorFile = null; 

        [Argument(ArgumentType.AtMostOnce, LongName = "repeat", ShortName = "r",
        DefaultValue = 1, HelpText = "Number of repetitions.")]
        public int Repeat = 1;

        [Argument(ArgumentType.AtMostOnce, LongName = "enum-count", ShortName = "",
        DefaultValue = false, HelpText = "Count enumerated combinations and exit.")]
        public bool EnumCount = false;

        [Argument(ArgumentType.AtMostOnce, LongName = "rng-seed", ShortName = "",
        DefaultValue = 0, HelpText = "RNG seed (0) - use random seed.")]
        public int RngSeed = 0;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        #endregion
    }
}
