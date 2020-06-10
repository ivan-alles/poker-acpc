/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.holdem.strategy.core.hecainfo
{
    /// <summary>
    /// Command line. 
    /// </summary>
    [CommandLine(HelpText = "Shows info about HE chance abstraction.")]
    class CommandLine : StandardCmdLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Required | ArgumentType.Multiple , LongName = "ca-props-file", HelpText = "Chance abstraction properties file")] 
        public string [] PropsFiles = null;

        #endregion

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        [Argument(ArgumentType.AtMostOnce, LongName = "preflop-ranges", ShortName = "",
        DefaultValue = true, HelpText = "Shows preflop ranges.")]
        public bool PreflopRanges = true;

        [Argument(ArgumentType.Multiple, LongName = "hand", ShortName = "",
            DefaultValue = null, HelpText = "A hand (cards without separators: AcAh7d5c3d). If specified, the program shows abstract cards for each round for the hand.")]
        public string[] Hands = null;

        #endregion
    }
}