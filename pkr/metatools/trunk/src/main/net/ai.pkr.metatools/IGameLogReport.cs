/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using ai.pkr.metagame;
using System.IO;

namespace ai.pkr.metatools
{
    /// <summary>
    /// An interface to implement custom reporters for game logs.
    /// A log analyzer parses the log and gives each game record to the game log reported
    /// for processing. At the end a Print() can be called to view the report.
    /// </summary>
    public interface IGameLogReport
    {
        void ShowHelp(TextWriter tw);

        /// <summary>
        /// A name of this report given by the user.
        /// </summary>
        string Name
        {
            set;
            get;
        }

        /// <summary>
        /// Configures the log, is called at the beginning of the analysis.
        /// </summary>
        void Configure(Props pm);

        /// <summary>
        /// Is called to update the report by a game record.
        /// </summary>
        void Update(GameRecord gameRecord);

        /// <summary>
        /// Prints the report.
        /// </summary>
        void Print(TextWriter tw);
    }
}
