/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.pkr.holdem.strategy.core;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.holdem.learn
{
    public static unsafe class PreflopStrategy
    {
        public static ChanceTree CreateCt(HePocketKind [] pockets, double [] oppCardProbab)
        {
            int n = pockets.Count();
            double[] dealProbab = PocketHelper.GetProbabDistr(pockets);

            int nodeCount = 1 + n + n * n;
            ChanceTree ct = new ChanceTree(nodeCount);
            ct.PlayersCount = 2;
            ct.Nodes[0].Probab = 1;
            ct.SetDepth(0, 0);
            for(int c0 = 0; c0 < n; ++c0)
            {
                int heroNode = 1 + c0 * (n + 1);
                ct.SetDepth(heroNode, 1);
                ct.Nodes[heroNode].Position = 0;
                ct.Nodes[heroNode].Probab = dealProbab[c0];
                ct.Nodes[heroNode].Card = c0;
                
                double[] corrOppProbab = CorrectOpponentProbab(pockets, c0, dealProbab, oppCardProbab);

                for(int c1 = 0; c1 < n; ++c1)
                {
                    int oppNode = heroNode + 1 + c1;
                    ct.SetDepth(oppNode, 2);
                    ct.Nodes[oppNode].Position = 1;
                    ct.Nodes[oppNode].Probab = ct.Nodes[heroNode].Probab * corrOppProbab[c1];
                    ct.Nodes[oppNode].Card = c1;
                    PocketEquity.Result pe = PocketEquity.CalculateFast(pockets[c0], pockets[c1]);
                    var potShare = new double[]{pe.Equity, 1 - pe.Equity};
                    ct.Nodes[oppNode].SetPotShare(3, potShare);
                }
            }
            VerifyChanceTree.VerifyS(ct, 1e-5);
            return ct;
        }

        /// <summary>
        /// A heuristic to correct opponent probabilies based on the card dealt to the hero.
        /// </summary>
        private static double[] CorrectOpponentProbab(HePocketKind[] pockets, int heroCard, double[] dealProbab, double[] oppCardProbab)
        {
            int n = pockets.Length;
            // Calculate card distribution for opponents with condition that the hero received heroCard.
            double[] oppDealProbabCond = PocketHelper.GetProbabDistr(pockets, HePocket.KindToCardSet(pockets[heroCard]));
            // Calculate a corrected probability of opponent cards.
            double[] corrOppProbab = new double[n];
            double sum = 0;
            for(int i = 0; i < n; ++i)
            {
                double condCoeff = oppDealProbabCond[i]/dealProbab[i];
                corrOppProbab[i] = oppCardProbab[i] * condCoeff;
                sum += corrOppProbab[i];
            }
            // Normalize the probability of opponent cards.
            for (int i = 0; i < n; ++i)
            {
                corrOppProbab[i] /= sum;
            }
            return corrOppProbab;
        }
    }
}
