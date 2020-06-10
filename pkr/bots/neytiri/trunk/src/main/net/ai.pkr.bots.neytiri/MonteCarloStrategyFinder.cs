/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using System.Diagnostics;
using ai.lib.algorithms.tree;

namespace ai.pkr.bots.neytiri
{
    public class MonteCarloStrategyFinder
    {
        static int Showdown(CardSet ourPocket, CardSet oppPocket, CardSet board, int handSize)
        {
            Debug.Assert(!ourPocket.IsIntersectingWith(oppPocket));
            Debug.Assert(!ourPocket.IsIntersectingWith(board));
            Debug.Assert(!ourPocket.IsIntersectingWith(board));

            CardSet ourHand = new CardSet(ourPocket, board);
            CardSet oppHand = new CardSet(oppPocket, board);

            UInt32 ourRank = CardSetEvaluator.Evaluate(ref ourHand);
            UInt32 oppRank = CardSetEvaluator.Evaluate(ref oppHand);
            if (ourRank > oppRank)
                return 1;
            else if (ourRank < oppRank)
                return -1;
            return 0;
        }

        public static void DoMonteCarlo(
            ActionTree tree,
            int ourPos,
            CardSet pocket,
            int round,
            string sharedCardsAsString,
            List<ActionTreeNode> strategyPath,
            int repetitionsCount)
        {
            string[] sharedCards = sharedCardsAsString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            ActionTreeNode curStrategyNode = strategyPath[strategyPath.Count - 1];

            // Here we sometimes use the knowledge about the game definition.
            _clearValues.Walk(tree, curStrategyNode);

            MonteCarloData[] mc = new MonteCarloData[tree.GameDef.RoundsCount];
            for (int r = 0; r < mc.Length; ++r)
            {
                mc[r] = new MonteCarloData();
            }

            // Fill known public data.
            for (int r = 1; r <= round; ++r)
            {
                int sharedCount = tree.GameDef.SharedCardsCount[r];
                mc[r].boardSize = mc[r - 1].boardSize + sharedCount;
                mc[r].shared = StdDeck.Descriptor.GetCardSet(sharedCards, mc[r - 1].boardSize, sharedCount);
                mc[r].board = mc[r - 1].board | mc[r].shared;

                Debug.Assert(mc[r].shared.CountCards() == sharedCount);
                Debug.Assert(mc[r].board.CountCards() == mc[r].boardSize);
            }
            Debug.Assert(mc[round].boardSize == sharedCards.Length);

            MonteCarloDealer mcDealer = new MonteCarloDealer();
            ApplyMonteCarloData applyMc = new ApplyMonteCarloData();
            mcDealer.Initialize(pocket | mc[round].board);
            Debug.Assert(mcDealer.Cards.Length + pocket.CountCards() + mc[round].boardSize == 52);

            for (int repetition = 0; repetition < repetitionsCount * 10; ++repetition)
            {
                mcDealer.Shuffle(7); // 2 for opponent, up to 5 for the board

                CardSet mcPocket = StdDeck.Descriptor.GetCardSet(mcDealer.Cards, 0, 2);

                Debug.Assert(mc[0].board.IsEmpty());
                int sharedDealt = 0;
                for (int r = 0; r < tree.GameDef.RoundsCount; ++r)
                {
                    if (r > round)
                    {
                        mc[r].boardSize = mc[r - 1].boardSize + tree.GameDef.SharedCardsCount[r];
                        mc[r].shared = StdDeck.Descriptor.GetCardSet(mcDealer.Cards, 2 + sharedDealt, tree.GameDef.SharedCardsCount[r]);
                        sharedDealt += tree.GameDef.SharedCardsCount[r];
                        Debug.Assert(!mc[r - 1].board.Contains(mc[r].shared));
                        Debug.Assert(!mcPocket.Contains(mc[r].shared));
                        mc[r].board = mc[r - 1].board | mc[r].shared;
                    }
                    mc[r].bucket = tree.Bucketizer.GetBucket(mcPocket, mc[r].board, r);
                }
                double strategyFactor = 1;
                // Go through the strategy path, skip the b-node
                Debug.Assert(strategyPath[0].ActionKind == Ak.b);
                // Start from pos. 2 to skip deals (where OppBuckets always contains 0s if ourPos == 0).
                for (int pathIndex = 2; pathIndex < strategyPath.Count; pathIndex++)
                {
                    ActionTreeNode pathNode = strategyPath[pathIndex];
                    Debug.Assert(pathNode.ActionKind != Ak.b);
                    // Take nodes where opponent acts.
                    if (pathNode.State.CurrentActor == 1 - ourPos)
                    {
                        // nextNode always exists because this function is called when we act
                        ActionTreeNode nextNode = strategyPath[pathIndex + 1];
                        Debug.Assert(pathNode.State.Round == nextNode.State.Round);
                        int freq = pathNode.OppBuckets.Counts[mc[pathNode.State.Round].bucket];
                        int nextFreq = nextNode.OppBuckets.Counts[mc[pathNode.State.Round].bucket];
                        double coef = freq == 0 ? 0 : (double)nextFreq / freq;
                        strategyFactor *= coef;
                    }
                }
                if (strategyFactor > 0)
                {
                    int showdownValue = Showdown(pocket, mcPocket,
                                                 mc[tree.GameDef.RoundsCount - 1].board,
                                                 mc[tree.GameDef.RoundsCount - 1].boardSize + 2);
                    applyMc.ApplyData(curStrategyNode, mc, ourPos, showdownValue, strategyFactor);
                    repetition += 9;
                }
            }
            FinalizeMonteCarloData finalizer = new FinalizeMonteCarloData();
            finalizer.Finalize(tree, curStrategyNode, ourPos);
        }

        static WalkTree<ActionTree, ActionTreeNode, int> _clearValues =
        new WalkTree<ActionTree, ActionTreeNode, int>
        {
            OnNodeBegin = (t, n, s, d) =>
            {
                n.Value = 0;
                return true;
            }
        };
    }
}
