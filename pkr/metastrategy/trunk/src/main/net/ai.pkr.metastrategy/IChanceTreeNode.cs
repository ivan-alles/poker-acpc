/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Chance tree node. 
    /// <para>It supports the following requirements:</para>
    /// <para>1. It must be possible to describe the chance parameters of the game (full chance tree).</para>
    /// <para>2. Position-dependent game abstraction must be supported (for example different 
    /// bucketizing rule or number of buckets for each position).</para>
    /// <para>3. It must be possible to describe a player chance tree.</para> 
    /// <para>4. It must be possible to extract a player chance tree from a full game chance tree.</para>
    /// <para>There is no information about round, the round can be determined as following:</para>
    /// <para>- For the full game tree a new round starts in nodes with Position == 0.</para>
    /// <para>- For a player tree a new round starts after each deal.</para> 
    /// </summary>
    /// Todo: think if we need this interface.
    public interface IChanceTreeNode : IDealerAction
    {
        /// <summary>
        /// Chance probability of this node.
        /// </summary>
        double Probab { get; }

        /// <summary>
        /// Result of the game. activePlayers is a bit-mask. 
        /// result[] must have enough elements for all players 
        /// (also for inactive ones). It will be filled with the 
        /// bet-independent pot shares in range [0, 1]. Sum of
        /// results of active players is 1 unless rake is used.
        /// The method must work correctly also for the case where is only one non-folder 
        /// in all nodes except the root. 
        /// This is the most universal form of determining pot share 
        /// allowing to apply rake and use non-standard rules.
        /// The actual game result of a player is calculated as following:
        /// result[pos] = Pot * potShare[pos] - inPot[pos]. This is correct
        /// even for a single non-folder.
        /// </summary>
        /// Developer notes:
        /// Think about adding a constraint:
        /// The method must work correctly for the last node of each round, 
        /// where all cards of the round are dealt to each player, in other nodes the result is undefined.
        /// Todo:
        /// The typical rake rule includes a percentage and a cap, the cap is absolute. That means, the rake
        /// cannot be calculated without pot size. So, everywhere we need to calculate game result, we have to use
        /// this rake rule. We can write a helper for it. And we can simplify this class by prohibiting to call
        /// GetPotShare() in non-leaves and for number of active players less than 2. This will make such algos as CompareChanceTrees much easier.
        void GetPotShare(UInt16 activePlayers, double[] potShare);
    }
}