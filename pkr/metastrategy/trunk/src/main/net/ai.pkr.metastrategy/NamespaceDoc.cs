/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// This namespace contains various classes related to the poker strategy.
    /// The intention of this library is to provide a set of tools and reference data
    /// for development and verification of strategies for real world games. 
    /// <para>A game is described by a full game chance tree and a full game action tree.</para>
    /// <para>Chance tree contains:</para>
    /// <para>- chance probabilities of dealing specific "cards" to each player. Cards can be real cards or abstracted signals
    /// (buckets). Integers are used to encode cards. Cards must be recognizable from the perspective of the player. For example, 
    /// if a HE game abstraction uses different bucketizer for each position, than the encoding for each position must be 
    /// in terms of the corresponding bucketizer.</para>
    /// <para>- information about game result (showdown) for each possible combination of active players.</para>
    /// <para>Action tree contains possible actions for each player in each game situation (chance factor is ignored).</para>
    /// <para>A static strategy of a player is stored in a tree where chance information is encoded 
    /// from the perpective recognizable by the player.</para>
    /// </summary>
    public static class NamespaceDoc
    {
    }
}
