/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.metastrategy.algorithms;
using ai.lib.algorithms;
using ai.lib.algorithms.tree;
using System.Diagnostics;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Creates a chance tree for an abstracted game based on the game definition 
    /// and abstractions for each position. It is made for small model games with one card per round.
    /// Only the nodes that really exist in the abstract game will be added (no 0-probability nodes).
    /// The children of each abstract node nodes will be sorted by card in acsending order.
    /// The root node of the original chance tree is copied to the abstract tree as is (except PotShares).
    /// </summary>
    public unsafe class CreateChanceTreeByAbstraction
    {

        public ChanceTree Create(GameDefinition gd, IChanceAbstraction [] abstractions)
        {
            _abstractions = abstractions;
            _gameDef = gd;
            _hands = new int[gd.MinPlayers][].Fill(i => new int[gd.RoundsCount]);
            _activePlayersOne = ActivePlayers.Get(gd.MinPlayers, 1, 1);
            _activePlayersAll = ActivePlayers.Get(gd.MinPlayers, 1, gd.MinPlayers);
            _maxDepth = gd.RoundsCount * gd.MinPlayers;

            ChanceTree gdChanceTree = CreateChanceTreeByGameDef.Create(gd);

            WalkUFTreePP<ChanceTree, CreateIntermediateTreeContext> wt1 = new WalkUFTreePP<ChanceTree, CreateIntermediateTreeContext>();
            wt1.OnNodeBegin = CreateIntermediateTree_OnNodeBegin;
            wt1.OnNodeEnd = CreateIntermediateTree_OnNodeEnd;
            _nodesCount = 0;
            wt1.Walk(gdChanceTree);

            _abstChanceTree  = new ChanceTree(_nodesCount + 1);

            WalkTreePP<IntermediateNode, IntermediateNode, int, CopyTreeContext> wt2 = new WalkTreePP<IntermediateNode, IntermediateNode, int, CopyTreeContext>();
            wt2.OnNodeBegin = CopyTree_OnNodeBegin;
            _nodesCount = 0;
            wt2.Walk(_intRoot, _intRoot);

            _abstChanceTree.Version.Description = String.Format("Chance tree (gamedef: {0}", gd.Name);
            for (int p = 0; p < gd.MinPlayers; ++p)
            {
                _abstChanceTree.Version.Description += String.Format(", {0}", abstractions[p].Name);
            }
            _abstChanceTree.Version.Description += ")";

            return _abstChanceTree;
        }

        public static ChanceTree CreateS(GameDefinition gd, IChanceAbstraction[] abstractions)
        {
            CreateChanceTreeByAbstraction cct = new CreateChanceTreeByAbstraction();
            ChanceTree ct = cct.Create(gd, abstractions);
            return ct;
        }

        class IntermediateNode: IChanceTreeNode, IComparable<IntermediateNode>
        {
            public static bool TreeGetChild(IntermediateNode tree, IntermediateNode n, ref int i, out IntermediateNode child)
            {
                return (i < n.Children.Count ? child = n.Children[i++] : child = null) != null;
            }

            public double Probab
            {
                get {return _probab;}
                set { _probab = value; }
            }

            public void GetPotShare(ushort activePlayers, double[] potShare)
            {
                _potShares[activePlayers].CopyTo(potShare, 0);
            }

            public void AddPotShare(ushort activePlayers, double[] potShare, double probab)
            {
                double []curPotShare;
                if (!_potShares.TryGetValue(activePlayers, out curPotShare))
                {
                    curPotShare = new double[potShare.Length];
                    _potShares.Add(activePlayers, curPotShare);
                }
                double newProbab = _probab + probab;
                if(newProbab > 0)
                {
                    for (int p = 0; p < potShare.Length; ++p)
                    {
                        curPotShare[p] = (curPotShare[p] * _probab + potShare[p] * probab) / newProbab;
                    }
                }
            }

            public int Card
            {
                set;
                get;
            }

            public int Position
            {
                get;
                set;
            }

            public string ToStrategicString(object parameters)
            {
                return "";
            }

            public List<IntermediateNode> Children = new List<IntermediateNode>();

            public IntermediateNode FindOrCreateChild(int abstrCard, ref int nodesCount)
            {
                for (int ch = 0; ch < Children.Count; ++ch)
                {
                    if (Children[ch].Card == abstrCard)
                    {
                        return Children[ch];
                    }
                }
                IntermediateNode newChild = new IntermediateNode();
                Children.Add(newChild);
                nodesCount++;
                return newChild;
            }


            Dictionary<UInt16, double[]> _potShares = new Dictionary<ushort, double[]>();
            double _probab;


            #region IComparable<IntermediateNode> Members

            public int CompareTo(IntermediateNode other)
            {
                if (Card < other.Card)
                {
                    return -1;
                }
                if (Card > other.Card)
                {
                    return 1;
                }
                return 0;
            }

            #endregion
        }

        class CreateIntermediateTreeContext : WalkUFTreePPContext
        {
            public double StrategicProbab = 1.0;
            public IntermediateNode IntNode;
        }

        class CopyTreeContext : WalkTreePPContext<IntermediateNode, int>
        {
        }

        void CreateIntermediateTree_OnNodeBegin(ChanceTree tree, CreateIntermediateTreeContext[] stack, int depth)
        {
            CreateIntermediateTreeContext context = stack[depth];
            Int64 n = context.NodeIdx;
            int round = (depth - 1) / _gameDef.MinPlayers;

            if (depth == 0)
            {
                _intRoot = context.IntNode = new IntermediateNode();
                _intRoot.Position = tree.Nodes[0].Position;
                _intRoot.Card = tree.Nodes[0].Card;
                _intRoot.Probab = tree.Nodes[0].Probab;
                return;
            }
            int curPlayer = tree.Nodes[n].Position;
            int curCard = tree.Nodes[n].Card;
            _hands[curPlayer][round] = curCard;
            int abstrCard = _abstractions[curPlayer].GetAbstractCard(_hands[curPlayer], round + 1);

            context.IntNode = stack[depth-1].IntNode.FindOrCreateChild(abstrCard, ref _nodesCount);

            // Set or update properties of the intermediate node.

            context.IntNode.Position = curPlayer;
            context.IntNode.Card = abstrCard;
            double [] potShare  = new double[_gameDef.MinPlayers];
            UInt16 [] activePlayers = depth == _maxDepth ? _activePlayersAll : _activePlayersOne;
            foreach (UInt16 ap in activePlayers)
            {
                tree.Nodes[n].GetPotShare(ap, potShare);
                context.IntNode.AddPotShare(ap, potShare, tree.Nodes[n].Probab);
            }
            context.IntNode.Probab += tree.Nodes[n].Probab;
        }

        void CreateIntermediateTree_OnNodeEnd(ChanceTree tree, CreateIntermediateTreeContext[] stack, int depth)
        {
            CreateIntermediateTreeContext context = stack[depth];
        }

        bool CopyTree_OnNodeBegin(IntermediateNode tree, IntermediateNode node, List<CopyTreeContext> stack, int depth)
        {
            node.Children.Sort();

            _abstChanceTree.SetDepth(_nodesCount, (byte)depth);
            _abstChanceTree.Nodes[_nodesCount].Position = node.Position;
            _abstChanceTree.Nodes[_nodesCount].Card = node.Card;
            _abstChanceTree.Nodes[_nodesCount].Probab = node.Probab;
            if (_nodesCount > 0)
            {
                double[] potShare = new double[_gameDef.MinPlayers];
                UInt16 [] activePlayers = depth == _maxDepth ? _activePlayersAll : _activePlayersOne;
                foreach (UInt16 ap in activePlayers)
                {
                    node.GetPotShare(ap, potShare);
                    _abstChanceTree.Nodes[_nodesCount].SetPotShare(ap, potShare);
                }
            }
            _nodesCount++;
            return true;
        }

        GameDefinition _gameDef;
        int[][] _hands;
        IChanceAbstraction[] _abstractions;
        UInt16[] _activePlayersAll;
        UInt16[] _activePlayersOne;
        int _nodesCount;
        IntermediateNode _intRoot;
        ChanceTree _abstChanceTree;
        int _maxDepth;
    }
}
