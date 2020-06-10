/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.metabots
{
    /// <summary>
    /// All data about a player from the server's perspective.
    /// </summary>
    internal class PlayerData
    {
        #region Room-lifetime and session-lifetime information about a player.
        internal IPlayer iplayer;
        internal PlayerInfo info;
        internal double stack;
        internal bool isLocal;
        #endregion

        #region Game-lifetime parameters 

        /// <summary>
        /// Player's view of the game, player's private cards are visible, 
        /// cards of other players are replaced by "?".
        /// </summary>
        internal GameRecord gameRecord;

        #endregion

    }
}
