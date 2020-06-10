/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using ai.lib.utils;
using System.Threading;

namespace ai.pkr.acpc.server_adapter
{
    /// <summary>
    /// Command line. 
    /// </summary>
    public class CommandLine : StandardCmdLine
    {
        [Argument(ArgumentType.AtMostOnce | ArgumentType.Required, LongName = "bot-class", ShortName="b",
        HelpText = "Bot class: full-type-name,assembly-name[;assembly-file]")]
        public PropString BotClass;

        [Argument(ArgumentType.AtMostOnce | ArgumentType.Required, LongName = "server-addr", ShortName="a",
        HelpText = "Server address: host:port")]
        public PropString ServerAddress;

        [Argument(ArgumentType.AtMostOnce | ArgumentType.Required, LongName = "creation-params", 
        HelpText = "Name of property file with creation parameters")]
        public PropString CreationParametersFileName;

        [Argument(ArgumentType.AtMostOnce, LongName = "connect-timeout",
        DefaultValue = 600, HelpText = "Socket connection timeout in seconds")]
        public int ConnectTimeout;

        [Argument(ArgumentType.AtMostOnce, LongName = "verbose", ShortName = "v",
        DefaultValue = false, HelpText = "Verbose.")]
        public bool Verbose;

        [Argument(ArgumentType.AtMostOnce, LongName = "verbose-traffic", 
        DefaultValue = false, HelpText = "Prints all clinent and server messages).")]
        public bool VerboseTraffic;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;
    }
}
