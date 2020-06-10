/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.bots.neytiri.builder
{
    /// <summary>
    /// Command line. 
    /// It is recommended to use only one of create- arguments at a time.
    /// </summary>
    public class CommandLine
    {
        #region Commands

        [Argument(ArgumentType.AtMostOnce, LongName = "process-logs", ShortName = "",
        DefaultValue = false, HelpText = "Updates (creates if necessary) action tree from logs. Parameters: -logs -opponent -opp-act [-game-def] [-bucketizer].")]
        public bool processLogs;

        [Argument(ArgumentType.AtMostOnce, LongName = "show-opp-act", ShortName = "",
        DefaultValue = false, HelpText = "Create a graphviz files for opponent action tree. Parameters: -opp-act.")]
        public bool showOppActionTree;

        [Argument(ArgumentType.AtMostOnce, LongName = "monte-carlo", ShortName = "",
        DefaultValue = false, HelpText = "Apply monte-carlo to create preflop strategy for neytiry. Parameters: -neytiri -mc-count [-pockets].")]
        public bool monteCarlo;

        [Argument(ArgumentType.AtMostOnce, LongName = "print-neytiri-pf", ShortName = "",
        DefaultValue = false, HelpText = "Prints information about Neytiry preflop strategy. Parameters: -neytiri.")]
        public bool printNeytiryPf;

        [Argument(ArgumentType.AtMostOnce, LongName = "dump-node", ShortName = "",
        DefaultValue = false, HelpText = "Shows node as xml file. Parameters: -opp-act -node-id.")]
        public bool dumpNode;
        #endregion

        #region File parameters

        [Argument(ArgumentType.AtMostOnce, LongName = "opp-act", ShortName = "",
            DefaultValue = "", HelpText = "Opponent action tree file.")]
        public string oppActionTreeFile;

        [Argument(ArgumentType.AtMostOnce, LongName = "logs", ShortName = "",
            DefaultValue = "", HelpText = "Game logs file or folder")]
        public string gameLogsPath;

        [Argument(ArgumentType.AtMostOnce, LongName = "game-def", ShortName = "",
            DefaultValue = "", HelpText = "Game definition file.")]
        public string gameDef;

        [Argument(ArgumentType.AtMostOnce, LongName = "bucketizer", ShortName = "",
            DefaultValue = "", HelpText = "Bucketizer configuration file.")]
        public string bucketizer;

        [Argument(ArgumentType.AtMostOnce, LongName = "neytiri", ShortName = "",
            DefaultValue = "", HelpText = "Neytiri strategy.")]
        public string neytiri;


        [Argument(ArgumentType.AtMostOnce, LongName = "inc-logs", ShortName = "i",
            DefaultValue = "\\.log$", HelpText = "Include logs matching regex pattern.")]
        public string includeLogs;

        #endregion

        #region Other parameters

        [Argument(ArgumentType.AtMostOnce, LongName = "pockets", ShortName = "",
            DefaultValue = "", HelpText = "Specifies pocket cards, e.g. AcKcAcAd")]
        public string pockets;

        [Argument(ArgumentType.AtMostOnce, LongName = "match-path", ShortName = "",
            DefaultValue = null, HelpText = "Show only nodes that mathc the path, e.g. /B/d/r/r/r.")]
        public string matchPath;

        [Argument(ArgumentType.AtMostOnce, LongName = "show-buckets", ShortName = "",
            DefaultValue = 0, HelpText = "Show up to N buckets in action tree.")]
        public int showBuckets;

        [Argument(ArgumentType.AtMostOnce, LongName = "max-round", ShortName = "",
            DefaultValue = 999, HelpText = "Shous only rounds <= max in the action tree.")]
        public int maxRound;

        [Argument(ArgumentType.AtMostOnce, LongName = "node-id", ShortName = "",
            DefaultValue = "", HelpText = "Action tree node id (pos.id), e.g. 0.1345")]
        public string nodeId;

        [Argument(ArgumentType.AtMostOnce, LongName = "mc-count", ShortName = "",
            DefaultValue = 1000, HelpText = "Number of repetitions in monte-carlo method (for each pocket in each position).")]
        public int mcCount;

        [Argument(ArgumentType.AtMostOnce, LongName = "opponent", ShortName = "o",
            DefaultValue = "", HelpText = "Name of the opponent")]
        public string opponent;

        #endregion
    }
}