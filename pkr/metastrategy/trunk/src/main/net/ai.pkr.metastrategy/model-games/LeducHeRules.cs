/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metastrategy.model_games
{
    public class LeducHeRules : IGameRules
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
                    string privateCards = gameDefinition.DeckDescr.GetCardNames(hands[p], 0, 1);
                    string sharedCards = gameDefinition.DeckDescr.GetCardNames(hands[p], 1, 1);
                    ranks[p] = (UInt32)GetRank(privateCards, sharedCards);
                }
                // If folded, the value is set to 0 by default.
            }
        }

        public static int GetRank(string privateCards, string sharedCards)
        {
            int board  = CardToRank(sharedCards);
            int pocket = CardToRank(privateCards);
            if (pocket == board)
            {
                return 4;
            }
            return pocket;
        }

        #endregion

        #region Implementation

        private static readonly string ALL_CARDS = "JQK";

        static int CardToRank(string card)
        {
            // This is required for suited version of Leduc HE.
            card = card.Substring(0, 1);

            int rank = ALL_CARDS.IndexOf(card[0]);
            if (card.Length != 1 || rank == -1)
                throw new ApplicationException("Unknown card: " + card);
            return rank + 1;
        }

        object _thisLock = new object();

        #endregion
    }
}
