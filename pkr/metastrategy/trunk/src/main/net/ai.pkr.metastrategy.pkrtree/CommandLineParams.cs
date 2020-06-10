/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using ai.lib.utils;

namespace ai.pkr.metastrategy.pkrtree
{
    [CommandLine(HelpText = 
@"Creates or reads a poker tree and stores it in the specified format.
To create a tree specify game definition.
To read existing tree specify input file.")]
    public class CommandLineParams : StandardCmdLine
    {
        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "tree", ShortName = "",
            DefaultValue = "action", HelpText = "Tree kind: action, chance, chance-player, strategy.")]
        public string TreeKind;

        [Argument(ArgumentType.AtMostOnce, LongName = "game-def", ShortName = "g",
            DefaultValue = "", HelpText = "Game definition file")]
        public PropString GameDef;

        [Argument(ArgumentType.AtMostOnce, LongName = "input", ShortName = "i",
            DefaultValue = "", HelpText = "Input *.dat file, game definition is ignored.")]
        public PropString Input;

        [Argument(ArgumentType.AtMostOnce, LongName = "root", ShortName = "p",
            DefaultValue = 0, HelpText = "Root node for input tree (supported only for visualization of strategy tree).")]
        public int Root = 0;

        [Argument(ArgumentType.AtMostOnce, LongName = "position", ShortName = "p",
            DefaultValue = 0, HelpText = "Hero position for player trees.")]
        public int Position = 0;

        [Argument(ArgumentType.AtMostOnce, LongName = "max-round", ShortName = "",
        DefaultValue = int.MaxValue, HelpText = "Max round to show.")]
        public int MaxRound = 0;

        [Argument(ArgumentType.AtMostOnce, LongName = "match-path", ShortName = "",
        DefaultValue = null, HelpText = "Show only nodes with this path, like //0p0.5/1p0/0d3")]
        public string MatchPath = null;

        [Argument(ArgumentType.Required, LongName = "output", ShortName = "o",
            DefaultValue = "", HelpText = "Output file, extension is used to autodetect format (gv, xml, dat, txt).")]
        public PropString Output;

        [Argument(ArgumentType.Multiple, LongName = "show-expr", ShortName = "",
            DefaultValue = new string[0], HelpText = "Show expression in a gv or xml tree. Format <expr>[;<fmt>]: s[d].Tree.Nodes[s[d].Node].Round;\nr={1}")]
        public string[] ShowExpr = new string[0];

        [Argument(ArgumentType.Multiple, LongName = "clear-expr", ShortName = "",
        DefaultValue = false, HelpText = "Clear default show expressions")]
        public bool ClearExpr = false;

        [Argument(ArgumentType.AtMostOnce, LongName = "debugger-launch", ShortName = "",
        DefaultValue = false, HelpText = "Launch debugger.")]
        public bool DebuggerLaunch;
        
        #endregion
    }
}