/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Node of an action tree. It contains sufficient information
    /// to build the important game state data, like pot size or round.
    /// </summary>
    public interface IActionTreeNode : IPlayerAction
    {
        /// <summary>
        /// Bit-mask, 1 corresponds to an active player, 0 - to a non-active.
        /// Non-active players do not contest the pot and lose their bets.
        /// This corresponds to a fold in regular poker terminology.
        /// </summary>
        UInt16 ActivePlayers
        {
            get;
        }

        /// <summary>
        /// Current round. We need to store it because we do cannot make an assumption
        /// about action in the strategy. In other words, we do not know if it is a "stopping" action 
        /// (call, fold) or "continuing" action (raise).
        /// </summary>
        int Round { get; }
    }
}