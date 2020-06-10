/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.holdem.strategy.ca.hecamcgen
{
    /// <summary>
    /// Command line. 
    /// </summary>
    [CommandLine(HelpText = "Generates a HE CA files by monte-carlo sampling.")]
    class CommandLine : StandardCmdLine
    {

        [Argument(ArgumentType.AtMostOnce, LongName = "samples-count",
        DefaultValue = "1000,1000,1000,1000", HelpText = "Comma-separated list of samples count for each round.")]
        public string SamplesCount = "";


        [DefaultArgument(ArgumentType.Required, LongName = "chance-abstraction-props",
        DefaultValue = new string[0], HelpText = "Chance abstraction property file.")]
        public PropString ChanceAbstractionFile = "";


        [Argument(ArgumentType.AtMostOnce, LongName = "rng-seed",
        DefaultValue = 0, HelpText = "RNG seed. If 0, a time-based seed is used.")]
        public int RngSeed = 0;

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        #endregion
    }
}