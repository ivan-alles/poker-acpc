/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Merge data from one strategy tree to another.
    /// </summary>
    public unsafe static class MergeStrategies
    {
        /// <summary>
        /// Copies data from src to dst.
        /// Returns true if everything is OK. If something went wrong, an exception is thrown.
        /// </summary>
        public static void Merge(StrategyTree dst, StrategyTree src, int pos)
        {
            Comparer comparer = new Comparer();
            comparer.Position = pos;
            if (comparer.Compare(dst, src, comparer.CopyNode))
            {
                return;
            }
            string message;
            switch (comparer.Result)
            {
                case ICompareUFTreesDefs.ResultKind.StructureDiffers:
                    message = string.Format("Strategies have different structures at node: {0}", comparer.DiffersAt);
                    break;
                case ICompareUFTreesDefs.ResultKind.ValueDiffers:
                    message = string.Format("Position or node type mismatch at node: {0}", comparer.DiffersAt);
                    break;
                default:
                    message = string.Format("Unknown error at node: {0}", comparer.DiffersAt);
                    break;
            }
            throw new ApplicationException(message);
        }

        class Comparer : CompareUFTrees<StrategyTree, StrategyTree>
        {
            public int Position;

            public bool CopyNode(StrategyTree dst, StrategyTree src, long n)
            {
                if (dst.Nodes[n].Position != src.Nodes[n].Position)
                {
                    return false;
                }
                if (dst.Nodes[n].IsDealerAction != src.Nodes[n].IsDealerAction)
                {
                    return false;
                }
                if (n > 0 && dst.Nodes[n].IsPlayerAction(Position))
                {
                    dst.Nodes[n].Probab = src.Nodes[n].Probab;
                }
                return true;
            }
        }
    }
}
