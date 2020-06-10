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
    /// Example of game rules. Can be used for demo and test games.
    /// </summary>
    public class GameRulesExample: IGameRules
    {
        #region IGameRules Members

        public void OnCreate(Props creationParams)
        {
        }

        public void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
        {
            for (int i = 0; i < hands.Length; ++i)
            {
                if (hands[i] != null)
                {
                    // Tie except for folders.
                    ranks[i] = 1;
                }
            }
        }

        #endregion
    }
}
