/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.pkr.stdpoker;
using ai.lib.utils;
using ai.pkr.metagame;

namespace ai.pkr.holdem.gamedef
{
    /// <summary>
    /// IGameRules implementation for TexasHoldem.
    /// </summary>
    public class HoldemGameRules : IGameRules
    {
        #region IGameRules Members

        public void OnCreate(Props creationParams)
        {
        }

        public unsafe void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
        {
            UInt32 board = 0;
            for (int p = 0; p < hands.Length; ++p)
            {
                if (hands[p] != null)
                {
                    if (board == 0)
                    {
                        // Evaluate board only once.
                        board = LutEvaluator7.pLut[hands[p][2]];
                        board = LutEvaluator7.pLut[board + hands[p][3]];
                        board = LutEvaluator7.pLut[board + hands[p][4]];
                        board = LutEvaluator7.pLut[board + hands[p][5]];
                        board = LutEvaluator7.pLut[board + hands[p][6]];
                    }
                    ranks[p] = LutEvaluator7.pLut[LutEvaluator7.pLut[board + hands[p][0]] + hands[p][1]];
                }
            }
        }

        #endregion

    }
}