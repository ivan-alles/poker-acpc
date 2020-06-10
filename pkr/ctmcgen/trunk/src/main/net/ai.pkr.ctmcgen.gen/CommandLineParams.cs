/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.ctmcgen.gen
{
    [CommandLine(HelpText =
    @"Generate intermediate chance tree data by MC sampling.")]
    public class CommandLineParams : StandardCmdLine
    {
        [Argument(ArgumentType.Required, LongName = "game-def", ShortName = "g",
        DefaultValue = "", HelpText = "Game definition file")]
        public PropString GameDef;

        [Argument(ArgumentType.AtMostOnce, LongName = "output-dir", ShortName = "o",
        DefaultValue = ".", HelpText = "Output directory. Uniquely named files will be created there.")]
        public PropString Output;

        [Argument(ArgumentType.AtMostOnce, LongName = "samples-count", 
        DefaultValue = 1000000, HelpText = "Number of samples. For equal abstractions one MC sample updates multiple nodes in the chance tree.")]
        public int SamplesCount;

        [Argument(ArgumentType.AtMostOnce, LongName = "runs-count",
        DefaultValue = -1, HelpText = "Number of runs. A non-positive number: unlimited (can be stopped by pressing 'q').")]
        public int RunsCount;

        [DefaultArgument(ArgumentType.Multiple, LongName = "chance-abstraction-props",
        DefaultValue = new string[0], HelpText = "Chance abstraction property file. If the same file is used for all absractions, they are considered equal.")]
        public PropString[] ChanceAbstractionFiles;

        [Argument(ArgumentType.AtMostOnce, LongName = "add-ca-names",
        DefaultValue = true, HelpText = "Add names of CAs to the output file names.")]
        public bool AddCaNames;


        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;
    }
}