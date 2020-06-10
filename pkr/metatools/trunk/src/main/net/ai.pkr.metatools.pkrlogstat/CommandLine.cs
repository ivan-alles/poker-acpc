/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.metatools.pkrlogstat
{
    /// <summary>
    /// Command line. 
    /// It is recommended to use only one of create- arguments at a time.
    /// </summary>
    public class CommandLine : StandardCmdLine
    {
        #region Commands

        [Argument(ArgumentType.AtMostOnce, LongName = "show-report-help", ShortName = "c",
        DefaultValue = false, HelpText = "Show the help of the report class and exit.")]
        public bool ShowReportHelp;

        [Argument(ArgumentType.AtMostOnce, LongName = "count-games", ShortName = "c",
        DefaultValue = false, HelpText = "Count games in log file(s).")]
        public bool CountGames;

        [Argument(ArgumentType.AtMostOnce, LongName = "total-result", ShortName = "t",
        DefaultValue = true, HelpText = "Show total result for all sessions.")]
        public bool TotalResult;

        [Argument(ArgumentType.AtMostOnce, LongName = "session-result", ShortName = "s",
        DefaultValue = false, HelpText = "Show result for each session (summary for all repetitions).")]
        public bool SessionResult;

        [Argument(ArgumentType.AtMostOnce, LongName = "report-class",
        DefaultValue = "", HelpText = "Report class: full-type-name,assembly-name[;assembly-file]")]
        public string ReportClass;

        [Argument(ArgumentType.Multiple, LongName = "report-param",
        DefaultValue = new string[0], HelpText = "Parameters for report class: name=value")]
        public string[] ReportParameters = new string[0];

        #endregion

        #region File parameters

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "input-paths",
        HelpText = "Input files or dirs")]
        public string[] InputPaths = null;

        #endregion

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "verbose", ShortName = "v",
        DefaultValue = false, HelpText = "Verbose parsing.")]
        public bool Verbose;

        [Argument(ArgumentType.AtMostOnce, LongName = "time", ShortName = "",
        DefaultValue = false, HelpText = "Print run time.")]
        public bool Time;

        [Argument(ArgumentType.AtMostOnce, LongName = "incl-files", ShortName = "",
        DefaultValue = "\\.log$", HelpText = "Include files matching regex pattern.")]
        public string IncludeFiles = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "output", ShortName = "o",
        DefaultValue = null, HelpText = "Report output file. Default: stdout.")]
        public string Output = null;

        [Argument(ArgumentType.AtMostOnce, LongName = "game-limit", ShortName = "",
        DefaultValue = int.MaxValue, HelpText = "Analyze up to N games.")]
        public int GameLimit = int.MaxValue;

        #endregion
    }
}