/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy.algorithms;
using System.Reflection;
using System.IO;
using ai.pkr.metastrategy;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;
using ai.lib.algorithms.numbers;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Verifies the following properties of a chance tree:
    /// <para>1. Sum of child probabilities must be equal to the probab. of the parent.</para>
    /// <para>2. Position should change after every deal from 0 to playerCount - 1, 
    /// unless it is set to -1 for all nodes except root.</para>
    /// <para> If something is wrong, an application exception is thrown.</para>
    /// </summary>
    public unsafe class VerifyChanceTree
    {
        #region Public API

        public VerifyChanceTree()
        {
            Epsilon = 0.000000000000001;
        }

        public void Verify(ChanceTree ct)
        {
            _playersCount = ct.Nodes[0].Position;
            _areAllPosInvalid = true;
            WalkUFTreePP<ChanceTree, Context> wt = new WalkUFTreePP<ChanceTree, Context>();
            wt.OnNodeBegin = OnNodeBegin;
            wt.OnNodeEnd = OnNodeEnd;
            wt.Walk(ct);
        }

        public double Epsilon
        {
            set;
            get;
        }

        public static void VerifyS(ChanceTree ct)
        {
            VerifyChanceTree vct = new VerifyChanceTree();
            vct.Verify(ct);
        }

        public static void VerifyS(ChanceTree ct, double epsilon)
        {
            VerifyChanceTree vct = new VerifyChanceTree {Epsilon = epsilon };
            vct.Verify(ct);
        }

        #endregion

        #region Implementation

        class Context : WalkUFTreePPContext
        {
            public bool IsLeaf;
            public double SumProbabsOfChildren;
        }

        void OnNodeBegin(ChanceTree tree, Context[] stack, int depth)
        {
            Context context = stack[depth];
            Int64 n = context.NodeIdx;
            context.SumProbabsOfChildren = 0;
            context.IsLeaf = true;

            if (depth > 0)
            {
                stack[depth - 1].SumProbabsOfChildren += tree.Nodes[n].Probab;
                stack[depth - 1].IsLeaf = false;

                if(tree.Nodes[n].Position != -1)
                {
                    _areAllPosInvalid = false;
                }
                if(!_areAllPosInvalid)
                {
                    VerifyPostion(tree, n, (depth - 1) % (_playersCount));
                }
            }
        }

        void VerifyPostion(ChanceTree tree, Int64 n, int expectedPos)
        {   
            if(tree.Nodes[n].Position != expectedPos)
            {
                throw new ApplicationException(String.Format("Node {0} - wrong position {1}, expected {2}",
                    n, tree.Nodes[n].Position, expectedPos));
            }
        }

        void OnNodeEnd(ChanceTree tree, Context[] stack, int depth)
        {
            Context context = stack[depth];
            Int64 n = context.NodeIdx;

            if (!context.IsLeaf)
            {
                if(!FloatingPoint.AreEqual(tree.Nodes[context.NodeIdx].Probab, context.SumProbabsOfChildren, Epsilon))
                {
                    throw new ApplicationException(String.Format(
                        "Node {0}: sum of chance probability of children {1} != chance probability of the paren {2}",
                                  context.NodeIdx, context.SumProbabsOfChildren, tree.Nodes[context.NodeIdx].Probab));
                    ;
                }
            }
        }

        bool _areAllPosInvalid;
        int _playersCount;

        #endregion
    }
}
