/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using ai.lib.utils;


namespace ai.pkr.fictpl.fictpl
{
    [CommandLine(HelpText =
@"Solve eq by fictitious play.")]
    public class CommandLineParams : StandardCmdLine
    {
        [Argument(ArgumentType.Required, ShortName = "", LongName = "chance-tree",
            HelpText = "Chance tree file.")]
        public PropString ChanceTree = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "equal-ca", ShortName = "",
        DefaultValue = false, HelpText = "If true, optimize for equal chance abstractions.")]
        public bool EqualCa;

        [Argument(ArgumentType.Required, ShortName = "", LongName = "action-tree",
        HelpText = "Action tree file.")]
        public PropString ActionTree = null;


        [Argument(ArgumentType.AtMostOnce, ShortName = "o", LongName = "output",
        DefaultValue = "fictpl", HelpText = "Path for snapshots with results.")]
        public PropString Output = "fictpl";

        [Argument(ArgumentType.AtMostOnce, ShortName = "", LongName = "snapshot-count",
        DefaultValue = 2, HelpText = "Number of snapshots.")]
        public int SnapshotCount = 2;

        [Argument(ArgumentType.AtMostOnce, ShortName = "", LongName = "epsilons",
        DefaultValue = "0.1", HelpText = "Comma-separated list of epsilons. When any of them is reached, a snapshot will be made.")]
        public PropString Epsilons = "";

        [Argument(ArgumentType.AtMostOnce, ShortName = "", LongName = "snapshot-time",
        DefaultValue = 720, HelpText = "Time in minutes. When time since last snapshot is greater that this value, a new snapshot will be made.")]
        public int SnapshotTime = 1;

        [Argument(ArgumentType.AtMostOnce, ShortName = "", LongName = "epsilon-log-threshold",
        DefaultValue = "0.9", HelpText = "Epsilon log threshold [0..1).")]
        public string EpsilonLogThreshold = "";

        [Argument(ArgumentType.AtMostOnce, ShortName = "", LongName = "iteration-verbosity",
        DefaultValue = 0, HelpText = "Iteration verbosity.")]
        public int IterationVerbosity = 0;

        [Argument(ArgumentType.AtMostOnce, ShortName = "", LongName = "thread-count",
        DefaultValue = 0, HelpText = "Number threads in the thread pool.")]
        public int ThreadCount = 0;


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