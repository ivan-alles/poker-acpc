/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.holdem.strategy.core.hecainfo
{
    /// <summary>
    /// Command line. 
    /// </summary>
    [CommandLine(HelpText = "Prints HS and SD for given hands.")]
    class CommandLine : StandardCmdLine
    {
        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        [DefaultArgument(ArgumentType.Required | ArgumentType.Multiple, LongName = "hand", ShortName = "", 
            HelpText = "A hand (cards without separators: AcAh7d5c3d).")]
        public string[] Hands = null;
    }
}