﻿/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metastrategy.model_games
{
    public class OcpRules : IGameRules
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
                    string card = gameDefinition.DeckDescr.GetCardNames(hands[p]);
                    int rank = ALL_CARDS.IndexOf(card[0]);
                    if (card.Length != 1 || rank == -1)
                        throw new ApplicationException("Unknown card: " + card);
                    ranks[p] = (UInt32)(rank + 1);
                }
            }
        }

        #endregion

        #region Implementation

        private static readonly string ALL_CARDS = "23456789TJQKA";

        object _thisLock = new object();

        #endregion
    }
}
