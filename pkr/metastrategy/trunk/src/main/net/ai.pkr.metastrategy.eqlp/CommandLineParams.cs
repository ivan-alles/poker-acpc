/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.metastrategy.eqlp
{
    [CommandLine(HelpText =
@"Solves equilibirum by LP.")]
    public class CommandLineParams : StandardCmdLine
    {

        [Argument(ArgumentType.AtMostOnce, LongName = "game-def", ShortName = "g",
        DefaultValue = "", HelpText = "Game definition file")]
        public PropString GameDef;

        [Argument(ArgumentType.AtMostOnce, LongName = "chance-tree",
        HelpText = "Chance tree. If not specified, the program tries to build it from the game definition.")]
        public PropString ChanceTree = null;

        [Argument(ArgumentType.AtMostOnce, LongName = "output", ShortName = "o",
        DefaultValue = ".", HelpText = "Output file, used as base name. Indexes of positions will be added to it.")]
        public PropString Output;

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;

        #endregion
    }
}