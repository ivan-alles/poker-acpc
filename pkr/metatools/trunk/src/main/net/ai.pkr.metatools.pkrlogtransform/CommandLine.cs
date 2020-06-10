/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.pkr.metatools.pkrlogtransform
{
    /// <summary>
    /// Command line. 
    /// </summary>
    [CommandLine(HelpText = @"Transforms game logs. See options for details.
Renaming players:
Renames are done before other usages of player names. Order: eq, neq. Only one rename per name can be done.")]
    public class CommandLine : StandardCmdLine
    {
        #region Commands

        #endregion

        #region File parameters

        [DefaultArgument(ArgumentType.Required, LongName = "input-log-file",
        HelpText = "Input log file")]
        public string InputFile = null;

        #endregion

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "verbose", ShortName = "v",
        DefaultValue = false, HelpText = "Verbose parsing.")]
        public bool Verbose;

        [Argument(ArgumentType.AtMostOnce, LongName = "output", ShortName = "o",
        DefaultValue = null, HelpText = "Output file. Default: 'InputFileName-tr.ext'.")]
        public string Output = null;

        [Argument(ArgumentType.AtMostOnce, LongName = "game-limit", ShortName = "",
        DefaultValue = int.MaxValue, HelpText = "Analyze up to N games.")]
        public int GameLimit = int.MaxValue;

        [Argument(ArgumentType.AtMostOnce, LongName = "finalize-games", ShortName = "",
        DefaultValue = false, HelpText = "Set all games to game over.")]
        public bool FinalizeGames;

        [Argument(ArgumentType.AtMostOnce, LongName = "renumerate-games", ShortName = "",
        DefaultValue = false, HelpText = "Renumerate games from 0.")]
        public bool RenumerateGames;

        [Argument(ArgumentType.AtMostOnce, LongName = "hide-opponent-cards", ShortName = "",
        DefaultValue = false, HelpText = "Replace cards of opponents by '?' (both private and public).")]
        public bool HideOpponentCards;

        [Argument(ArgumentType.AtMostOnce, LongName = "normalize-cards", ShortName = "",
        DefaultValue = false, HelpText = "Makes sure the cards are one space separated and trimmed.")]
        public bool NormalizeCards;

        [Argument(ArgumentType.AtMostOnce, LongName = "reset-stacks", ShortName = "",
        DefaultValue = false, HelpText = "Set stacks to 0.")]
        public bool ResetStacks;

        [Argument(ArgumentType.AtMostOnce, LongName = "reset-results", ShortName = "",
        DefaultValue = false, HelpText = "Set game results to 0.")]
        public bool ResetResults;

        [Argument(ArgumentType.AtMostOnce, LongName = "normalize-stakes", ShortName = "n",
        DefaultValue = "", HelpText = "Normalize stakes with the given norm (a double value). Leave empty to skip normalizing, specify 0 to auto-detect.")]
        public string NormalizeStakes;

        [Argument(ArgumentType.AtMostOnce, LongName = "remove-metadata", ShortName = "",
        DefaultValue = false, HelpText = "Remove meta-data.")]
        public bool RemoveMetadata;

        [Argument(ArgumentType.AtMostOnce, LongName = "rename-eq", ShortName = "",
        DefaultValue = "", HelpText = "Syntax: Name,NewName. Renames a player if his name equals to Name.")]
        public string RenameEq = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "rename-neq", ShortName = "",
        DefaultValue = "", HelpText = "Syntax: name,newname. Renames a player if his name does NOT equal to Name.")]
        public string RenameNeq = "";

        [Argument(ArgumentType.AtMostOnce, LongName = "hero-name", ShortName = "",
        DefaultValue = "Agent", HelpText = "Hero name.")]
        public string HeroName = "Agent";

        [Argument(ArgumentType.AtMostOnce, LongName = "remove-no-hero-moves", ShortName = "",
        DefaultValue = false, HelpText = "Remove games without hero moves.")]
        public bool RemoveNoHeroMoves;

        [Argument(ArgumentType.AtMostOnce, LongName = "remove-no-showdown", ShortName = "",
        DefaultValue = false, HelpText = "Remove games without showdown.")]
        public bool RemoveNoShowdown;

        #endregion
    }
}