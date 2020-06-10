/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.ctmcgen.conv
{
    [CommandLine(HelpText =
@"Convert a ctmcgen file to a chance tree.")]
    public class CommandLineParams : StandardCmdLine
    {

        [DefaultArgument(ArgumentType.Required | ArgumentType.Required, LongName = "input",
        HelpText = "Input file.")]
        public string Input = null;

        [Argument(ArgumentType.AtMostOnce, ShortName = "o", LongName = "output",
        HelpText = "Output file.")]
        public string Output = null;

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;

        #endregion
    }
}