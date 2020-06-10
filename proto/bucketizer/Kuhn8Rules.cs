using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils;

namespace bucketizer_proto
{
    public class Kuhn8Rules : IGameRules
    {
        #region IGameRules Members

        public void OnCreate(Props creationParams)
        {
        }

        public void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
        {
            bool tie = false;
            lock (_thisLock)
            {
                for (int p = 0; p < ranks.Length; ++p)
                {
                    if (hands[p] == null)
                        continue;
                    ranks[p] = (UInt32)CardToRank(gameDefinition.DeckDescr.GetCardNames(hands[p]));
                    tie |= ranks[p] == 1; // any T - always tie
                }
                if(tie)
                {
                    for (int p = 0; p < ranks.Length; ++p)
                    {
                        if (hands[p] != null)
                        {
                            ranks[p] = 1;
                        }
                    }
                }
            }
        }

        #endregion

        #region Implementation

        private static readonly string ALL_CARDS = "TJQK";

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