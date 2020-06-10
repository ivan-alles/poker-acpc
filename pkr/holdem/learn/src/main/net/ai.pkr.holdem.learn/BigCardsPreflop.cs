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
    public unsafe class BigCardsPreflop
    {
        public void Solve(ActionTree at, HePocketKind[] sbCards, HePocketKind[] bbCards)
        {
            Ct = CreateCt(sbCards, bbCards);
            var gv = new double[2];
            Strategies = EqLp.Solve(at, Ct, out gv);
            GameValues = gv;
        }

        public ChanceTree Ct
        {
            private set;
            get;
        }

        public double[] GameValues
        {
            private set;
            get;
        }

        public StrategyTree[] Strategies
        {
            private set;
            get;
        }

        private ChanceTree CreateCt(HePocketKind[] sbCards, HePocketKind[] bbCards)
        {
            int nodeCount = 1 + sbCards.Length + sbCards.Length * bbCards.Length;
            ChanceTree ct = new ChanceTree(nodeCount);
            ct.PlayersCount = 2;
            ct.Nodes[0].Probab = 1;
            ct.SetDepth(0, 0);

            int totalCombSB = 0;
            foreach(HePocketKind p in sbCards)
            {
                totalCombSB += HePocket.KindToRange(p).Length;
            }

            for (int c0 = 0; c0 < sbCards.Length; ++c0)
            {
                int sbNode = 1 + c0 * (bbCards.Length + 1);
                HePocketKind sbPocket = sbCards[c0];
                ct.SetDepth(sbNode, 1);
                ct.Nodes[sbNode].Position = 0;
                ct.Nodes[sbNode].Probab = (double)HePocket.KindToRange(sbPocket).Length / totalCombSB;
                ct.Nodes[sbNode].Card = c0;

                double[] oppDealProbabCond = PocketHelper.GetProbabDistr(bbCards, HePocket.KindToCardSet(sbPocket));

                for (int c1 = 0; c1 < bbCards.Length; ++c1)
                {
                    int bbNode = sbNode + 1 + c1;
                    ct.SetDepth(bbNode, 2);
                    ct.Nodes[bbNode].Position = 1;
                    ct.Nodes[bbNode].Probab = ct.Nodes[sbNode].Probab * oppDealProbabCond[c1];
                    ct.Nodes[bbNode].Card = c1;
                    PocketEquity.Result pe = PocketEquity.CalculateFast(sbPocket, bbCards[c1]);
                    var potShare = new double[] { pe.Equity, 1 - pe.Equity};
                    ct.Nodes[bbNode].SetPotShare(3, potShare);
                }
            }
            VerifyChanceTree.VerifyS(ct, 1e-5);
            return ct;
        }

    }
}
