/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;

namespace ai.pkr.metagame
{
    /// <summary>
    /// API to customize rules of the game.
    /// As different poker games (Hold'em, Omaha, Omaha Hi/Lo, experimental games) 
    /// have different hand evaluation rules, we need an API to deal with it. 
    /// </summary>
    public interface IGameRules
    {
        /// <summary>
        /// Called once when the object is created.
        /// </summary>
        /// <param name="creationParams">Configuration string in user-defined format.</param>
        void OnCreate(Props creationParams);

        /// <summary>
        /// Does a showdown, calculating hand ranks for non-folded players. 
        /// This function must be strictly thread-safe.
        /// <para>The following conventions are used:</para>
        /// <para>- Hand rank is a non-negative integer.</para> 
        /// <para>- 0 is reserved for folded players. If a player folded, this class must not touch 
        ///   the ranks for it, the caller handles this.</para>
        /// <para>- The higher the rank the stronger the hand.</para>
        /// <para>- Order of elements in hands[] is the same as order of elements in ranks[].</para>
        /// <para>- For hi/lo variants ranks[] will have twice as many entries, first N for high, next N for low.</para>
        /// </summary>
        /// <param name="hands">Array of cards indexes for each player. Is null for folders.</param>
        /// <param name="ranks">Hand ranks, is filled by this class.</param>
        void Showdown(GameDefinition gameDefinition, int [][] hands, UInt32 [] ranks);
    }
}