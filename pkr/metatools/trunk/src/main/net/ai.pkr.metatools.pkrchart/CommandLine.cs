/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;

namespace ai.pkr.metatools.pkrchart
{
    /// <summary>
    /// Command line. 
    /// </summary>
    class CommandLine : StandardCmdLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "input-paths",
        HelpText = "Input files or dirs")]
        public string[] InputPaths = null;

        #endregion

        #region Options

        [Argument(ArgumentType.Multiple, LongName = "curve",
            DefaultValue = new string[0],
            HelpText = "Curve descriptor in form <PlayerName,Position>. If position is omitted, all positions will be shown. If no curve descriptor are specified, they will be autocreated for each player.")]
        public string[] CurveDescriptors = new string[0];

        [Argument(ArgumentType.AtMostOnce, LongName = "incl-files", ShortName = "",
        DefaultValue = "\\.log$", HelpText = "Include files matching regex pattern.")]
        public string IncludeFiles = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "compare", ShortName = "",
        DefaultValue = false, HelpText = "Compare input paths instead of showing the total.")]
        public bool Compare = false;

        [Argument(ArgumentType.AtMostOnce, LongName = "curve-fitting", ShortName = "",
        DefaultValue = false, HelpText = "Fit the data by a straight line and show it.")]
        public bool ShowCurveFitting = false;

        #endregion
    }
}
