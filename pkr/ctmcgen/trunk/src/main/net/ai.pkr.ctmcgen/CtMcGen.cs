/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using ai.lib.algorithms;
using System.Diagnostics;
using ai.lib.algorithms.tree;
using ai.lib.utils;
using System.IO;
using ai.lib.algorithms.strings;

namespace ai.pkr.ctmcgen
{
    /// <summary>
    /// Creates a chance tree by Monte-Carlo sampling. 
    /// Currently supports heads-up games only.
    /// </summary>
    public static unsafe class CtMcGen
    {
        #region Public API

        /// <summary>
        /// Is called periodically to give feedback to the caller.
        /// </summary>
        /// <param name="currentSamplesCount">Current number of samples.</param>
        /// <returns>Return false to stop generation.</returns>
        public delegate bool FeedbackDelegate(Int64 currentSamplesCount);

        /// <summary>
        /// Representation of the tree for internal purposes. Is optimized for MC operations.
        /// </summary>
        public class Tree
        {
            #region Public API

            public Tree()
            {
                Root = new Node(false, DEFAULT_INITIAL_CAPACITY);
                Version = new BdsVersion();
                SourceInfo = "";
            }

            public BdsVersion Version
            {
                get;
                private set;
            }

            public UInt64 SamplesCount
            {
                internal set;
                get;
            }


            public Int64 CalculateLeavesCount()
            {
                int leavesCount = CountLeaves<int>.Count(Root, (object)Root);
                return leavesCount;
            }

            public ChanceTree ConvertToChanceTree()
            {
                int depth = PlayersCount * RoundsCount;

                int nodesCount = CountNodes<int>.Count(Root, Root as object);

                ChanceTree ct = new ChanceTree(nodesCount);
                ct.SetNodesMemory(0); // To clear results.

                SyncUniAndUF<int>.Sync(Root, Root as object, ct, SyncNodes);

                WalkUFTreePP<ChanceTree, WalkUFTreePPContext> wt = new WalkUFTreePP<ChanceTree, WalkUFTreePPContext>();
                wt.OnNodeEnd = FinalizeChanceTree_OnNodeEnd;
                wt.Walk(ct);
                ct.PlayersCount = PlayersCount;
                ct.Version.Description = String.Format("Chance tree (MC:{0:0,0}, {1})", SamplesCount, SourceInfo);
                return ct;
            }

            public void Write(BinaryWriter w)
            {
                WriteInternal(w);
            }

            public void Write(string fileName)
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                {
                    Write(bw);
                }
            }

            #endregion

            #region Internal API

            internal void UpdateDescription()
            {
                Version.Description = String.Format("McCtGen internal chance tree (MC:{0:0,0}, {1})", SamplesCount, SourceInfo);
            }

            internal void UpdateInitialCapacity(int depth, int card)
            {
                if(_initialCapacity[depth] <= card)
                {
                    _initialCapacity[depth] = card + 1;
                }
            }

            internal int GetInitialCapacity(int depth)
            {
                return _initialCapacity[depth];
            }

            #endregion

            #region Implementation

            bool SyncNodes(Node uniTree, object uniNode, byte uniDepth, int uniIt, UFTree ufTree, Int64 ufNode, object userData)
            {
                ChanceTree ct = (ChanceTree)ufTree;
                ct.SetDepth(ufNode, uniDepth);
                ct.Nodes[ufNode].Card = uniDepth == 0 ? 0 : uniIt - 1;
                ct.Nodes[ufNode].Position = (uniDepth - 1) % PlayersCount;
                if (uniNode is LeafT)
                {
                    // This is a leaf
                    LeafT leaf = (LeafT)uniNode;

                    ct.Nodes[ufNode].Probab = (double)leaf.Count / SamplesCount;
                    double [] potShares = new double[2];
                    potShares[0] = 0.5 * leaf.Result / leaf.Count;
                    potShares[1] = 1 - potShares[0];
                    ct.Nodes[ufNode].SetPotShare(0x11, potShares);
                }
                else
                {
                    ct.Nodes[ufNode].Probab = 0;
                }
                return true;
            }

            void FinalizeChanceTree_OnNodeEnd(ChanceTree tree, WalkUFTreePPContext[] stack, int depth)
            {
                if (depth > 0)
                {
                    tree.Nodes[stack[depth - 1].NodeIdx].Probab += tree.Nodes[stack[depth].NodeIdx].Probab;
                }
            }

            class WriteInternalContext: WalkTreePPContext<object, int>
            {}

            private void WriteInternal(BinaryWriter w)
            {
                // Write version first to allow standard tools work.
                Version.Write(w);
                w.Write(SERIALIZATION_FORMAT_VERSION);
                w.Write(PlayersCount);
                w.Write(RoundsCount);
                w.Write(SamplesCount);
                w.Write(SourceInfo);
                Int64 leavesCount = CalculateLeavesCount();
                w.Write(leavesCount);

                UInt64 sumLeavesCount = 0;

                var wt = new WalkTreePP<Node, object, int, WriteInternalContext>();
                wt.OnNodeEnd =
                    (Node tree, object node, List<WriteInternalContext> stack, int depth) =>
                    {
                        int maxDepth = PlayersCount * RoundsCount;
                        if (depth < maxDepth)
                        {
                            return;
                        }
                        byte[] cards = new byte[PlayersCount * RoundsCount];
                        for (int d = 0; d < cards.Length; ++d)
                        {
                            cards[d] = (byte)(stack[d].ChildrenIterator - 1);
                        }
                        LeafT leaf = (stack[depth - 1].Node as Node).Leaves[cards[depth - 1]];
                        w.Write(cards);
                        w.Write(leaf.Count);
                        w.Write(leaf.Result);
                        sumLeavesCount += leaf.Count;
                    };

                wt.Walk(Root, Root);
                VerifyTotalCount(sumLeavesCount);
                UpdateDescription();
            }

            /// <summary>
            /// Merging read a tree from a file.
            /// </summary>
            public void Read(String fileName)
            {
                using (BinaryReader br = new BinaryReader(File.Open(fileName,
                                                                    FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    Read(br);
                }
            }

            /// <summary>
            /// Merging read a tree from a stream.
            /// </summary>
            public void Read(BinaryReader r)
            {
                Version = new BdsVersion();
                Version.Read(r);
                int serFmtVer = r.ReadInt32();
                if (serFmtVer > SERIALIZATION_FORMAT_VERSION)
                {
                    throw new ApplicationException(
                        String.Format("Unsupported serialization format version: file format: {0}, supported: {1}",
                        serFmtVer, SERIALIZATION_FORMAT_VERSION));
                } 
                int playersCount = r.ReadInt32();
                int roundsCount = r.ReadInt32();
                UInt64 samplesCount = r.ReadUInt64();
                string sourceInfo = r.ReadString();

                if (SamplesCount == 0)
                {
                    // This is a virgin tree - copy all the data.
                    PlayersCount = playersCount;
                    RoundsCount = roundsCount;
                    SourceInfo = sourceInfo;
                }
                else
                {
                    if (PlayersCount != playersCount || RoundsCount != roundsCount || SourceInfo != sourceInfo)
                    {
                        throw new ApplicationException(String.Format(
                            "Merge target (pc: {0}, rc: {1}, si: {2}) does not match the merge source (pc: {3}, rc: {4}, si: {5})",
                            PlayersCount, RoundsCount, SourceInfo, playersCount, roundsCount, sourceInfo));
                    }
                }
                UInt64 sumLeavesCount = SamplesCount;
                SamplesCount += samplesCount;

                Int64 leavesCount = r.ReadInt64();
                int cardsCount = RoundsCount * PlayersCount;
                for(Int64 l = 0; l < leavesCount; ++l)
                {
                    byte[] cards = r.ReadBytes(cardsCount);
                    UInt32 count, result;
                    if (serFmtVer == 1)
                    {
                        UInt64 tmp = r.ReadUInt64();
                        if (tmp > UInt32.MaxValue) throw new ApplicationException("Overflow");
                        count = (UInt32)tmp;
                        tmp = r.ReadUInt64();
                        if (tmp > UInt32.MaxValue) throw new ApplicationException("Overflow");
                        result = (UInt32)tmp;
                    }
                    else
                    {
                        count = r.ReadUInt32();
                        result = r.ReadUInt32();
                    }
                    LeafT[] leaves = GetLeavesByCards(cards);
                    int lastCard = cards[cards.Length - 1];
                    leaves[lastCard].IncrementCount(count);
                    leaves[lastCard].IncrementResult(result);
                    sumLeavesCount += count;
                }
                VerifyTotalCount(sumLeavesCount);
            }

            internal LeafT[] GetLeavesByCards(byte[] cards)
            {
                Node curNode = Root;
                for (int ac = 0; ac < cards.Length - 1; ++ac)
                {
                    UpdateInitialCapacity(ac + 1, cards[ac]);
                    curNode = curNode.TouchChildAt(cards[ac], ac == cards.Length - 2, GetInitialCapacity(ac + 2));
                }
                int lastCard = cards[cards.Length - 1];
                UpdateInitialCapacity(cards.Length, lastCard);
                LeafT[] leaves = curNode.TouchLeafAt(lastCard);
                return leaves;
            }

            private void VerifyTotalCount(ulong sumLeavesCount)
            {
                if (SamplesCount != sumLeavesCount)
                {
                    throw new ApplicationException(String.Format("Data inconsistent: TotalCount {0:#,#} is not equal to sum of leave counts {1:#,#}",
                                                                 SamplesCount, sumLeavesCount));
                }
            }

            #endregion

            #region Constants

            /// <summary>
            /// Serialization format.
            /// History:
            /// 1: 64-bit count and result (todo: remove this after 20.10.11)
            /// 2: 32-bit count and result
            /// </summary>
            private const int SERIALIZATION_FORMAT_VERSION = 2;

            #endregion

            #region Data

            internal Node Root;
            internal int PlayersCount;
            internal int RoundsCount;
            internal string SourceInfo;

            const int DEFAULT_INITIAL_CAPACITY = 5;
            int[] _initialCapacity = new int[100].Fill(DEFAULT_INITIAL_CAPACITY);

            #endregion 
        }

        /// <summary>
        /// Generate an internal chance tree by MC sampling.
        /// </summary>
        /// <param name="chanceAbstractions">An array of chance abstractions</param>
        /// <param name="areAbstractionsEqual">If the absractions are equal, 
        /// one MC sample will update multiple nodes of the chance tree.</param>
        /// <param name="samplesCount">Number of samples. If equal absractions are used, 
        /// less MC samples will be actually done to reach the specified numbers of updates.</param>
        /// <param name="rngSeed">RNG seed.</param>
        /// <param name="feedback">User feedback callback.</param>
        /// <returns></returns>
        public static Tree Generate(GameDefinition gd, IChanceAbstraction[] chanceAbstractions, bool areAbstractionsEqual, long samplesCount, int rngSeed, FeedbackDelegate feedback)
        {
            if (chanceAbstractions.Length != 2)
            {
                throw new ArgumentOutOfRangeException("Only heads up games are supported now");
            }
            McDealer mcDealer = new McDealer(gd, rngSeed);
            int [][] hands = new int[chanceAbstractions.Length][].Fill(i => new int[mcDealer.HandSize]);
            int[] handSizes = gd.GetHandSizes();

            Tree tree = new Tree
            {
                PlayersCount = chanceAbstractions.Length,
                RoundsCount = gd.RoundsCount,
                SourceInfo = GetSourceInfo(gd, chanceAbstractions)
            };

            Node root = tree.Root;

            byte[] abstrCards = new byte[gd.RoundsCount * chanceAbstractions.Length];
            uint[] ranks = new uint[chanceAbstractions.Length];
            Int64 samplesDone;
            int updateCount = areAbstractionsEqual ? 2 : 1;

            for (samplesDone = 0; samplesDone < samplesCount; samplesDone += updateCount)
            {
                if (feedback != null && (samplesDone % FEEDBACK_PERIOD == 0))
                {
                    if (!feedback(samplesDone))
                    {
                        break;
                    }
                }

                mcDealer.NextDeal(hands);
                gd.GameRules.Showdown(gd, hands, ranks);

                int c = 0;
                for (int r = 0; r < gd.RoundsCount; ++r)
                {
                    for (int p = 0; p < chanceAbstractions.Length; ++p )
                    {
                        int abstrCard = chanceAbstractions[p].GetAbstractCard(hands[p], handSizes[r]);
                        if(abstrCard < byte.MinValue ||abstrCard > byte.MaxValue)
                        {
                            throw new ApplicationException(string.Format("Abstract card {0} out of byte range", abstrCard));
                        }
                        abstrCards[c++] = (byte)abstrCard;
                    }
                }

                for (int u = 0; ; )
                {
                    LeafT[] leaves = tree.GetLeavesByCards(abstrCards);
                    int lastCard = abstrCards[abstrCards.Length - 1];
                    leaves[lastCard].Update(ranks);

                    if (++u == updateCount)
                    {
                        break;
                    }

                    // If the abstractions are equal, we can update another node 
                    // by permuting cards and results 
                    // Note: implemented for 2 players only
                    for (c = 0; c < abstrCards.Length; c += 2)
                    {
                        ShortSequence.Swap(ref abstrCards[c], ref abstrCards[c+1]);
                    }
                    ShortSequence.Swap(ref ranks[0], ref ranks[1]);
                }
            }
            tree.SamplesCount = (UInt64)samplesDone;
            tree.UpdateDescription();
            return tree;
        }


        #endregion

        #region Implementation

        /// <summary>
        /// Check user feedback every X samples. Now the sample rate on a typical
        /// machine is 30-50 Ks/s, choose the rate for about 1 check per second.
        /// </summary>
        const int FEEDBACK_PERIOD = 50000;


        internal struct LeafT
        {

            public LeafT(UInt32 count, UInt32 result)
            {
                _count = count;
                _result = result;
            }


            /// <summary>
            /// Total visits count for this leaf.
            /// </summary>
            UInt32 _count;

            /// <summary>
            /// Total result from the point of view of player 0. Each win adds 2, each tie: 1.
            /// </summary>
            public UInt32 _result;


            public UInt32 Count
            {
                get
                {
                    return _count;
                }
            }

            public UInt32 Result
            {
                get
                {
                    return _result;
                }
            }

            public void IncrementCount(UInt32 delta)
            {
                UInt32 old = _count;
                _count += delta;
                if (_count < old)
                {
                    throw new ApplicationException(String.Format("Count overflow: cur value: {0}, delta: {1}", _count, delta));
                }
            }

            public void IncrementResult(UInt32 delta)
            {
                UInt32 old = _result;
                _result += delta;
                if (_result < old)
                {
                    throw new ApplicationException(String.Format("Result overflow: cur value: {0}, delta: {1}", _result, delta));
                }
            }
            
            public void Update(uint [] ranks)
            {
                IncrementCount(1);

                if (ranks[0] > ranks[1])
                {
                    // player 0 wins
                    IncrementResult(2);
                }
                else if (ranks[0] == ranks[1])
                {
                    // tie.
                    IncrementResult(1);
                }
            }

            public override string ToString()
            {
                return String.Format("c:{0} r:{1}",Count, Result);
            }
        }

        /// <summary>
        /// A node for internal tree representation. Is optimized for the following use-cases:
        /// 1. Fast insertion of new leaves by given cards.
        /// 2. Fast access to a leave by given cards.
        /// 3. Must support huge trees with 100M+ node with the least memory overhead, taking
        /// into account that multiple instances are running simultaneously.
        /// 
        /// This node contains either an array of child nodes or an array of leaves. Leaves are structs. Only the nodes at the 
        /// maxDepth-1 contain leaves. Both arrays can contain holes, for nodes filled with null, for leaves - with 
        /// zero-counters.
        /// 
        /// The node itself in a class, not a struct, because it makes the implementation easier, but does not save much memory, because
        /// the most of the memory goes for leaves.
        /// </summary>
        internal class Node
        {
            /// <summary>
            /// Standard tree accessor. As we have to access both nodes and leaves, use object as the common class for them.
            /// </summary>
            public static bool TreeGetChild(Node tree, object n, ref int i, out object child)
            {
                child = null;

                Node node = n as Node;
                if (node == null)
                {
                    // This is a leaf.
                    return false;
                }
                Node[] children = node._data as Node[];
                if (children != null)
                {
                    // This node contains children nodes.
                    for (; i < children.Length; )
                    {
                        if (children[i] != null)
                        {
                            child = children[i++];
                            return true;
                        }
                        else
                        {
                            ++i;
                        }
                    }
                }
                else
                {
                    // This node contains leaves
                    LeafT[] leaves = node._data as LeafT[];
                    for (; i < leaves.Length;)
                    {
                        // It this leaf updated at least once?
                        if (leaves[i].Count > 0)
                        {
                            child = leaves[i++];
                            return true;
                        }
                        else
                        {
                            ++i;
                        }
                    }
                }
                return false;
            }

            public Node(bool containsLeaves, int initialCapacity)
            {
                if (containsLeaves)
                {
                    _data = new LeafT[initialCapacity];
                }
                else
                {
                    _data = new Node[initialCapacity];
                }
            }

            public bool ContainsLeaves
            {
                get
                {
                    return (_data as LeafT[]) != null;
                }
            }

            /// <summary>
            /// Makes sure that this child is allocated. If necessary, resizes the array
            /// of children and creates a new object.
            /// </summary>
            public Node TouchChildAt(int index, bool childContainsLeaves, int childInitialCapacity)
            {
                Node[] children = _data as Node[];
                if (children.Length <= index)
                {
                    Array.Resize(ref children, index + 1);
                    _data = children;
                }
                if (children[index] == null)
                {
                    children[index] = new Node(childContainsLeaves, childInitialCapacity);
                }
                return children[index];
            }

            /// <summary>
            /// Makes sure that this leaf. If necessary, resizes the array
            /// of leasve.
            /// </summary>
            internal LeafT[] TouchLeafAt(int absractCard)
            {
                LeafT[] leaves = _data as LeafT[];
                if (leaves.Length <= absractCard)
                {
                    Array.Resize(ref leaves, absractCard + 1);
                    _data = leaves;
                }
                return leaves;
            }

            public LeafT [] Leaves
            {
                get { return _data as LeafT[]; }
            }

            public int Capacity
            {
                get 
                {
                    if (_data == null)
                    {
                        return 0;
                    }
                    if (ContainsLeaves)
                    {
                        return Leaves.Length;
                    }
                    return (_data as Node[]).Length;
                }
            }

            public override string ToString()
            {
                return string.Format("{0}:{1}", ContainsLeaves ? "L" : "N", Capacity);
            }


            /// <summary>
            /// Data of the node.
            /// For parents of leaves this is an array of LeafT.
            /// For other nodes this is an array of Nodes.
            /// </summary>
            object _data;
        }

        static string GetSourceInfo(GameDefinition gd, IChanceAbstraction [] abstractions)
        {
            string s = String.Format("gamedef: {0}", gd.Name);
            for (int p = 0; p < gd.MinPlayers; ++p)
            {
                s += String.Format(", {0}", abstractions[p].Name);
            }
            return s;
        }

        #endregion

    }
}
