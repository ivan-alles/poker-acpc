/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.numbers;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Verifies a set of equilibrium strategies by computing BR for each player against the rest and 
    /// making sure it's value equals to the value of the eq-strategy of the player.
    /// </summary>
    public static class VerifyEq
    {
        public static bool Verify(ActionTree at, ChanceTree ct, StrategyTree[] strategies, double epsilon, out string message)
        {
            message = "";
            // No need to check preconditions, GameValue does it 
            GameValue gv = new GameValue {ActionTree = at, ChanceTree = ct, Strategies = strategies };
            gv.Solve();

            for (int p = 0; p < at.PlayersCount; ++p)
            {
                StrategyTree[] strategiesCopy = (StrategyTree[])strategies.Clone();
                Br br = new Br { ActionTree = at, ChanceTree = ct, Strategies = strategiesCopy, HeroPosition = p };
                br.Solve();
                if (!FloatingPoint.AreEqual(gv.Values[p], br.Value, epsilon))
                {
                    message = String.Format("Unequal values for pos {0}: eq: {1}, br: {2}, eps: {3}",
                        p, gv.Values[p], br.Value, epsilon);
                    return false;
                }
            }
            return true;
        }
    }
}
