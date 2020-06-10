/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using ai.lib.utils;
using System.Threading;

namespace ai.pkr.bifaces.agentgui_exe
{
    /// <summary>
    /// Command line. 
    /// </summary>
    public class CommandLine : StandardCmdLine
    {
        [Argument(ArgumentType.AtMostOnce | ArgumentType.Required, LongName = "bot-class", ShortName="b",
        HelpText = "Bot class: full-type-name,assembly-name[;assembly-file]")]
        public PropString BotClass;

        [Argument(ArgumentType.AtMostOnce | ArgumentType.Required, LongName = "creation-params", 
        HelpText = "Name of property file with creation parameters")]
        public PropString CreationParametersFileName;

        [Argument(ArgumentType.AtMostOnce, LongName = "game-def", ShortName = "",
        DefaultValue = "", HelpText = "Game definition file")]
        public PropString GameDef = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;
    }
}
