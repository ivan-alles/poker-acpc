/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.metastrategy.pkranct
{
    [CommandLine(HelpText = 
@"Analyze a strategy tree.")]
    public class CommandLineParams : StandardCmdLine
    {

        [DefaultArgument(ArgumentType.Required, LongName = "input-path",
        HelpText = "Strategy tree.")]
        public string StrategyTree = null;

        [Argument(ArgumentType.Required, LongName = "hero-pos", ShortName = "",
        HelpText = "Position of the hero.")]
        public int HeroPosition = 0;

        [Argument(ArgumentType.AtMostOnce, LongName = "absolute", ShortName = "",
        DefaultValue = true, HelpText = "Is strategy absolute or conditional.")]
        public bool IsAbsolute;

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