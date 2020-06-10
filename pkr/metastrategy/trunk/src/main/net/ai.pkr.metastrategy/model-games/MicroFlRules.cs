/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils;
using System.Diagnostics;

namespace ai.pkr.metastrategy.model_games
{
    public class MicroFlRules : IGameRules
    {
        #region IGameRules Members

        public void OnCreate(Props creationParams)
        {
        }

        public void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
        {
            lock (_thisLock)
            {
                for (int p = 0; p < ranks.Length; ++p)
                {
                    if (hands[p] == null)
                        continue;
                    int board = CardToRank(gameDefinition.DeckDescr.GetCardNames(hands[p], 1,1));

                    int pocket = CardToRank(gameDefinition.DeckDescr.GetCardNames(hands[p], 0,1));
                    if (pocket == board)
                    {
                        Debug.Assert(pocket == 1);
                        // Pair of Jacks
                        ranks[p] = 4;
                    }
                    else
                    {
                        ranks[p] = (UInt32)pocket;
                    }
                }
            }
        }

        #endregion

        #region Implementation

        private static readonly string ALL_CARDS = "JQK";

        static int CardToRank(string card)
        {
            int rank = ALL_CARDS.IndexOf(card[0]);
            if (card.Length != 1 || rank == -1)
                throw new ApplicationException("Unknown card: " + card);
            return rank + 1;
        }

        object _thisLock = new object();

        #endregion
    }
}
