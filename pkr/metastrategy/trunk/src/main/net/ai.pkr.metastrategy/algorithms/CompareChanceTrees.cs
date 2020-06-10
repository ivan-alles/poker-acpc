/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using System.IO;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Compares 2 chance trees. The results of comparison are stored in properites and can
    /// also be printed in verbose mode.
    /// <para>The algorithm compares only data in leaves, because the probabilities in other nodes are 
    /// can be calculated knowing probabilities in leaves.</para>
    /// </summary>
    public unsafe class CompareChanceTrees
    {
        #region Public API

        public CompareChanceTrees()
        {
            Output = Console.Out;
        }

        /// <summary>
        /// Print information to console, default: false.
        /// </summary>
        public bool IsVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// If set, allows to compare chance trees with stuctural differences. 
        /// This is useful to for trees created by MC sampling.
        /// The number of players must be nevertheless equal.
        /// Default: false.
        /// </summary>
        public bool AllowDifferentStructure
        {
            set;
            get;
        }

        /// <summary>
        /// Writes informational messages to this writer, default: Console.Out.
        /// </summary>
        public TextWriter Output
        {
            set;
            get;
        }

        public int[] LeavesCount
        {
            protected set;
            get;
        }

        public double SumProbabDiff
        {
            protected set;
            get;
        }

        public double AverageProbabDiff
        {
            protected set;
            get;
        }

        public double MaxProbabDiff
        {
            protected set;
            get;
        }

        public double SumRelProbabDiff
        {
            protected set;
            get;
        }

        public double AverageRelProbabDiff
        {
            protected set;
            get;
        }

        public double MaxRelProbabDiff
        {
            protected set;
            get;
        }


        public double[] SumPotShareDiff
        {
            protected set;
            get;
        }

        public double[] AveragePotShareDiff
        {
            protected set;
            get;
        }

        public double[] MaxPotShareDiff
        {
            protected set;
            get;
        }

        public void Compare(ChanceTree ct0, ChanceTree ct1)
        {
            if (IsVerbose)
            {
                Output.WriteLine("Compare chance trees");
                Output.WriteLine("0: '{0}'", ct0.Version.Description);
                Output.WriteLine("1: '{0}'", ct1.Version.Description);
            }
            if (ct0.PlayersCount != ct1.PlayersCount)
            {
                throw new ApplicationException(String.Format("Player counts differ: {0} != {1}", 
                    ct0.PlayersCount, ct1.PlayersCount));
            }

            CompareUFTrees<ChanceTree, ChanceTree> comp = new CompareUFTrees<ChanceTree, ChanceTree>();

            _playersCount = ct0.PlayersCount;
            _maxDepth = ct0.CalculateRoundsCount() * _playersCount;

            LeavesCount = new int[2];
            SumPotShareDiff = new double[_playersCount];
            AveragePotShareDiff = new double[_playersCount];
            SumProbabDiff = 0;
            MaxProbabDiff = double.MinValue;
            MaxPotShareDiff = new double[_playersCount].Fill(i => double.MinValue);


            CompareTrees(ct0, ct1);

            double leavesCount = (LeavesCount[0] + LeavesCount[1]) *0.5;
            AverageProbabDiff = SumProbabDiff / leavesCount;
            AverageRelProbabDiff = SumRelProbabDiff / leavesCount;
            for (int p = 0; p < _playersCount; ++p)
            {
                AveragePotShareDiff[p] = SumPotShareDiff[p]/leavesCount;
            }

            if (IsVerbose)
            {
                for (int p = 0; p < _playersCount; ++p)
                {
                    Output.WriteLine("p {0}: leaves: {1:#,#}", p, LeavesCount[p]);
                }

                Output.WriteLine("Probab diff       : max {0,-20}  sum {1,-20}  av {2,-20}", MaxProbabDiff, SumProbabDiff, AverageProbabDiff);
                Output.WriteLine("Rel pr diff       : max {0,-20}  sum {1,-20}  av {2,-20}", MaxRelProbabDiff, SumRelProbabDiff, AverageRelProbabDiff);
                for (int p = 0; p < _playersCount; ++p)
                {
                    Output.WriteLine("Pot share diff p {0}: max {1,-20}  sum {2,-20}  av {3,-20}", p,
                        MaxPotShareDiff[p], SumPotShareDiff[p], AveragePotShareDiff[p]);
                }
            }

            // This prevents the chance trees from premature garbage collection.
            string dummy = ct0.ToString() + ct1.ToString();
        }

        /// <summary>
        /// Compares trees verbose.
        /// </summary>
        /// <param name="ct1"></param>
        /// <param name="ct2"></param>
        public static void CompareS(ChanceTree ct1, ChanceTree ct2)
        {
            CompareChanceTrees comparer = new CompareChanceTrees { IsVerbose = true };
            comparer.Compare(ct1, ct2);
        }


        #endregion

        #region Implementation

        static readonly string[] _cardToString;

        static CompareChanceTrees()
        {
            _cardToString = new string[100];
            for (int c = 0; c < 100; ++c)
            {
                _cardToString[c] = String.Format("{0:00}", c);
            }
        }

        class ConvertToDictContext : WalkUFTreePPContext
        {
            public string Key = "";
        }

        /// <summary>
        /// Converts a chance tree to a dictionary of leaves. This allows to compare trees with different structure easily.
        /// Use a string key of two charachters, this is proven to be very fast. // Todo: use new hash.
        /// </summary>
        static Dictionary<string, IntPtr> ConvertToDict(ChanceTree ct, int maxDepth)
        {
            WalkUFTreePP<ChanceTree, ConvertToDictContext> wt = new WalkUFTreePP<ChanceTree, ConvertToDictContext>();
            Dictionary<string, IntPtr> dict = new Dictionary<string, IntPtr>();
            StringBuilder sb = new StringBuilder();

            wt.OnNodeBegin =  (tree, stack, depth) =>
            {
                Int64 n = stack[depth].NodeIdx;
                if (depth > 0)
                {
                    int card = tree.Nodes[n].Card;
                    stack[depth].Key = stack[depth-1].Key  + _cardToString[card];
                }
                if(depth == maxDepth)
                {
                    dict.Add(stack[depth].Key, new IntPtr(tree.Nodes + n));
                }
            };
            wt.Walk(ct);
            return dict;
        }

        private void CompareTrees(ChanceTree ct0, ChanceTree ct1)
        {
            Dictionary<string, IntPtr> dict0 = ConvertToDict(ct0, _maxDepth);
            Dictionary<string, IntPtr> dict1 = ConvertToDict(ct1, _maxDepth);

            LeavesCount[0] = dict0.Count;
            LeavesCount[1] = dict1.Count;

            ChanceTreeNode dummyNode;

            // First compare common nodes and nodes from (dict0 - dict1)
            foreach (var kvp0 in dict0)
            {
                IntPtr p1;
                if (dict1.TryGetValue(kvp0.Key, out p1))
                {
                    CompareNodes((ChanceTreeNode*)kvp0.Value, (ChanceTreeNode*)p1);
                }
                else
                {
                    if (AllowDifferentStructure)
                    {
                        CompareNodes((ChanceTreeNode*)kvp0.Value, &dummyNode);
                    }
                    else
                    {
                        ReportDifferentStructure(1, kvp0.Key);
                    }
                }
            }

            // Now compare only (dict1 - dict0)
            foreach (var kvp1 in dict1)
            {
                IntPtr p0;
                if (!dict0.TryGetValue(kvp1.Key, out p0))
                {
                    if (AllowDifferentStructure)
                    {
                        CompareNodes(&dummyNode, (ChanceTreeNode*)kvp1.Value);
                    }
                    else
                    {
                        ReportDifferentStructure(0, kvp1.Key);
                    }
                }
            }
        }

        private void ReportDifferentStructure(int tree, string key)
        {
            throw new ApplicationException(String.Format("Structure differs: tree {0} does not contain path {1}", tree, key));
        }

        private void CompareNodes(ChanceTreeNode* pNode0, ChanceTreeNode * pNode1)
        {
            double probab0 = pNode0->Probab;
            double probab1 = pNode1->Probab;

            double probabDiff = Math.Abs(probab0 - probab1);
            SumProbabDiff += probabDiff;
            MaxProbabDiff = Math.Max(MaxProbabDiff, probabDiff);

            double relProbabDiff = (probab0 + probab1) != 0 ? probabDiff / (probab0 + probab1) / 2 : 0;
            SumRelProbabDiff += relProbabDiff;
            MaxRelProbabDiff = Math.Max(MaxRelProbabDiff, relProbabDiff);

            UInt16[] activePlayers;
            activePlayers = ActivePlayers.Get(_playersCount, 2, _playersCount);

            double[] potShares0 = new double[_playersCount];
            double[] potShares1 = new double[_playersCount];
            foreach (UInt16 ap in activePlayers)
            {
                pNode0->GetPotShare(ap, potShares0);
                pNode1->GetPotShare(ap, potShares1);

                for (int p = 0; p < _playersCount; ++p)
                {
                    double potShareDiff = Math.Abs(potShares0[p] - potShares1[p]);
                    SumPotShareDiff[p] += potShareDiff;
                    MaxPotShareDiff[p] = Math.Max(MaxPotShareDiff[p], potShareDiff);
                }
            }
        }

        int _playersCount;
        int _maxDepth;

        #endregion
    }
}
