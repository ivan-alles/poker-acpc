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
    public class MiniFlRules : IGameRules
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
            }
        }


        public static int CardToRank(string card)
        {
            int rank = ALL_CARDS.IndexOf(card[0]);
            if (card.Length != 1 || rank == -1)
                throw new ApplicationException("Unknown card: " + card);
            return rank + 1;
        }

        #endregion

        #region Implementation

        private static readonly string ALL_CARDS = "JQKA";

        private static int GetRank(string privateCards, string sharedCards)
        {
            int board = CardToRank(sharedCards);
            int pocket = CardToRank(privateCards);
            int rank = pocket + board;
            if (pocket == 1 && board == 2)
            {
                // Straight
                rank = 8;
            }
            return rank;
        }

        object _thisLock = new object();

        #endregion
    }
}
