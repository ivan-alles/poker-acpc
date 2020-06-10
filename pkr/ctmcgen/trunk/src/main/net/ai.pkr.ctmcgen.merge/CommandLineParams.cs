/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.ctmcgen.merge
{
    [CommandLine(HelpText =
@"Merges intermediate chance tree data.")]
    public class CommandLineParams : StandardCmdLine
    {

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "input-paths",
        HelpText = "Input files or dirs. Directories will be processed recursively.")]
        public string[] InputPaths = null;

        #region Options

        [Argument(ArgumentType.AtMostOnce, ShortName = "o", LongName = "output",
        HelpText = "Output file. If it exists, it will be merged into, otherwise a new file will be created.")]
        public string Output = null;

        [Argument(ArgumentType.AtMostOnce, LongName = "incl-files", ShortName = "",
        DefaultValue = "\\.dat$", HelpText = "Include files matching regex pattern.")]
        public string IncludeFiles = "";
        
        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;

        #endregion
    }
}