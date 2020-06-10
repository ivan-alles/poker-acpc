/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.metastrategy.pkranct
{
    [CommandLine(HelpText = 
@"Analyze a chance tree.")]
    public class CommandLineParams : StandardCmdLine
    {

        [DefaultArgument(ArgumentType.Required, LongName = "input-paths",
        HelpText = "Chance tree.")]
        public string ChanceTree = null;

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "verify", ShortName = "",
        DefaultValue = true, HelpText = "Verify tree.")]
        public bool Verify;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;
        
        #endregion
    }
}