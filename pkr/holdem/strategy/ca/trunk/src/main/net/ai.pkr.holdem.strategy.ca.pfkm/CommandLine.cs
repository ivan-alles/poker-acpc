/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.holdem.strategy.ca.hecamcgen
{
    /// <summary>
    /// Command line. 
    /// </summary>
    [CommandLine(HelpText = "Preflop bucketing by k-means.")]
    class CommandLine : StandardCmdLine
    {

        [Argument(ArgumentType.AtMostOnce, LongName = "centers-count", ShortName = "k",
        DefaultValue = 10, HelpText = "Number of clusters")]
        public int K = 1;

        [Argument(ArgumentType.AtMostOnce, LongName = "dim", ShortName="",
        DefaultValue = 1, HelpText = "Dimension")]
        public int Dim = 1;

        [Argument(ArgumentType.AtMostOnce, LongName = "stages", ShortName="",
        DefaultValue = 100, HelpText = "Stages count")]
        public int Stages = 100;

        [Argument(ArgumentType.AtMostOnce, LongName = "use-pocket-counts", ShortName="",
        DefaultValue = true, HelpText = "If true, pass to k-means the data according to the number of pockets in a pocket kind (for example 6 times for AA)")]
        public bool UsePocketCounts = true;

        [Argument(ArgumentType.AtMostOnce, LongName = "skip-pocket-names", ShortName = "",
        DefaultValue = false, HelpText = "If true, assumes that the first column in the input file contains pocket names and skips it.")]
        public bool SkipPocketNames = false;

        [DefaultArgument(ArgumentType.Required, LongName = "input file",
        HelpText = "Input file with 169 vectors of given dimension, one per line, sorted in HePocketKind order.")]
        public PropString InputFile = "";
        
        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        #endregion
    }
}