/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.metatools.pkrserver
{
    /// <summary>
    /// Command line. 
    /// </summary>
    class CommandLine : StandardCmdLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "session-suit-cfg",
            HelpText = "Session suite config file(s).")] 
        public string[] SessionSuites = null;

        #endregion

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "menu", ShortName = "m",
            DefaultValue = true, HelpText = "Console menu on.")]
        public bool Menu = true;

        [Argument(ArgumentType.AtMostOnce, LongName = "remote-timout", ShortName = "t",
            DefaultValue = 60, HelpText = "Timeout (s) to wait for remote players before each session suite.")]
        public int RemoteTimeout = 60;

        [Argument(ArgumentType.AtMostOnce, LongName = "port", ShortName = "p",
            DefaultValue = 9001, HelpText = "TCP port.")]
        public int Port = 9001;

        [Argument(ArgumentType.AtMostOnce, LongName = "log-dir", ShortName = "l",
            DefaultValue = "", HelpText = "Logs directory.")]
        public string LogDir = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "on-session-suite-end", ShortName = "",
        DefaultValue = "", HelpText = "Runs a program on session suite end from the current directory. Parameters: log file.")]
        public string OnSessionSuiteEnd = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        #endregion
    }
}