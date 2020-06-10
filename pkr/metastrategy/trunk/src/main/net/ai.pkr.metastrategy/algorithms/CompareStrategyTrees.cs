/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;
using System.IO;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Compares strategy trees. The main use case is to compare the difference in probabilities,
    /// therefore it is expected that the tree structures, positions, amounts and cards match.
    /// If they do not match, an exception is thrown.
    /// </summary>
    public unsafe class CompareStrategyTrees
    {
        public CompareStrategyTrees()
        {
            Output = Console.Out;
        }

        public void Compare(StrategyTree st0, StrategyTree st1)
        {
            if (IsVerbose)
            {
                Output.WriteLine("Compare chance trees");
                Output.WriteLine("0: '{0}'", st0.Version.Description);
                Output.WriteLine("1: '{0}'", st1.Version.Description);
            }

            if (st0.PlayersCount != st1.PlayersCount)
            {
                ReportError(0, string.Format("player count: t0:{0} != t1:{1}", st0.PlayersCount, st1.PlayersCount));
            }
            int playersCount = st0.PlayersCount;
            MaxProbabDiff = new double[playersCount].Fill(double.MinValue);
            SumProbabDiff = new double[playersCount];
            AverageProbabDiff = new double[playersCount];
            PlayerNodesCount = new long[playersCount];

            CompareUFTrees<StrategyTree, StrategyTree> comparer = new CompareUFTrees<StrategyTree, StrategyTree>();
            comparer.Compare(st0, st1, CompareNodes);
            if (comparer.Result != ICompareUFTreesDefs.ResultKind.Equal)
            {
                ReportError(comparer.DiffersAt, comparer.Result.ToString());
            }
            for (int p = 0; p < playersCount; ++p)
            {
                AverageProbabDiff[p] = SumProbabDiff[p] / PlayerNodesCount[p];
            }

            if (IsVerbose)
            {
                for (int p = 0; p < playersCount; ++p)
                {
                    Output.WriteLine("Probab diff p {0}: max {1,-20}  sum {2,-20}  av {3,-20}", 
                        p, MaxProbabDiff[p], SumProbabDiff[p], AverageProbabDiff[p]);
                }
            }
        }

        /// <summary>
        /// Compares trees verbose.
        /// </summary>
        public static void CompareS(StrategyTree st0, StrategyTree st1)
        {
            CompareStrategyTrees comparer = new CompareStrategyTrees { IsVerbose = true };
            comparer.Compare(st0, st1);
        }

        public bool IsVerbose
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

        public double[] MaxProbabDiff
        {
            get;
            private set;
        }

        public double[] SumProbabDiff
        {
            get;
            private set;
        }

        public double[] AverageProbabDiff
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of player nodes, including blinds.
        /// </summary>
        public long[] PlayerNodesCount
        {
            get;
            private set;
        }

        #region Implementation

        void ReportError(long node, string error)
        {
            throw new ApplicationException(string.Format("Mismatch in node {0}: {1}", node, error));
        }

        bool CompareNodes(StrategyTree t0, StrategyTree t1, Int64 n)
        {
            if (t0.Nodes[n].Position != t1.Nodes[n].Position)
            {
                ReportError(n, string.Format("position mismatch: t0:{0} != t1:{1}", t0.Nodes[n].Position, t1.Nodes[n].Position));
            }
            if (n == 0)
            {
                return true;
            }
            if (t0.Nodes[n].IsDealerAction != t1.Nodes[n].IsDealerAction)
            {
                ReportError(n, "action kind mismatch");
            }
            if (t0.Nodes[n].IsDealerAction)
            {
                if (t0.Nodes[n].Card != t1.Nodes[n].Card)
                {
                    ReportError(n, string.Format("card mismatch: t0:{0} != t1:{1}", t0.Nodes[n].Card, t1.Nodes[n].Card));
                }
                return true;
            }
            if (t0.Nodes[n].Amount != t1.Nodes[n].Amount)
            {
                ReportError(n, string.Format("amount mismatch: t0:{0} != t1:{1}", t0.Nodes[n].Amount, t1.Nodes[n].Amount));
            }
            int p = t0.Nodes[n].Position;
            double probabDiff = Math.Abs(t0.Nodes[n].Probab - t1.Nodes[n].Probab);
            SumProbabDiff[p] += probabDiff;
            MaxProbabDiff[p] = Math.Max(probabDiff, MaxProbabDiff[p]);
            PlayerNodesCount[p]++;

            return true;
        }
        #endregion
    }
}
