/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.research.eqbr
{
    public class Rules: IGameRules
    {
        #region IGameRules Members

        public void OnCreate(Props creationParams)
        {
        }

        public int[] Showdown(GameDefinition gd, GameState gameState)
        {
            int[] ranks = new int[gameState.Players.Length];
            for (int p = 0; p < ranks.Length; ++p)
            {
                if (gameState.Players[p].IsFolded)
                    continue;
                string card = gameState.Players[p].PrivateCards;
                if (card == "A")
                    ranks[p] = 2;
                else if (card == "K")
                    ranks[p] = 1;
                else
                    throw new ApplicationException("Unknown card: " + card);
            }
            return ranks;
        }

        #endregion
    }
}
