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
    /// Extracts player chance tree for a given position from the full game chance tree.
    /// <para>Information about cards and probabilities will be exctracted. Game results will not be set.</para>
    /// <para>Cards dealt in a node will be sorted in ascending order.</para>
    /// <para>Position will be set to 0 for all nodes except root. Position of root will be set to 1, because
    /// this is the number of players in this tree.</para>
    /// <para>Pot shares will be set to 0.</para>
    /// </summary>
    public unsafe class ExtractPlayerChanceTree
    {
        #region Public API

        public ChanceTree Extract(ChanceTree ct, int position)
        {
            if (position >= ct.PlayersCount)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Position {0} is out of range (players count: {1})", position, ct.PlayersCount));
            }
            _position = position;
            
            WalkUFTreePP<ChanceTree, Context> wt = new WalkUFTreePP<ChanceTree, Context>();
            wt.OnNodeBegin = OnNodeBegin;

            _nodeCount = 0;
            _tempRoot = new TempNode {  Card = -1, Probab = 1};

            wt.Walk(ct);

            ChanceTree newTree = new ChanceTree(_nodeCount + 1);

            // Reset memory to clear results.
            newTree.SetNodesMemory(0);

            _nodeCount = 0;
            CopyFromTemp(newTree, _tempRoot, 0);            

            // Overwrite root position
            newTree.Nodes[0].Position = 1;

            newTree.Version.Description = String.Format("Player {0} chance tree from {1}", position, ct.Version.Description);

            return newTree;
        }

        public static ChanceTree ExtractS(ChanceTree ct, int position)
        {
            ExtractPlayerChanceTree ect = new ExtractPlayerChanceTree();
            return ect.Extract(ct, position);
        }

        #endregion

        #region Implementation

        class TempNode
        {
            public int Card;
            public double Probab;
            public Dictionary<int, TempNode> Children = new Dictionary<int,TempNode>();
        }

        class Context : WalkUFTreePPContext
        {
            public TempNode CurTempNode;
        }

        void CopyFromTemp(ChanceTree newTree, TempNode tempNode, int depth)
        {
            newTree.SetDepth(_nodeCount, (byte)depth);
            newTree.Nodes[_nodeCount].Card = tempNode.Card;
            newTree.Nodes[_nodeCount].Probab = tempNode.Probab;
            newTree.Nodes[_nodeCount].Position = 0;
            _nodeCount++;
            List<int> cards = new List<int>(tempNode.Children.Keys.ToArray());
            cards.Sort();
            foreach (int card in cards)
            {
                CopyFromTemp(newTree, tempNode.Children[card], depth + 1);
            }
        }

        void OnNodeBegin(ChanceTree tree, Context[] stack, int depth)
        {
            Context context = stack[depth];
            Int64 n = context.NodeIdx;

            if (depth == 0)
            {
                stack[depth].CurTempNode = _tempRoot;
            }
            else
            {
                stack[depth].CurTempNode = stack[depth - 1].CurTempNode;
            }

            if(tree.Nodes[n].Position == _position)
            {
                int card = tree.Nodes[n].Card;
                TempNode childTempNode;
                if (!stack[depth].CurTempNode.Children.TryGetValue(card, out childTempNode))
                {
                    childTempNode = new TempNode { Card = card};
                    stack[depth].CurTempNode.Children.Add(card, childTempNode);
                    _nodeCount++;
                }
                childTempNode.Probab += tree.Nodes[n].Probab;
                stack[depth].CurTempNode = childTempNode;
            }
        }

   
        int _position;
        int _nodeCount;
        TempNode _tempRoot;

        #endregion
    }
}
