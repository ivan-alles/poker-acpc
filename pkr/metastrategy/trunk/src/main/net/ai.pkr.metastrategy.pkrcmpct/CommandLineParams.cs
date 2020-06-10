/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.metastrategy.pkrcmpct
{
    [CommandLine(HelpText = 
@"Compares 2 chance trees.")]
    public class CommandLineParams : StandardCmdLine
    {

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "chance-trees",
        HelpText = "Chance trees.")]
        public string[] ChanceTrees = null;

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "allow-diff-struct", ShortName = "s",
        DefaultValue = false, HelpText = "Allows to compare trees with different structure.")]
        public bool AllowDifferentStructure;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;
        
        #endregion
    }
}