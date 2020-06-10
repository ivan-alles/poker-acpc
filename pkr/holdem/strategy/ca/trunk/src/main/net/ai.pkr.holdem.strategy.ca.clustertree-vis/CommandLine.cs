/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.holdem.strategy.ca.clustertree_vis
{
    /// <summary>
    /// Command line. 
    /// </summary>
    [CommandLine(HelpText = "Converts a HE CA cluster tree to Graphviz file.")]
    class CommandLine : StandardCmdLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Required | ArgumentType.Multiple, LongName = "input", HelpText = "Range tree files")]
        public string[] InputFiles = null;

        

        #endregion

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "show-cards", ShortName = "",
        DefaultValue = true, HelpText = "Show cards dealt in each node.")]
        public bool ShowPath = true;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch = false;

        #endregion
    }
}