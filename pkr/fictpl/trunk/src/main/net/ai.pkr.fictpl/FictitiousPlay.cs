/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

#if DEBUG

// If defined, prints some info with Debug.Write().
//#define DEBUG_PRINT

#endif

#define USE_CPP_LIB

// This is no more suppoted !!!
//#define USE_CHANCE_MASKS


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.pkr.metastrategy.vis;
using ai.lib.algorithms;
using ai.pkr.metagame;
using System.Runtime.InteropServices;
using System.IO;
using ai.lib.utils;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using System.Reflection;
using System.Diagnostics;
using System.Collections;

using ChanceValueT = System.Single;
using ai.lib.algorithms.parallel;
using System.Threading;
using ai.lib.algorithms.strings;
using ai.lib.algorithms.numbers;

#if DEBUG_PRINT 
#warning Compiling with debug information influencing performance and memory requirements.
#endif

namespace ai.pkr.fictpl
{
    /// <summary>
    /// Fictitious play.
    /// </summary>
    /// Developer nodes.
    /// Terms used in the documentation:
    /// Hero - the player currently playing BR.
    /// Opponent - any non-hero player.
    /// 
    /// Todo:
    /// 
    /// 1. We can make a separate algo computing infos for a CT:
    /// - min, max cards for each round.
    /// - number of final nodes for each round
    /// This algo is necessary for each player tree and for the main ct.
    public unsafe class FictitiousPlay
    {
        #region Public types

        public unsafe class Vis : VisPkrTree<UFToUniAdapter, int, int, Vis.Context>
        {
            public class Context : VisPkrTreeContext<int, int>
            {
                public bool IsLeaf;
                public int IdInActionGroup = -1;
            }

            public Vis()
            {
                CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));

                ShowExpr.Clear();
                ShowExpr.Add(new ExprFormatter("s[d].GvId", "id:{1}"));
            }


            public FictitiousPlay Solver
            {
                set;
                get;
            }

            public int Position
            {
                set;
                get;
            }

            void Show(FictitiousPlay.PlayerTree t)
            {
                UFToUniAdapter adapter = new UFToUniAdapter(t);
                Walk(adapter, 0);
            }

            public static void Show(FictitiousPlay solver, int position, string fileName)
            {
                using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
                {
                    Vis vis = new Vis { Output = w, Solver = solver, Position = position };
                    vis.Show(solver._playerTrees[position]);
                }
            }

            protected override void OnTreeBeginFunc(UFToUniAdapter tree, int root)
            {
                GraphAttributes.label = string.Format("Player tree pos {0}", Position);
                base.OnTreeBeginFunc(tree, root);
            }

            protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int n, List<Context> stack, int depth)
            {
                Context context = stack[depth];
                PlayerTree tree = (PlayerTree)aTree.UfTree;

                context.IsLeaf = true;

                if (depth == 0)
                {
                    context.Round = -1;
                }
                else
                {
                    stack[depth - 1].IsLeaf = false;
                    context.Round = stack[depth - 1].Round;
                    context.IdInActionGroup = stack[depth - 1].IdInActionGroup;
                }
                // Deal action => new round.
                if (tree.Nodes[n].Position == Solver._playersCount)
                {
                    context.Round++;
                    if (Solver._chanceInfos != null && Solver._chanceInfos[Position] != null)
                    {
                        context.IdInActionGroup = Solver._chanceInfos[Position][tree.Nodes[n].ChanceId].IdInActionGroup;
                    }
                }
                int pos = tree.Nodes[n].Position;
                context.IsDealerAction = pos == Solver._playersCount;
                context.Position = pos;
                context.ActionLabel = context.Position.ToString();

                return base.OnNodeBeginFunc(aTree, n, stack, depth);
            }

            protected override void CustomizeNodeAttributes(UFToUniAdapter aTree, int n, List<Context> stack, int depth, NodeAttributeMap attr)
            {
                Context context = stack[depth];
                PlayerTree tree = (PlayerTree)aTree.UfTree;

                base.CustomizeNodeAttributes(aTree, n, stack, depth, attr);
                string nodeLabel = attr.label;
                nodeLabel += string.Format("\\nha:{0}", tree.Nodes[n].IsHeroActing ? 't' : 'f');
                if (Position == tree.Nodes[n].Position && n > 0)
                {
                    // Hero node
                    nodeLabel += string.Format("\\nsv:{0}", tree.Nodes[n].StrVar);
                    //nodeLabel += string.Format("\\nsv:{0:0.0000}", tree.Nodes[n].GetStrVar(Solver));
                }
                if (tree.Nodes[n].IsHeroActing)
                {
                    nodeLabel += string.Format("\\nsc:{0}", tree.Nodes[n].NextNodeToSkipChildren);
                }
                if (tree.Nodes[n].Position == Solver._playersCount)
                {
                    nodeLabel += string.Format("\\nci:{0}", tree.Nodes[n].ChanceId);
                }
                if (context.IsLeaf)
                {
                    UInt32 atIdx = tree.Nodes[n].AtIdx;
                    if (context.IdInActionGroup == -1)
                    {
                        nodeLabel += string.Format("\\ngv:?");
                    }
                    else
                    {
                        double gameValue = Solver._actionGroups[Position][atIdx].GameValues[context.IdInActionGroup];
                        nodeLabel += string.Format("\\ngv:{0}", gameValue);
                    }
                    nodeLabel += string.Format("\\nat:{0}", atIdx);
                }
                if (n == 0)
                {
                    nodeLabel += string.Format("\\ngv:{0:0.000}", Solver.LastSbrValues[Position]);
                }
                attr.label = nodeLabel;
            }

            protected override void CustomizeEdgeAttributes(UFToUniAdapter aTree, int n, int pn, List<Context> stack, int depth, EdgeAttributeMap attr)
            {
                base.CustomizeEdgeAttributes(aTree, n, pn, stack, depth, attr);
                PlayerTree tree = (PlayerTree)aTree.UfTree;

                if (tree.Nodes[pn].IsHeroActing && n == tree.Nodes[pn].BestBrNode)
                {
                    // Hero have chosen this edge - show thick
                    attr.penwidth = 3.0;
                }
            }
        }

        /// <summary>
        /// Information about persistent data of the algorithms. Contains no actual data, but just file names.
        /// </summary>
        public class SnapshotInfo
        {
            public SnapshotInfo(string baseDir, int playersCount, bool equalCa)
            {
                BaseDir = baseDir;
                HeaderFile = Path.Combine(baseDir, GetSnapshotHeaderFileName());
                StrategyFile = new string[playersCount];
                if (equalCa)
                {
                    StrategyFile.Fill(i => Path.Combine(baseDir, "strategy.dat"));
                }
                else
                {
                    StrategyFile.Fill(i => Path.Combine(baseDir, string.Format("strategy-{0}.dat", i)));
                }
                GameValuesFile = new string[playersCount].Fill(i => Path.Combine(baseDir, string.Format("gv-{0}.dat", i)));
                EpsilonLog = Path.Combine(baseDir, "epsilon.log");
                InfoFile = Path.Combine(baseDir, "info.txt");
            }

            public String BaseDir
            {
                get;
                private set;
            }

            public String HeaderFile
            {
                private set;
                get;
            }

            public String[] StrategyFile
            {
                private set;
                get;
            }

            public String[] GameValuesFile
            {
                private set;
                get;
            }

            public String EpsilonLog
            {
                private set;
                get;
            }

            public String InfoFile
            {
                private set;
                get;
            }
        }

        public delegate bool OnIterationDoneDelegate(FictitiousPlay solver);

        #endregion

        #region Public properties

        public string ActionTreeFile
        {
            set;
            get;
        }

        public string ChanceTreeFile
        {
            set;
            get;
        }

        /// <summary>
        /// If true, the chance abstractions for all players are considered to be equal. This allows to optimize the algo.
        /// Default: false.
        /// </summary>
        public bool EqualCa
        {
            set;
            get;
        }

        /// <summary>
        /// Do a limited number of iterations. -1 - no limit (default).
        /// </summary>
        public int MaxIterationCount
        {
            set;
            get;
        }

        /// <summary>
        /// Number of actually done iterations for all positions. Is reset after each start,
        /// for example after loading snapshot.
        /// </summary>
        public int CurrentIterationCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Is called after each iteration, allows the user to interrupt the algo.
        /// The algoritm exits if this delegates returns false.
        /// </summary>
        public OnIterationDoneDelegate OnIterationDone
        {
            set;
            get;
        }

        /// <summary>
        /// Exits when CurrentEpsilon &lt;= Epsilon.
        /// </summary>
        public double Epsilon
        {
            set;
            get;
        }

        /// <summary>
        /// Actual absolute of sum of game values for each positions.
        /// </summary>
        public double CurrentEpsilon
        {
            set;
            get;
        }

        /// <summary>
        /// The path for snapshots containting the results.
        /// </summary>
        public string OutputPath
        {
            set;
            get;
        }

        /// <summary>
        /// Number of snapshots.
        /// </summary>
        public int SnapshotsCount
        {
            set;
            get;
        }


        /// <summary>
        /// Specifies the directory for intermediate trace output. null - no tracing.
        /// </summary>
        public string TraceDir
        {
            set;
            get;
        }

        /// <summary>
        /// If true, prints informational messages to console. Default: false.
        /// </summary>
        public bool IsVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// If IsVerbose is true, prints messages about iteration every N iterations.
        /// Set to 0 to disable iteration messages. Default value: 0.
        /// </summary>
        public int IterationVerbosity
        {
            set;
            get;
        }

        public SnapshotInfo CurrentSnapshotInfo
        {
            get
            {
                return _curSnapshotInfo;
            }
        }

        /// <summary>
        /// For each position: last SBR values against current strategy of the opponents.
        /// Remember that the strategy that was updated last before termination SBR was never played,
        /// therefore the value is outdated.
        /// </summary>
        public double[] LastSbrValues
        {
            get;
            private set;
        }

        /// <summary>
        /// Total number of iterations done for each position. It is preserved after loading snapshot.
        /// </summary>
        public int[] IterationCounts
        {
            get;
            private set;
        }

        public int TotalIterationCount
        {
            get
            {
                int sum = IterationCounts.Aggregate((a, v) => a + v);
                return sum;
            }
        }

        /// <summary>
        /// Number of threads in a thread pool for parallel execution. 
        /// The main thread is not included in this count.
        /// Default: 0, in this case no thread pool is created.
        /// </summary>
        public int ThreadsCount
        {
            set;
            get;
        }


        /// <summary>
        /// Add new entry to EpsilonLog if CurrentEpsilon &lt;= previous-epsilon * EpsilonLogThreshold.
        /// Must be less than 1. Default: 0.1.
        /// </summary>
        public double EpsilonLogThreshold
        {
            set;
            get;
        }

        #endregion

        #region Public methods

        public FictitiousPlay()
        {
            MaxIterationCount = -1;
            Epsilon = 0.001;
            // Set to a small value to reduce noise in tests
            EpsilonLogThreshold = 0.1;
            ThreadsCount = 0;
            SnapshotsCount = 2;
            OutputPath = "./FictPlay";
            //JobsPerThread = 1;
        }

        public void Solve()
        {
            Initialize();
            DoIterations();
            SwitchSnapshot();
            SaveSnapshot();
            FreeActionGroupsAndChanceFactors();
            SaveStrategies();
            CleanUp();
        }

        #endregion

        #region Implementation - main data types

        class EpsilonLogEntry
        {
            public double Epsilon;
            public double TotalIterationsCount;
        }

        const byte MASK_POSITION = 0x7F;

        /// <summary>
        /// Node of the players tree. Is made public to do some unit-testing.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct Node
        {
            public const UInt32 INVALID_AT_IDX = 0xFFFFFFFF;
            #region Data

            #region Union 1
            /// <summary>
            /// For nodes of the hero - value of the strategic variable.
            /// </summary>
            [FieldOffset(0)]
            public UInt32 StrVar;

            /// <summary>
            /// For nodes where the hero is acting (pos of children  == hero pos): 
            /// Id of the best BR node.
            /// </summary>
            [FieldOffset(0)]
            public UInt32 BestBrNode;

            #endregion

            #region Union 2

            /// <summary>
            /// In leaves: index in the action tree, used to link variables to action groups.
            /// </summary>
            [FieldOffset(4)]
            public UInt32 AtIdx;


            /// <summary>
            /// For nodes where the hero is acting (pos of children  == hero pos): 
            /// Id of the node following in pre-order after all ancestors (also non-direct) of this node.
            /// </summary>
            [FieldOffset(4)]
            public UInt32 NextNodeToSkipChildren;

            #endregion

            /// <summary>
            /// For nodes where the dealer has acted: ID of the chance info.
            /// <para>During initialization is used temporarily to store the card in deal nodes.</para>
            /// </summary>
            [FieldOffset(8)]
            public UInt32 ChanceId;

            /// <summary>
            /// Encoded info about player positions.
            /// bits 0-6: player position. For dealer: _playerCount, this is the simplest and fastest encoding.
            /// bit 7: if set, the hero is acting (position of children == hero position).
            /// </summary>
            [FieldOffset(12)]
            public byte PositionInfo;

            #endregion

            #region Properties
            /// <summary>
            /// Position: player position for the player, _playerCount for the dealer.
            /// </summary>
            public byte Position
            {
                set
                {
                    unchecked
                    {
                        PositionInfo &= (byte)~MASK_POSITION;
                        PositionInfo |= value;
                    }
                }
                get { return (byte)(PositionInfo & MASK_POSITION); }
            }
            /// <summary>
            /// Returns true if the hero is acting is this node (position of all children == hero position).
            /// </summary>
            public bool IsHeroActing
            {
                set
                {
                    if (value)
                    {
                        PositionInfo |= (byte)0x80u;
                    }
                    else
                    {
                        unchecked
                        {
                            PositionInfo &= (byte)~0x80u;
                        }
                        ;
                    }
                }
                get
                {
                    return (PositionInfo & 0x80) != 0;
                }
            }
            public bool IsLeaf
            {
                get { return AtIdx != INVALID_AT_IDX; }
            }
            #endregion
            #region Methods
            /// <summary>
            /// A helper method converting internal game value format to the real
            /// game value. Is not designed for massive fast calculations.
            /// </summary>
            /* public double GetGameValue(EqFictiousPlayAbstr3 solver, int heroPos)
             {
                 double v = 0;
                 Debug.Assert(solver.GameDef.MinPlayers == 2);
                 int oppPos = 1 - heroPos;
                 if (solver.IterationCounts[oppPos] != 0)
                 {
                     fixed (RealT* pValues = GameValuesInternal)
                     {
                         v = pValues[heroPos] / solver.IterationCounts[oppPos];
                     }
                 }
                 return v;
             }*/

            public double GetStrVar(FictitiousPlay solver)
            {
                if (solver.IterationCounts[Position] == 0)
                {
                    return 0;
                }
                double value = (double)StrVar / solver.IterationCounts[Position];
                return value;
            }

            public void SetStrVar(FictitiousPlay solver, double value)
            {
                UInt32 intVarValue = (UInt32)(value * solver.IterationCounts[Position] + 0.1);
                StrVar = intVarValue;
                Debug.Assert(FloatingPoint.AreEqual(value, GetStrVar(solver), 0.000001));
            }

            #endregion
        }

        unsafe class PlayerTree : UFTree
        {
            public PlayerTree()
            {
            }

            public PlayerTree(Int64 nodesCount)
                : base(nodesCount, Marshal.SizeOf(typeof(Node)))
            {
                // Clear the memory to ensure default values of all fields
                SetNodesMemory(0);
                SetDepthsMemory(0);
                AfterRead();
            }

            protected override void WriteUserData(BinaryWriter w)
            {
            }

            protected override void ReadUserData(BinaryReader r)
            {
            }

            protected override void AfterRead()
            {
                Nodes = (Node*)_nodesPtr.Ptr.ToPointer();
            }
            /// <summary>
            /// Pointer to nodes. Use a field to speed-up (used very often).
            /// </summary>
            public Node* Nodes;
        }

        struct ChanceInfoEntryT
        {
            public UInt32 ChanceMaskIdx;
            public UInt32 ChanceFactorIdx;
            /// <summary>
            /// This is the id of the leaves of the player tree in the corresponding action group.
            /// This id is valid for all leaves below this dealer node and having the same round.
            /// </summary>
            public int IdInActionGroup;
        };

        enum CfKind
        {
            NoSd = 0,
            Sd = 1,
            _Count = 2
        }

        struct ActionGroup
        {
            public double PotFactor;
            
            public CfKind ChanceInfoKind;

            /// <summary>
            /// Game value without pot factor.
            /// </summary>
            public double * GameValues;
            public int GameValuesLength;
            IntPtr _unalignedPtr;

            internal void Allocate(int length)
            {
                // Allocate memory aligned at 16-byte addresses
                // Make sure the size is aligned at 4 elements and 16 bytes (required for SSE code in cpp lib).
                GameValuesLength = length;
                length = (length + 3) & (~0x3);
                Int64 byteSize = length * sizeof(double) + 15;
                _unalignedPtr = UnmanagedMemory.AllocHGlobalEx(byteSize);
                UnmanagedMemory.SetMemory(_unalignedPtr, byteSize, 0);
                GameValues = (double*)((_unalignedPtr.ToInt64() + 15) & (~0xFL));
            }

            internal void Free()
            {
                if (_unalignedPtr != IntPtr.Zero)
                {
                    UnmanagedMemory.FreeHGlobal(_unalignedPtr);
                    _unalignedPtr = IntPtr.Zero;
                    GameValues = null;
                    GameValuesLength = 0;
                }
            }
        }

        /// <summary>
        /// A node in the top tree. Such a node is created for each node with depth == 1 in a player tree.
        /// This is always a deal node. This class calculates the game value in its own subtree of the player tree.
        /// This can be done is a separate thread.
        /// The total game value is the sum of the game values of all top tree nodes.
        /// </summary>
        class TopTreeNode
        {
            public TopTreeNode(FictitiousPlay solver, int heroPos, Int64 rootNode)
            {
                Debug.Assert(solver._playerTrees[heroPos].Nodes[rootNode].Position == solver._playersCount, 
                    "The root must be a deal node");
                _solver = solver;
                _heroPos = heroPos;
                Job.Param1 = rootNode;
                Job.Execute = BestResponseValuesUp;
                _wt.OnNodeBegin = BestResponseValuesUp_OnNodeBegin;
                _wt.OnNodeEnd = BestResponseValuesUp_OnNodeEnd;
                _startDepth = 1 + solver._playersCount;
            }

            public ThreadPoolBase.Job<Int64> Job = new ThreadPoolBase.Job<Int64>();
            public double GameValue;

            int _startDepth;
            FictitiousPlay _solver;
            int _heroPos;
            WalkUFTreePP<PlayerTree, BestResponseValuesUpContext> _wt = new WalkUFTreePP<PlayerTree, BestResponseValuesUpContext>();
            ActionGroup * _actionGroup;

            void BestResponseValuesUp(Int64 rootNode)
            {
                fixed(ActionGroup * pAg = &(_solver._actionGroups[_heroPos][0]))
                {
                    _actionGroup = pAg;
                    _wt.Walk(_solver._playerTrees[_heroPos], rootNode);
                }
            }

            class BestResponseValuesUpContext : WalkUFTreePPContext
            {
                public double GameValue;
                public int IdInActionGroup = -1;
            }

            void BestResponseValuesUp_OnNodeBegin(PlayerTree t, BestResponseValuesUpContext[] s, int d)
            {
                Debug.Assert(d > 0, "Root cannot be processed in the top tree");
                BestResponseValuesUpContext c = s[d];
                Node* pNode = t.Nodes + c.NodeIdx;
                int pos = pNode->PositionInfo & MASK_POSITION; // Inline to speed up
                if (pos == _solver._playersCount)
                {
                    // Dealer
                    c.IdInActionGroup = _solver._chanceInfos[_heroPos][pNode->ChanceId].IdInActionGroup;
                }
                else
                {
                    c.IdInActionGroup = s[d - 1].IdInActionGroup;
                }
            }


            void BestResponseValuesUp_OnNodeEnd(PlayerTree t, BestResponseValuesUpContext[] s, int d)
            {
                Debug.Assert(d > 0, "Root cannot be processed in the top tree");
                BestResponseValuesUpContext c = s[d];
                Node* pNode = t.Nodes + c.NodeIdx;
                if (c.ChildrenCount == 0)
                {
                    // A leaf
                    ActionGroup * pAg = _actionGroup + pNode->AtIdx;
                    c.GameValue = pAg->GameValues[c.IdInActionGroup] * pAg->PotFactor;
                }
                if (d > _startDepth)
                {
                    BestResponseValuesUpContext pc = s[d - 1];
                    int pos = pNode->PositionInfo & MASK_POSITION; // Inline to speed up
                    if (pos == _heroPos)
                    {
                        // Maximize
                        // Allow equal values overwrite. As the moves are sorted f, c, r, the most agressive move will win.
                        if (pc.ChildrenCount == 1 || pc.GameValue <= c.GameValue)
                        {
                            pc.GameValue = c.GameValue;
                            t.Nodes[pc.NodeIdx].BestBrNode = (UInt32)c.NodeIdx;
                        }
                    }
                    else
                    {
                        // Sum up
                        if (pc.ChildrenCount == 1)
                        {
                            pc.GameValue = c.GameValue;
                        }
                        else
                        {
                            pc.GameValue += c.GameValue;
                        }
                    }
                }
                else
                {
                    double gameValue = _solver.ConvertGameValue(c.GameValue, _heroPos);
                    GameValue = gameValue;
                }
            }
        }

        /// <summary>
        /// A top of the main player tree used for parallel processing.
        /// </summary>
        class TopTree
        {
            public List<TopTreeNode> Nodes = new List<TopTreeNode>();
            internal double SumAndClearValues()
            {
                double sum = 0;
                for(int i = 0; i < Nodes.Count; ++i)
                {
                    sum += Nodes[i].GameValue;
                    Nodes[i].GameValue = 0;
                }
                return sum;
            }
        }

        #endregion

        #region Implementation - main data fields

        List<EpsilonLogEntry> _epsilonLog = new List<EpsilonLogEntry>();
        SnapshotSwitcher _snapshotSwitcher;
        private SnapshotInfo _curSnapshotInfo;
        /// <summary>
        /// A copy of last loaded snapshot info.
        /// </summary>
        private SnapshotInfo _loadedSnapshotInfo;
        int _playersCount;
        PlayerTree[] _playerTrees;
        TopTree[] _topTrees;

#if USE_CHANCE_MASKS
        /// <summary>
        /// For each player: bit masks of possible card comibnations.
        /// </summary>
        FastBitArray[] _chanceMasks;
#endif
        struct ChanceFactorArray
        {
            public ChanceValueT * Data;
            IntPtr _unalignedPtr;

            public uint Length
            {
                get;
                private set;
            }

            internal void Allocate(uint length)
            {
                // Allocate memory aligned at 16-byte addresses
                // Make sure the size is aligned at 4 elements and 16 bytes (required for SSE code in cpp lib).
                Length = length;
                length = (length + 3) & (~0x3U);
                Int64 byteSize = length * sizeof(ChanceValueT) + 15;
                _unalignedPtr = UnmanagedMemory.AllocHGlobalEx(byteSize);
                UnmanagedMemory.SetMemory(_unalignedPtr, byteSize, 0);
                Data = (ChanceValueT*)((_unalignedPtr.ToInt64() + 15) & (~0xFL));
            }

            internal void Free()
            {
                if (_unalignedPtr != IntPtr.Zero)
                {
                    UnmanagedMemory.FreeHGlobal(_unalignedPtr);
                    _unalignedPtr = IntPtr.Zero;
                    Data = null;
                    Length = 0;
                }
            }
        }
        /// <summary>
        /// For each player, each kind (non-showdown, showdown): chance factors 
        /// <para>For non-showdown chanceFactor == chanceProbab.</para>
        /// <para>For showdown:</para>
        /// <para>result = (pot * potShare - inPot) * chanceProbab</para>
        /// <para>For 2 players pot == 2*inPot => pot * potShare - inPot = 2 * inPot * potShare - inPot = inPot * (2*potShare - 1)</para>
        /// <para>So, chanceFactor = chanceProbab * ( 2 * potShare - 1)</para>
        /// </summary>
        ChanceFactorArray[][] _chanceFactors;

        double _minCfOrder = double.MaxValue;
        double _maxCfOrder = double.MinValue;

        /// <summary>
        /// For each player, deal node: indexes in _chanceMasks and _chanceFactors.
        /// </summary>
        ChanceInfoEntryT[][] _chanceInfos;
        
        /// <summary>
        /// For each player: an array of his action group. Array length equals to 
        /// At.NodesCount. Some elements (for non-leaves) are empty.
        /// </summary>
        ActionGroup [][]_actionGroups;

        /// <summary>
        /// Position of the player currently doing BR.
        /// </summary>
        int _heroPos;

        private double _iterationTime;

        AssigningThreadPool _threadPool;

        #endregion

        #region Implementation - initialization

        static FictitiousPlay()
        {
            CppLib.Init();
        }

        struct ChanceTreeIndexEntry
        {
            /// <summary>
            /// Contains stringizised card paths for each player. 
            /// First N cards - player 0, next N - player 1, etc.
            /// </summary>
            public string Key;

            /// <summary>
            /// Index of the node in the chance tree.
            /// </summary>
            public UInt32 NodeIdx;

            public override string ToString()
            {
                return string.Format("{0} -> {1}", Key, NodeIdx);
            }
        }

        /// <summary>
        /// Contains data required for initialization only
        /// </summary>
        class InitData
        {

            public InitData(FictitiousPlay solver)
            {
                ChanceTree ct = ChanceTree.Read<ChanceTree>(solver.ChanceTreeFile);
                At = ActionTree.Read<ActionTree>(solver.ActionTreeFile);

                if (ct.PlayersCount != At.PlayersCount)
                {
                    throw new ApplicationException(String.Format("Player count mismatch: at: {0}, ct: {1}",
                                                                 At.PlayersCount, ct.PlayersCount));
                }

                if (solver.IsVerbose)
                {
                    Console.WriteLine("Chance tree: {0}", ct.Version.ToString());
                    Console.WriteLine("Action tree: {0}", At.Version.ToString());
                }

                int playersCount = ct.PlayersCount;

                RoundsCount = ct.CalculateRoundsCount();

                AtChildrenIndex = new UFTreeChildrenIndex(At);

                PlayerTreeMaxDepth = new int[playersCount];

                PlayerCts = new ChanceTree[playersCount];

                PlayerCtMinCard = new int[playersCount][];
                PlayerCtMaxCard = new int[playersCount][];

                PlayerCtNodesCount = new int[playersCount][].Fill(i => new int[RoundsCount]);

                PlayerCtKeys = new string[playersCount][][];

                for (int p = 0; p < playersCount; ++p)
                {
                    PlayerCtMinCard[p] = new int[RoundsCount].Fill(int.MaxValue);
                    PlayerCtMaxCard[p] = new int[RoundsCount].Fill(int.MinValue);
                    PlayerCts[p] = ExtractPlayerChanceTree.ExtractS(ct, p);

                    for (int n = 1; n < PlayerCts[p].NodesCount; ++n)
                    {
                        int round = (int) PlayerCts[p].GetDepth(n) - 1;
                        int card = PlayerCts[p].Nodes[n].Card;
                        PlayerCtMinCard[p][round] = Math.Min(PlayerCtMinCard[p][round], card);
                        PlayerCtMaxCard[p][round] = Math.Max(PlayerCtMaxCard[p][round], card);
                        PlayerCtNodesCount[p][round]++;
                    }
                    PlayerCtKeys[p] = new string[RoundsCount][];
                    for (int r = 0; r < RoundsCount; ++r)
                    {
                        PlayerCtKeys[p][r] = new string[PlayerCtNodesCount[p][r]];
                    }
                    BuildPlayerCtKeys(p);
                }

                if (solver.EqualCa)
                {
                    // Verify that the player chance trees are equal (they can still have different probabilities and results).
                    for (int p = 1; p < playersCount; ++p)
                    {
                        bool treesEqual = CompareUFTrees<ChanceTree, ChanceTree>.CompareS(PlayerCts[0], PlayerCts[p],
                                                                                          (t1, t2, n) =>
                                                                                          t1.Nodes[n].Card ==
                                                                                          t2.Nodes[n].Card);
                        if (!treesEqual)
                        {
                            throw new ApplicationException(String.Format("Player chance tree pos 0 and {0} differ", p));
                        }
                    }
                }
            }

            /// <summary>
            /// Loads Ct and builds index. Ct requires a lot of memory, so it can be loaded only for the time it is required.
            /// </summary>
            public void LoadCt(FictitiousPlay solver)
            {
                Ct = ChanceTree.Read<ChanceTree>(solver.ChanceTreeFile);

                int playersCount = Ct.PlayersCount;
                BuildCtIndex(solver, playersCount);

                ChanceMasksCount = new uint[playersCount];
                ChanceFactorsCount = new uint[playersCount];

                for (int heroPos = 0; heroPos < playersCount; ++heroPos)
                {
                    for (int r = 0; r < RoundsCount; ++r)
                    {
                        for (int oppPos = 0; oppPos < playersCount; ++oppPos)
                        {
                            if (heroPos == oppPos)
                            {
                                continue;
                            }
                            int oppCtNodesCount = PlayerCtNodesCount[oppPos][r];
                            oppCtNodesCount = (oppCtNodesCount + 3) & (~0x3); // 4-blocks alignment for SSE commands in cpp lib
                            ChanceFactorsCount[heroPos] += (UInt32)(PlayerCtNodesCount[heroPos][r] * oppCtNodesCount);
                        }
                    }
                }
            }

            public void UnloadCt()
            {
                Ct.Dispose();
                Ct = null;
                ChanceTreeIndex = null;
            }

            class BuildChanceTreeIndexContext : WalkUFTreePPContext
            {
                /// <summary>
                /// Cards for each player.
                /// </summary>
                public List<int>[] Cards;
            }

            private void BuildCtIndex(FictitiousPlay solver, int playersCount)
            {
                if (solver.IsVerbose)
                {
                    Console.WriteLine("Building CT index");
                }
                CtFinalNodesCount = new int[RoundsCount];
                for (int n = 1; n < Ct.NodesCount; ++n)
                {
                    int d = (int)Ct.GetDepth(n);
                    if (d % playersCount == 0)
                    {
                        int round = (d - 1) / playersCount;
                        CtFinalNodesCount[round]++;
                    }
                }
                ChanceTreeIndex = new ChanceTreeIndexEntry[RoundsCount][];
                for (int r = 0; r < RoundsCount; ++r)
                {
                    ChanceTreeIndex[r] = new ChanceTreeIndexEntry[CtFinalNodesCount[r]];
                }
                UInt32[] count = new uint[RoundsCount];
                WalkUFTreePP<ChanceTree, BuildChanceTreeIndexContext> wt = new WalkUFTreePP<ChanceTree, BuildChanceTreeIndexContext>();
                wt.OnNodeBegin = (t, s, d) =>
                 {
                     if (s[d].Cards == null)
                     {
                         s[d].Cards = new List<int>[playersCount].Fill(i => new List<int>());
                     }
                     if (d == 0)
                     {
                         return;
                     }

                     for (int p = 0; p < s[d].Cards.Length; ++p)
                     {
                         s[d].Cards[p].Clear();
                         s[d].Cards[p].AddRange(s[d - 1].Cards[p]);
                     }
                     int card = t.Nodes[s[d].NodeIdx].Card;
                     int pos = t.Nodes[s[d].NodeIdx].Position;
                     s[d].Cards[pos].Add(card);
                     if (d % playersCount == 0)
                     {
                         int round = (d - 1) / playersCount;
                         for (int p = 0; p < playersCount; ++p)
                         {
                             ChanceTreeIndex[round][count[round]].Key += CardsToKey(s[d].Cards[p]);
                         }
                         ChanceTreeIndex[round][count[round]].NodeIdx += (UInt32)s[d].NodeIdx;
                         count[round]++;
                     }
                 };
                wt.Walk(Ct);
            }

            class BuildPlayerCtKeysContext : WalkUFTreePPContext
            {
                public List<int> Cards = new List<int>();
            }

            private void BuildPlayerCtKeys(int p)
            {
                UInt32 [] count = new UInt32[RoundsCount];
                WalkUFTreePP<ChanceTree, BuildPlayerCtKeysContext> wt =
                    new WalkUFTreePP<ChanceTree, BuildPlayerCtKeysContext>();
                wt.OnNodeBegin = (t, s, d)
                    =>
                    {
                        Int64 n = s[d].NodeIdx;
                        if (d == 0)
                        {
                            return;
                        }
                        s[d].Cards.Clear();
                        s[d].Cards.AddRange(s[d - 1].Cards);
                        int round = d - 1; 
                        int card = t.Nodes[n].Card;
                        s[d].Cards.Add(card);
                        PlayerCtKeys[p][round][count[round]] = CardsToKey(s[d].Cards);
                        count[round]++;
                    };
                wt.Walk(PlayerCts[p]);
            }

            class ChanceTreeIndexEntryComparer : IComparer<ChanceTreeIndexEntry>
            {
                public ChanceTreeIndexEntryComparer(int heroPos, int round)
                {
                    _heroPos = heroPos;
                    // One card per round, each card occupies 1 char.
                    _playerCardsLength = round + 1;
                }

                public int Compare(ChanceTreeIndexEntry x, ChanceTreeIndexEntry y)
                {
                    // Sort first by cards of the hero
                    int startHero = _heroPos * _playerCardsLength;
                    int r = string.Compare(x.Key, startHero, y.Key, startHero, _playerCardsLength, StringComparison.Ordinal);
                    if (r != 0)
                    {
                        return r;
                    }
                    // Sort by the rest of players.
                    for (int start = 0; start < x.Key.Length; start += _playerCardsLength)
                    {
                        if (start == startHero)
                        {
                            continue;
                        }
                        r = string.Compare(x.Key, start, y.Key, start, _playerCardsLength, StringComparison.Ordinal);
                        if (r != 0)
                        {
                            return r;
                        }
                    }
                    return 0;
                }
                readonly int _heroPos;
                readonly int _playerCardsLength;
            }

            public void SortChanceTreeIndex(int pos)
            {
                for (int r = 0; r < RoundsCount; ++r)
                {
                    Array.Sort(ChanceTreeIndex[r], new ChanceTreeIndexEntryComparer(pos, r));
                }
            }
            
            public int RoundsCount;
            
            public ActionTree At;
            public ChanceTree[] PlayerCts;
            /// <summary>
            /// For each player, each round - minimal card in the player's chance tree.
            /// </summary>
            public int[][] PlayerCtMinCard;
            public int[][] PlayerCtMaxCard;

            public UInt32[] ChanceMasksCount;
            public UInt32[] ChanceFactorsCount;

            /// <summary>
            /// For the ct of the game: for each round - number of chance nodes where all cards for this rounds are dealt.
            /// </summary>
            public int[] CtFinalNodesCount;

            /// <summary>
            /// For each player's ct, for each round - number of nodes.
            /// </summary>
            public int[][] PlayerCtNodesCount;

            /// <summary>
            /// For each player, round: an array of card keys for the player chance tree, in pre-order.
            /// </summary>
            public string[][][] PlayerCtKeys;


            /// <summary>
            /// Returns number of cards in a player's chance tree assuming there is no holes.
            /// </summary>
            public int GetPlayerCtCardsCount(int player, int round)
            {
                return PlayerCtMaxCard[player][round] - PlayerCtMinCard[player][round] + 1;
            }

            public int[] PlayerTreeMaxDepth;

            /// <summary>
            /// Chance tree. Is loaded by LoadCt().
            /// </summary>
            public ChanceTree Ct;

            /// <summary>
            /// Chance tree index. Is created by LoadCt().
            /// For each round - an array of entries pointing to the chance tree.
            /// Will be sorted for each player.
            /// </summary>
            public ChanceTreeIndexEntry[][] ChanceTreeIndex;

            public UFTreeChildrenIndex AtChildrenIndex;

        }

        InitData _init;

        static private string CardsToKey(List<int> cards)
        {
            StringBuilder sb = new StringBuilder();
            int cardsLength = cards.Count;
            for (int i = 0; i < cardsLength; )
            {
                char ch = (char)(cards[i++]);
                sb.Append(ch);
                /* Old variant used 2 chars per card:
                sb.AppendFormat("{0:00}", cards[i++]);
                */
            }
            return sb.ToString();
        }

        static string GetSnapshotHeaderFileName()
        {
            return string.Format("header.txt");
        }

        void Initialize()
        {
            DateTime startTime = DateTime.Now;

            _init = new InitData(this);
            _playersCount = _init.At.PlayersCount;
            _epsilonLog = new List<EpsilonLogEntry>();
            _snapshotSwitcher = new SnapshotSwitcher(OutputPath, GetSnapshotHeaderFileName(), SnapshotsCount);
            _curSnapshotInfo = new SnapshotInfo(_snapshotSwitcher.CurrentSnapshotPath, _playersCount, EqualCa);
            IterationCounts = new int[_playersCount];
            LastSbrValues = new double[_playersCount];

            bool isNewSnapshot = !_snapshotSwitcher.IsSnapshotAvailable;
            if (isNewSnapshot)
            {
                CreateNewSnapshot();
            }
            LoadSnapshot();

            _playerTrees = new PlayerTree[_playersCount];
            _topTrees = new TopTree[_playersCount].Fill(i => new TopTree());
#if USE_CHANCE_MASKS
            _chanceMasks = new FastBitArray[_playersCount];
#endif
            _chanceFactors = new ChanceFactorArray[_playersCount][];
            _chanceInfos = new ChanceInfoEntryT[_playersCount][];
            _actionGroups = new ActionGroup[_playersCount][];

            CreatePlayerTrees();

            for (int p = 0; p < _playersCount; ++p)
            {
                if (TraceDir != null)
                {
                    Vis.Show(this, p, GetTraceFileName(p, "tree", "init-pt", "gv"));
                    VisChanceTree.Show(_init.PlayerCts[p], GetTraceFileName(p, "pct", "", "gv"));
                }
            }

            _init.LoadCt(this);

            for (int p = 0; p < _playersCount; ++p)
            {
                CreateChanceInfo(p);
            }

            _init.UnloadCt();

            for (int p = 0; p < _playersCount; ++p)
            {
                CreateSkipChildIndexes(p);
                CreateActionGroups(p);
            }



            if (!isNewSnapshot)
            {
                for (int p = 0; p < _playersCount; ++p)
                {
                    LoadGameValues(p);
                    // This can be used to go to a new version with a new game value format.
                    // SetGameValuesOpp(p);
                }
            }
            else
            {
                // We have to set the game values for opponents for the initial strategy
                for (int p = 0; p < _playersCount; ++p)
                {
                    SetGameValuesOpp(p);
                }
            }

            PrintInitDone();

            if (ThreadsCount > 0)
            {
                // Create thread pool only if we really need it, 
                // use single threaded mode for debugging and profiling.
                _threadPool = new AssigningThreadPool(ThreadsCount);
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                if (IsVerbose)
                {
                    Console.WriteLine("Thread pool is created, threads: {0}", ThreadsCount);
                }
            }
            else
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Single-threaded mode");
                }
            }


            // Clean-up
            _init = null;
            double time = (DateTime.Now - startTime).TotalSeconds;
            if (IsVerbose)
            {
                Console.WriteLine("Initialization done in {0:0.0} s", time);
            }
        }

        private void PrintInitDone()
        {
            if (TraceDir != null)
            {
                for (int p = 0; p < _playersCount; ++p)
                {
                    Vis.Show(this, p, GetTraceFileName(p, "tree", "init-done", "gv"));
                    PrintChanceInfo(p, GetTraceFileName(p, "ci", "", "txt"));
                    PrintActionGroups(p, GetTraceFileName(p, "ag", "", "txt"));
                }
            }
            if (IsVerbose)
            {

                for(int p= 0; p < _playersCount; ++p)
                {
                    int agCount = 0;
                    Int64 agTotalSize = 0;
                    for (int i = 0; i < _actionGroups[p].Length; ++i)
                    {
                        if (_actionGroups[p][i].GameValues != null)
                        {
                            agCount++;
                            agTotalSize += _actionGroups[p][i].GameValuesLength;
                        }
                    }
                    int cfCount = 0;
                    for (int i = 0; i < (int)CfKind._Count; ++i)
                    {
                        cfCount += (int)_chanceFactors[p][i].Length;
                    }
                    uint chanceMasksCount = 0;
#if USE_CHANCE_MASKS
                    chanceMasksCount = _chanceMasks[p].Length;
#endif

                    Console.WriteLine("Data structures pos {0}:", p);
                    Console.WriteLine("Player tree nodes: {0:#,#}, chance infos: {1:#,#}, chance masks: {2:#,#}, chance factors: {3: #,#}", 
                                      _playerTrees[p].NodesCount,  
                                      _chanceInfos[p].Length,
                                      chanceMasksCount,
                                      cfCount);
                    Console.WriteLine("Action groups: count: {0:#,#}, total leaves: {1:#,#}", agCount, agTotalSize);
                    Console.WriteLine("Chance factors order: min: {0}, max: {1}", _minCfOrder, _maxCfOrder);
                }
            }
        }

        class CreatePlayerTreeContext : WalkUFTreePPContext
        {
            public UInt32 AtIdx;
        }

        private void CreatePlayerTrees()
        {
            StrategyTree st = null;
            for (int heroPos = 0; heroPos < _playersCount; ++heroPos)
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Creating player tree for pos {0}", heroPos);
                }
                if (!EqualCa || heroPos == 0)
                {
                    st = StrategyTree.Read<StrategyTree>(_curSnapshotInfo.StrategyFile[heroPos]);
                }
                _playerTrees[heroPos] = new PlayerTree(st.NodesCount);
                WalkUFTreePP<StrategyTree, CreatePlayerTreeContext> wt =
                    new WalkUFTreePP<StrategyTree, CreatePlayerTreeContext>();
                wt.OnNodeBegin = (t, s, d) =>
                                     {
                                         Int64 n = s[d].NodeIdx;
                                         if (d == 0)
                                         {
                                             s[d].AtIdx = 0;
                                         }
                                         else
                                         {
                                             s[d].AtIdx = s[d - 1].AtIdx;
                                         }
                                         _playerTrees[heroPos].SetDepth(n, (byte) d);
                                         if (d > _init.PlayerTreeMaxDepth[heroPos])
                                             _init.PlayerTreeMaxDepth[heroPos] = d;
                                         if (st.Nodes[n].IsDealerAction)
                                         {
                                             _playerTrees[heroPos].Nodes[n].Position = (byte) _playersCount;
                                             _playerTrees[heroPos].Nodes[n].ChanceId = (UInt32) st.Nodes[n].Card;
                                         }
                                         else if (n > 0)
                                         {
                                             s[d].AtIdx = FindActionTreeNodeIdx(t, n, s[d].AtIdx,
                                                                                s[d - 1].ChildrenCount - 1);

                                             _playerTrees[heroPos].Nodes[n].Position = (byte) st.Nodes[n].Position;
                                             if (st.Nodes[n].Position == heroPos)
                                             {

                                                 _playerTrees[heroPos].Nodes[s[d - 1].NodeIdx].IsHeroActing = true;
                                                 _playerTrees[heroPos].Nodes[n].SetStrVar(this, st.Nodes[n].Probab);
                                             }
                                         }
                                         _playerTrees[heroPos].Nodes[n].AtIdx = Node.INVALID_AT_IDX;
                                         if (d == _playersCount + 1)
                                         {
                                             // Set up the top tree. Do it at the end when the node is initialized
                                             TopTreeNode ttn = new TopTreeNode(this, heroPos, n);
                                             _topTrees[heroPos].Nodes.Add(ttn);
                                         }
                                     };
                wt.OnNodeEnd = (t, s, d) =>
                                   {
                                       if (s[d].ChildrenCount == 0)
                                       {
                                           // A leaf
                                           _playerTrees[heroPos].Nodes[s[d].NodeIdx].AtIdx = s[d].AtIdx;
                                       }
                                   };

                wt.Walk(st);
            }
            st.Dispose();
        }

        private uint FindActionTreeNodeIdx(StrategyTree tree, long sNodeIdx, long aNodeIdx, int hintChildIdx)
        {
            long actionTreeIdx = _init.At.FindChildByAmount(aNodeIdx, tree.Nodes[sNodeIdx].Amount, 
                _init.AtChildrenIndex, hintChildIdx);
            if (actionTreeIdx == -1)
            {
                throw new ApplicationException(String.Format("Cannot find action tree node, strategy node {0}", sNodeIdx));
            }
            return (uint)actionTreeIdx;
        }


        private void CreateSkipChildIndexes(int pos)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Creating skip child indexes for pos {0}", pos);
            }

            PlayerTree pt = _playerTrees[pos];
            Int64[] indexAtDepth = new long[_init.PlayerTreeMaxDepth[pos] + 1].Fill(pt.NodesCount);
            for (Int64 n = pt.NodesCount - 1; n >= 0; n--)
            {
                int depth = pt.GetDepth(n);
                if (pt.Nodes[n].IsHeroActing)
                {
                    Int64 bestIdx = Int64.MaxValue;
                    for (int d = 0; d <= depth; ++d)
                    {
                        bestIdx = Math.Min(bestIdx, indexAtDepth[d]);
                    }
                    pt.Nodes[n].NextNodeToSkipChildren = (UInt32)bestIdx;
                }
                indexAtDepth[depth] = n;
            }
        }

        class CreateChanceInfoContext : WalkUFTreePPContext
        {
            public List<int> HeroCards = new List<int>();
            public int Round = -1;
        }



        private void CreateChanceInfo(int heroPos)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Creating chance info for pos {0}", heroPos);
            }

            WalkUFTreePP<PlayerTree, CreateChanceInfoContext> wt = new WalkUFTreePP<PlayerTree, CreateChanceInfoContext>();

            if (EqualCa && heroPos > 0)
            {
                // Copy chance data from pos 0 to hero position.
                _chanceFactors[heroPos] = _chanceFactors[0];
                _chanceInfos[heroPos] = _chanceInfos[0];
#if USE_CHANCE_MASKS
                _chanceMasks[heroPos] = _chanceMasks[0];
#endif
                wt.OnNodeBegin = (t, s, d) =>
                {
                    if (d == 0) return;
                    Int64 n = s[d].NodeIdx;
                    if (t.Nodes[n].Position == _playersCount)
                    {
                        _playerTrees[heroPos].Nodes[n].ChanceId = t.Nodes[n].ChanceId;
                    }
                };
                wt.Walk(_playerTrees[0]);
                return;
            }

            _chanceInfos[heroPos] = new ChanceInfoEntryT[_init.PlayerCts[heroPos].NodesCount - 1];

            _init.SortChanceTreeIndex(heroPos);

#if USE_CHANCE_MASKS
            _chanceMasks[heroPos] = new FastBitArray(_init.ChanceMasksCount[heroPos]);
#endif
            _chanceFactors[heroPos] = new ChanceFactorArray[(int)CfKind._Count];
            _chanceFactors[heroPos][(int)CfKind.NoSd].Allocate(_init.ChanceFactorsCount[heroPos]);
            _chanceFactors[heroPos][(int)CfKind.Sd].Allocate(_init.ChanceFactorsCount[heroPos]);


            int[] heroPlayerCtKeyIdx = new int[_init.RoundsCount];

            UInt32 chanceMaskIdx = 0;
            UInt32 chanceFactorIdx = 0;
            UInt32 chanceInfoIdx = 0;
            UInt32 [] ctIndexIdx = new UInt32[_init.RoundsCount];

            Dictionary<string, UInt32> heroKeyToChanceId = new Dictionary<string, uint>(new CardKeyComparer());
            double[] potShares = new double[_playersCount];

            int[] chanceInfoInRoundCount = new int[_init.RoundsCount];

            wt.OnNodeBegin = (t, s, d) =>
             {
                 Int64 n = s[d].NodeIdx;
                 if (d == 0)
                 {
                     return;
                 }
                 s[d].Round = s[d - 1].Round;
                 s[d].HeroCards.Clear();
                 s[d].HeroCards.AddRange(s[d - 1].HeroCards);
                 byte pos = t.Nodes[n].Position;
                 if (pos == _playersCount)
                 {
                     // Dealer is acting.
                     s[d].Round++;
                     int round = s[d].Round;
                     // Do some verification
                     int card = (int)t.Nodes[n].ChanceId;
                     s[d].HeroCards.Add(card);
                     string heroKey = CardsToKey(s[d].HeroCards);

                     UInt32 chanceId;
                     if (heroKeyToChanceId.TryGetValue(heroKey, out chanceId))
                     {
                         // This chance info is already created.
                         t.Nodes[n].ChanceId = chanceId;
                     }
                     else
                     {
                         // Create new chance info entry.
                         _chanceInfos[heroPos][chanceInfoIdx].ChanceFactorIdx = chanceFactorIdx;
                         _chanceInfos[heroPos][chanceInfoIdx].ChanceMaskIdx = chanceMaskIdx;
                         _chanceInfos[heroPos][chanceInfoIdx].IdInActionGroup = chanceInfoInRoundCount[round];
                         chanceInfoInRoundCount[round]++;
                         heroKeyToChanceId.Add(heroKey, chanceInfoIdx);
                         t.Nodes[n].ChanceId = chanceInfoIdx++;

                         //if (InitData.CardToString[card] != heroKey.Substring(heroKey.Length - 2))
                         //{
                         //    throw new ApplicationException("Inconsistent hero key");
                         //}

                         // Iterate through all card combinations of opponents
                         // For each combination generate a key consisting of the hero key and opponent keys.
                         // The order of these keys corresponds to the order in the chance index.
                         // Therefore we can make a single comparison to find out if the chance index
                         // contains this combination.
                         ChanceTreeIndexEntry[] ctIndex = _init.ChanceTreeIndex[round];
                         // Note: only 2 players are supported.
                         foreach (string oppKey in _init.PlayerCtKeys[1 - heroPos][round])
                         {
                             string key = CombineKey(heroKey, oppKey, heroPos);
                             if (ctIndexIdx[round] < ctIndex.Length && ctIndex[ctIndexIdx[round]].Key == key)
                             {
                                 // Opponent can have this combination of cards
#if USE_CHANCE_MASKS
                                 _chanceMasks[heroPos].Set(chanceMaskIdx, true);
                                 chanceMaskIdx++;
#endif
                                 IChanceTreeNode chanceTreeNode = _init.Ct.Nodes[ctIndex[ctIndexIdx[round]].NodeIdx];
                                 double chanceProbab = chanceTreeNode.Probab;
                                 // non-showdown
                                 UpdateCfOrders(chanceProbab);
                                 _chanceFactors[heroPos][(int)CfKind.NoSd].Data[chanceFactorIdx] = (ChanceValueT)chanceProbab; 
                                 // showdown - only for last round, othewise set to 0. This is not a big overhead,
                                 // because the most of the chance factros are in the last round.
                                 double sdcf = 0;
                                 if (round == _init.RoundsCount - 1)
                                 {
                                     chanceTreeNode.GetPotShare(0x3, potShares);
                                     // Take pot share of the opponent, because this chance factor will be used 
                                     // to update the tree of the opponent.
                                     sdcf = (ChanceValueT) (chanceProbab*(2*potShares[1 - heroPos] - 1));
                                 }
                                 UpdateCfOrders(sdcf);
                                 _chanceFactors[heroPos][(int)CfKind.Sd].Data[chanceFactorIdx] = (ChanceValueT)sdcf;
                                 chanceFactorIdx++;
                                 // Go to the next combination
                                 ctIndexIdx[round]++;
                             }
                             else
                             {
#if USE_CHANCE_MASKS
                                 // Opponent can not have this combination of cards
                                 _chanceMasks[heroPos].Set(chanceMaskIdx, false);
                                 chanceMaskIdx++;
#else
                                 _chanceFactors[heroPos][(int)CfKind.NoSd].Data[chanceFactorIdx] = 0;
                                 _chanceFactors[heroPos][(int)CfKind.Sd].Data[chanceFactorIdx] = 0;
                                 chanceFactorIdx++;
#endif
                             }
                         }
                         chanceFactorIdx = (chanceFactorIdx + 3) & (~0x3U); // 4-blocks alignment for SSE commands in cpp lib
                         //heroPlayerCtKeyIdx[round]++;
                     }
                 }
             };
            wt.Walk(_playerTrees[heroPos]);
#if USE_CHANCE_MASKS
            Debug.Assert(chanceMaskIdx == _chanceMasks[heroPos].Length);
#endif
            Debug.Assert(chanceFactorIdx == _chanceFactors[heroPos][(int)CfKind.NoSd].Length);
            Debug.Assert(chanceFactorIdx == _chanceFactors[heroPos][(int)CfKind.Sd].Length);
            Debug.Assert(chanceInfoIdx == _chanceInfos[heroPos].Length);
        }

        private void UpdateCfOrders(double chanceFactor)
        {
            double a = Math.Abs(chanceFactor);
            if(a == 0)
            {
                return;
            }
            double log = Math.Log10(a);
            _minCfOrder = Math.Min(_minCfOrder, log);
            _maxCfOrder = Math.Max(_maxCfOrder, log);
        }

        /// <summary>
        /// Use a special comparer for keys. It uses ordinal comparison
        /// and a fast hashing function.
        /// </summary>
        class CardKeyComparer : IEqualityComparer<string>
        {
            #region IEqualityComparer<string> Members

            public bool Equals(string x, string y)
            {
                return StringComparer.Ordinal.Equals(x, y);
            }

            public int GetHashCode(string s)
            {
                return StringHashCode.Get(s);
            }


            #endregion
        }

        class CreateActionGroupsContext : WalkUFTreePPContext
        {
            public StrategicState State;
        }

        void CreateActionGroups(int heroPos)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Creating action groups for pos {0}", heroPos);
            }

            _actionGroups[heroPos] = new ActionGroup[_init.At.NodesCount];

            WalkUFTreePP<ActionTree, CreateActionGroupsContext> wt = new WalkUFTreePP<ActionTree, CreateActionGroupsContext>();

            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (d == 0)
                {
                    s[d].State = new StrategicState(_playersCount);
                }
                else
                {
                    s[d].State = s[d - 1].State.GetNextState(t.Nodes[n]);
                }
            };

            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (s[d].ChildrenCount == 0)
                {
                    // A leaf
                    UInt16 activePlayers = t.Nodes[n].ActivePlayers;
                    UInt16 heroMask = (UInt16)(1 << heroPos);
                    // Note: implemented for 2 players
                    double potFactor;
                    CfKind chanceInfoKind;
                    if (activePlayers == (activePlayers & (~heroMask)))
                    {
                        // The hero folded and loses its inPot
                        potFactor = -s[d].State.InPot[heroPos];
                        chanceInfoKind = 0;
                    }
                    else if (activePlayers == heroMask)
                    {
                        // The opp. folds: hero wins inpot of the opponent
                        potFactor = s[d].State.InPot[1 - heroPos];
                        chanceInfoKind = CfKind.NoSd;
                    }
                    else 
                    {
                        // Showdown (inpots are equal)
                        potFactor = s[d].State.InPot[1 - heroPos];
                        chanceInfoKind = CfKind.Sd;
                    }
                    int round = t.Nodes[n].Round;
                    _actionGroups[heroPos][n].Allocate(_init.PlayerCtNodesCount[heroPos][round]);
                    //_actionGroups[heroPos][n].Leaves = new uint[_init.PlayerCtNodesCount[heroPos][round]];
                    _actionGroups[heroPos][n].PotFactor = potFactor;
                    _actionGroups[heroPos][n].ChanceInfoKind = chanceInfoKind;
                }
            };
            wt.Walk(_init.At);
        }


        private const UInt32 SV_VARLESS = 0xFFFFFFFF;
        private const UInt32 SV_ZERO_MASK = 0x10000000;

        class SetGameValuesContext1 : WalkUFTreePPContext
        {
            public List<Int64> HeroZeroMoves = null;
        }

        class SetGameValuesContext2 : WalkUFTreePPContext
        {
            public UInt32 StrVar = SV_VARLESS;
            public UInt32 ChanceId;
            public double AbsProbabForAlmostZero = 1;
        }


        /// <summary>
        /// Sets game values in opp. tree according to the variables of the hero.
        /// </summary>
        /// <param name="heroPos"></param>
        private void SetGameValuesOpp(int heroPos)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Setting game values of opponents for pos {0}", heroPos);
            }

            var wt1 = new WalkUFTreePP<PlayerTree, SetGameValuesContext1>();
            wt1.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                byte pos = t.Nodes[n].Position;
                s[d].HeroZeroMoves = null;
                if (d > _playersCount && pos == heroPos && t.Nodes[n].StrVar == 0)
                {
                    if(s[d-1].HeroZeroMoves == null)
                    {
                        s[d - 1].HeroZeroMoves = new List<Int64>();
                    }
                    s[d - 1].HeroZeroMoves.Add(n);
                }
            };

            wt1.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (s[d].HeroZeroMoves != null)
                {
                    foreach(uint cn in s[d].HeroZeroMoves )
                    {
                        Debug.Assert(t.Nodes[cn].StrVar == 0);
                        t.Nodes[cn].StrVar = SV_ZERO_MASK | (UInt32)s[d].ChildrenCount;
                    }
                }
            };
            wt1.Walk(_playerTrees[heroPos]);

            var wt2 = new WalkUFTreePP<PlayerTree, SetGameValuesContext2>();
            wt2.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (d > 0)
                {
                    s[d].StrVar = s[d - 1].StrVar;
                    s[d].ChanceId = s[d - 1].ChanceId;
                    s[d].AbsProbabForAlmostZero = s[d - 1].AbsProbabForAlmostZero;
                }
                byte pos = t.Nodes[n].Position;
                if (pos == heroPos)
                {
                    uint strVar = t.Nodes[n].StrVar;
                    if (strVar != SV_VARLESS && ((strVar & SV_ZERO_MASK) == SV_ZERO_MASK))
                    {
                        uint varCnt = strVar & (~SV_ZERO_MASK);
                        s[d].AbsProbabForAlmostZero /= varCnt;
                        t.Nodes[n].StrVar = strVar = 0;
                    }
                    s[d].StrVar = strVar;
                }
                else if (pos == _playersCount)
                {
                    // Dealer
                    s[d].ChanceId = t.Nodes[n].ChanceId;
                }
            };

            wt2.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (s[d].ChildrenCount == 0)
                {
                    // A leaf
                    UInt32 atIdx = t.Nodes[n].AtIdx;
                    UInt32 strVar = s[d].StrVar;
                    if (strVar == SV_VARLESS)
                    {
                        // This is a varless node (the opponent folds before any move of the hero)
                        // The variable of the opponent is updated in every iteration
                        // of the hero, so the "value" is the iteration count of the hero.
                        strVar = (UInt32)IterationCounts[heroPos];
                    }
                    SetGameValuesOppInLeaf(heroPos, strVar, s[d].AbsProbabForAlmostZero, s[d].ChanceId, atIdx);
                }
            };
            wt2.Walk(_playerTrees[heroPos]);

            if (IsVerbose)
            {
                int zeroGvCount = 0;
                int oppPos = 1 - heroPos;
                ActionGroup[] oppAg = _actionGroups[oppPos];
                PlayerTree oppTree = _playerTrees[oppPos];
                for (int a = 0; a < oppAg.Length; a++)
                {
                    if (oppAg[a].GameValues == null)
                    {
                        continue;
                    }
                    double* gameValues = oppAg[a].GameValues;
                    for (UInt32 i = 0; i < oppAg[a].GameValuesLength; ++i)
                    {
                        if (gameValues[i] == 0)
                        {
                            zeroGvCount++;
                        }
                    }
                }
                Console.WriteLine("Zero game value count in leaves for pos {0}: {1}", oppPos, zeroGvCount);
            }
        }

        private void SetGameValuesOppInLeaf(int heroPos, UInt32 strVar, double absProbabForAlmostZero, UInt32 chIdx, UInt32 actIdx)
        {
            // Do not use the pot factor, it will be applied later in BR.
            // Note: implemented for 2 players only
            int oppPos = 1 - heroPos;
            ActionGroup[] oppAg = _actionGroups[oppPos];
            UInt32 cfIdx = _chanceInfos[heroPos][chIdx].ChanceFactorIdx;
#if USE_CHANCE_MASKS
            UInt32 cmIdx = _chanceInfos[heroPos][chIdx].ChanceMaskIdx;
            FastBitArray cm = _chanceMasks[heroPos];
#endif
            ChanceValueT * cf = _chanceFactors[heroPos][(int)oppAg[actIdx].ChanceInfoKind].Data;
            PlayerTree oppTree = _playerTrees[oppPos];
            double * gameValues = oppAg[actIdx].GameValues;

            double almostZero = 1e-50*absProbabForAlmostZero;

            for (UInt32 i = 0; i < oppAg[actIdx].GameValuesLength; ++i)
            {
#if USE_CHANCE_MASKS
                if (cm.Get(cmIdx++))
#endif
                {
                    double gameValue = cf[cfIdx++];
                    if (strVar != 0)
                    {
                        // Normal case, where the hero was in this node.
                        gameValue *= strVar;
                    }
                    else
                    {
                        // Special case, the hero never reached this node so far.
                        // If we leave the zero value, the game value for each node of the opponent including folds
                        // will be zero, which is obviously incorrect.
                        // To avoid this, let's use an almost-zero value. This will be enough to find best response 
                        // between folds, calls and raises. This is an equivalent of playing BR against a uniform
                        // card distribution of opponent. This can be also achived by initial strategy chosing each move 
                        // with some non-zero probability equal for all cards, but this is more difficult.
                        // On the other hand, this will not influence the real game values. 
                        // There are at most Ncf (number of chance factors) values that go in one node, and this is about 10K.
                        gameValue *= almostZero;
                    }
                    gameValues[i] += gameValue;
                }
            }
        }


        private string CombineKey(string heroKey, string oppKey, int heroPos)
        {
            // Note: only 2 players are supported.
            if (heroPos == 0)
            {
                return heroKey + oppKey;
            }
            return oppKey + heroKey;
        }


        /// <summary>
        /// Create initial snapshot. 
        /// To simplify the algo, we have a precondition: it always solves from a snapshot.
        /// For the case there is no snapshot this function creates a new one.
        /// </summary>
        private void CreateNewSnapshot()
        {
            if (IsVerbose)
            {
                Console.WriteLine("Creating initial snapshot {0}", _curSnapshotInfo.BaseDir);
            }
            // Creates strategies for each player and saves them.
            for (int p = 0; p < _playersCount; ++p)
            {
                StrategyTree st = CreateStrategyTreeByChanceAndActionTrees.CreateS(_init.PlayerCts[p], _init.At);
                if (p == 0)
                {
                    SetInitialStrategy(st, 0);
                }
                st.Write(_curSnapshotInfo.StrategyFile[p]);
                if (EqualCa)
                {
                    // We have only one file for all strategies in this case
                    break; 
                }
            }

            // Do it at the end, where all snapshot-related fields are initialized.
            SaveSnapshotAuxData();
        }

        class SetInitialStrategyContext : WalkUFTreePPContext
        {
            public double Probab = 1;
        }


        /// <summary>
        /// Set initial strategy. Only pure strategies can be used, because 
        /// we use integers to keep strategy variables, and for the initial iteration count of 1
        /// it is impossible to convert any number probability except 0 or 1 to an integer.
        /// </summary>
        /// <param name="st"></param>
        /// <param name="pos"></param>
        private void SetInitialStrategy(StrategyTree st, int pos)
        {
            IterationCounts[pos]++;
            WalkUFTreePP<StrategyTree, SetInitialStrategyContext> wt = new WalkUFTreePP<StrategyTree, SetInitialStrategyContext>();
            wt.OnNodeBegin = (t, s, d) =>
             {
                 Int64 n = s[d].NodeIdx;
                 if (d > _playersCount)
                 {
                     s[d].Probab = s[d - 1].Probab;
                     if (t.Nodes[n].IsPlayerAction(pos))
                     {
                         // Set child #2 (call) to 1
                         if (s[d - 1].ChildrenCount == 2 && s[d].Probab > 0)
                         {
                             s[d].Probab = t.Nodes[n].Probab = 1.0;
                         }
                         else
                         {
                             s[d].Probab = 0;
                         }
                     }
                 }
             };
            wt.Walk(st);
            string error;
            if (!VerifyAbsStrategy.Verify(st, pos, out error))
            {
                throw new ApplicationException(String.Format("Initial strategy inconsistent: {0}", error));
            }
        }


        #endregion

        #region Implementation - iterations

        /// <summary>
        /// Now do fictious play repeatedly.
        /// </summary>
        private void DoIterations()
        {
            DateTime start = DateTime.Now;

            // Start from the player with minimal iteration count, if equal, start from 0.
            int minIterCount = int.MaxValue;
            for (int p = 0; p < _playersCount; ++p)
            {
                if (minIterCount > IterationCounts[p])
                {
                    _heroPos = p;
                    minIterCount = IterationCounts[p];
                }
            }

            for (CurrentIterationCount = 0; ; _heroPos = (_heroPos + 1) % _playersCount)
            {
                if (MaxIterationCount >= 0 && CurrentIterationCount >= MaxIterationCount)
                {
                    break;
                }
                IterationCounts[_heroPos]++;
                CurrentIterationCount++;

                BestResponse();

                if (CurrentIterationCount >= _playersCount)
                {
                    // Calculate espsilon as soon as we have done SBR for each player at least once.
                    CurrentEpsilon = 0;
                    for (int p = 0; p < _playersCount; ++p)
                    {
                        CurrentEpsilon += LastSbrValues[p];
                    }
                    CurrentEpsilon = Math.Abs(CurrentEpsilon);
                    UpdateEpsilonLog();
                }
                else
                {
                    CurrentEpsilon = Double.MaxValue;
                }

                _iterationTime = (DateTime.Now - start).TotalSeconds;

                if (IsIterationVerbose())
                {
                    PrintIterationStatus(Console.Out);
                }

                if (CheckExitCriteria())
                {
                    break;
                }
            }

            if (IsVerbose)
            {
                PrintIterationStatus(Console.Out);
            }
        }

        /// <summary>
        /// Check exit criteria.
        /// </summary>
        /// <returns>true if an exit criterion satisfies.</returns>
        bool CheckExitCriteria()
        {
            if (OnIterationDone != null)
            {
                if (!OnIterationDone(this))
                {
                    return true;
                }
            }

            // Exit criteria: make sure we have done at least one iteration for each position
            // and sum of values is close enough to 0.
            if (CurrentIterationCount >= _playersCount)
            {
                if (CurrentEpsilon <= Epsilon)
                {
                    return true;
                }
            }
            return false;
        }

        private void BestResponse()
        {
            DateTime time1 = DateTime.Now;
            BestResponseValuesUp();
            DateTime time2 = DateTime.Now;
            _timeInBrValuesUp += (time2 - time1).TotalSeconds;
            BestResponseFinalize();
            _timeInBrFinalize += (DateTime.Now - time2).TotalSeconds;

            if (TraceDir != null)
            {
                Vis.Show(this, _heroPos, GetTraceFileName(_heroPos, "tree", "", "gv"));
            }
        }        

        void BestResponseValuesUp()
        {
            TopTree topTree = _topTrees[_heroPos];
            if (_threadPool == null)
            {
                for(int i = 0; i < topTree.Nodes.Count; ++i)
                {
                    topTree.Nodes[i].Job.Do();
                }
            }
            else
            {
                for (int i = 0; i < topTree.Nodes.Count; ++i)
                {
                    _threadPool.QueueJob(topTree.Nodes[i].Job, i % _threadPool.ThreadsCount);
                }
                _threadPool.WaitAllJobs();
            }
            LastSbrValues[_heroPos] = _topTrees[_heroPos].SumAndClearValues();
        }

        /// <summary>
        /// A helper method converting internal game value format to the real
        /// game value. 
        /// </summary>
        public double ConvertGameValue(double internalGameValue, int heroPos)
        {
            double v = 0;
            Debug.Assert(_playersCount == 2);
            int oppPos = 1 - heroPos;
            if (IterationCounts[oppPos] != 0)
            {
                v = internalGameValue / IterationCounts[oppPos];
            }
            return v;
        }

        struct BestResponseFinalizeContext
        {
            public UInt32 ChanceId;
        }

        BestResponseFinalizeContext [] _brfStack = new BestResponseFinalizeContext[256];

        void BestResponseFinalize()
        {
            WalkTreeWithSkipChildren(_playerTrees[_heroPos], (uint)_playersCount);
            if (_threadPool != null)
            {
                _threadPool.WaitAllJobs();
            }
        }

        void WalkTreeWithSkipChildren(PlayerTree tree, UInt32 startNode)
        {
            Int32 startDepth = tree.GetDepth(startNode);
            int curDepth;
            for (UInt32 i = startNode; i < tree.NodesCount; ++i)
            {
                curDepth = tree.GetDepth(i);
                // Check depth first because it is almost always false, 
                // and the second check will be not necessary.
                if (curDepth <= startDepth && i > startNode)
                {
                    break;
                }
                BestResponseFinalize_OnNode(tree, ref i, curDepth);
            }
        }

        /// <summary>
        /// Called on each node.
        /// </summary>
        /// <param name="n">Current node. To skip children, set it to nextNode - 1 (because it will be incremented).</param>
        void BestResponseFinalize_OnNode(PlayerTree tree, ref UInt32 n, int d)
        {
            Node* pNode = tree.Nodes + n;
            int pos = pNode->Position;
            if (pos == _heroPos)
            {
                // This var belongs to BR (otherwise we would have skipped it).
                pNode->StrVar++;
            }
            if(pNode->Position == _playersCount)
            {
                // Dealer node
                _brfStack[d].ChanceId = pNode->ChanceId;
            }
            else if(d > _playersCount)
            {
                _brfStack[d].ChanceId = _brfStack[d - 1].ChanceId;
            }

            if (pNode->IsHeroActing)
            {
                WalkTreeWithSkipChildren(tree, pNode->BestBrNode);
                n = pNode->NextNodeToSkipChildren - 1;
            }
            else if(pNode->IsLeaf)
            {
                // This leaf belongs to BR (otherwise we would have skipped it).
                _finalBrLeavesCount++;
                QueueIncrementGameValueJob(_brfStack[d].ChanceId, pNode->AtIdx);
            }
        }

        private void QueueIncrementGameValueJob(UInt32 chIdx, UInt32 actIdx)
        {
            if (_threadPool != null)
            {
                // Create and queue a job.
                var  job = new AssigningThreadPool.Job<uint, uint>
                {
                    Param1 = chIdx, Param2 = actIdx,
                    Execute = IncrementGameValue
                };
                int threadId = (int) (actIdx % _threadPool.ThreadsCount);
                _threadPool.QueueJob(job, threadId);
                //IncrementGameValueJob(job.Parameters);
            }
            else
            {
                // Call directly for single-threaded.
                IncrementGameValue(chIdx, actIdx);
            }
        }


        private void IncrementGameValue(UInt32 chIdx, UInt32 actIdx)
        {
            Debug.WriteLineIf(IsDebugPrintOn, String.Format("IncGV: hero: {0}, ch: {1}, act: {2}", _heroPos, chIdx, actIdx));
            // Note: implemented for 2 players only
            int oppPos = 1 - _heroPos;
            ActionGroup[] oppAg = _actionGroups[oppPos];
#if USE_CHANCE_MASKS
            UInt32 cmIdx = _chanceInfos[_heroPos][chIdx].ChanceMaskIdx;
            FastBitArray cm = _chanceMasks[_heroPos];
#endif
            PlayerTree oppTree = _playerTrees[oppPos];
            int gvCount = oppAg[actIdx].GameValuesLength;

            UInt32 cfIdx = _chanceInfos[_heroPos][chIdx].ChanceFactorIdx;
            ChanceValueT* pCf = _chanceFactors[_heroPos][(int)oppAg[actIdx].ChanceInfoKind].Data + cfIdx;
            double* pGameValues = oppAg[actIdx].GameValues;
#if USE_CPP_LIB
#if USE_CHANCE_MASKS
            fixed(UInt32 * pChanceMaskBits = &(cm.Bits[0]))
            {
                CppLib.IncrementGameValue(pGameValues, (uint)gvCount, pCf, pChanceMaskBits, cmIdx);
            }
#else
            CppLib.IncrementGameValueNoMasks(pGameValues, (uint)gvCount, pCf);
#endif
#else
            for (UInt32 l = 0; l < gvCount; ++l)
            {
                Debug.WriteIf(IsDebugPrintOn, String.Format("Leaf# {0}: ", l));
#if USE_CHANCE_MASKS
                if (cm.Get(cmIdx++))
#endif
                {
                    Debug.WriteLineIf(IsDebugPrintOn, String.Format("delta {0}", *pCf));
                    pGameValues[l] += *pCf++;
                }
#if USE_CHANCE_MASKS

#if DEBUG
                else
                {
                    Debug.WriteLineIf(IsDebugPrintOn, "skip");
                }
#endif
#endif
            }
#endif

        }

        private void UpdateEpsilonLog()
        {
            // Add to empty log only after some iterations to get a more or less converged epsilon 
            if (_epsilonLog.Count > 0 || TotalIterationCount > 100)
            {

                bool add = true;
                if (_epsilonLog.Count > 0)
                {
                    EpsilonLogEntry last = _epsilonLog[_epsilonLog.Count - 1];
                    add = CurrentEpsilon <= last.Epsilon * EpsilonLogThreshold;
                }
                if (add)
                {
                    _epsilonLog.Add(new EpsilonLogEntry
                    {
                        Epsilon = CurrentEpsilon,
                        TotalIterationsCount = TotalIterationCount
                    });
                    if (IsVerbose)
                    {
                        Console.WriteLine("Epsilon log updated:");
                        WriteEpsilonLog(Console.Out);
                    }
                }
            }
        }

        private void PrintIterationStatus(TextWriter output)
        {
            string epsString = CurrentEpsilon == Double.MaxValue
                                   ? "?"
                                   : string.Format("{0:0.00000}", CurrentEpsilon);
            output.Write("Iteration cur: {0:#,#}, {1:0.000} it/s, total: {2:#,#}; eps: {3:0.000000}",
                          CurrentIterationCount,
                          CurrentIterationCount / _iterationTime,
                          TotalIterationCount,
                          epsString);
            output.Write("; last SBR: ");
            for (int p = 0; p < LastSbrValues.Length; ++p)
            {
                output.Write("{0}:{1:0.00000} ", p, LastSbrValues[p]);
            }
            output.Write("; time in BR: v-up: {0:0.0} s, fin: {1:0.0} s", _timeInBrValuesUp, _timeInBrFinalize);
            output.Write("; fin BR leaves: {0:#,#}K", _finalBrLeavesCount * 1e-3);
            output.WriteLine();
        }

        #endregion

        #region Implementation - other

        void CleanUp()
        {
            if (_threadPool != null)
            {
                _threadPool.Dispose();
                _threadPool = null;
            }
        }

        private void FreeActionGroupsAndChanceFactors()
        {
            for (int p = 0; p < _playersCount; ++p)
            {
                for (int g = 0; g < _actionGroups[p].Length; ++g)
                {
                    _actionGroups[p][g].Free();
                }
                for (int c = 0; c < _chanceFactors[p].Length; ++c)
                {
                    _chanceFactors[p][c].Free();
                }
            }
        }

        private void WriteEpsilonLog(TextWriter tw)
        {
            foreach (EpsilonLogEntry e in _epsilonLog)
            {
                tw.WriteLine("{0} {1}", e.Epsilon, e.TotalIterationsCount);
            }
        }

        /// <summary>
        /// Saves game values. It is necessary to avoid numeric instability caused by
        /// differences in setting game values. During iterations they are updated
        /// incrementally. If during initialization they are set by calculating them from variables, 
        /// they deviate slightly, which causes different BRs in nodes with almost equal values and sooner
        /// or later leads to a different strategy. The more snapshots are used the more the difference.
        /// This strategy is also an equilibrium one. But to make the life easier it is better to get every time
        /// the same result. Moreover, during initialization calculating game value take the most of the time. 
        /// Reading them from a file is much faster.
        /// </summary>
        /// <param name="heroPos"></param>
        private void SaveGameValues(int heroPos)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Saving game values for pos {0}", heroPos);
            }
            using (BinaryWriter bw = new BinaryWriter(File.Open(_curSnapshotInfo.GameValuesFile[heroPos],
                FileMode.Create, FileAccess.Write)))
            {
                for(int i = 0; i < _actionGroups[heroPos].Length; ++i)
                {
                    double * gameValues = _actionGroups[heroPos][i].GameValues;
                    int length = _actionGroups[heroPos][i].GameValuesLength;
                    bw.Write(length);
                    if (gameValues != null)
                    {
                        UnmanagedMemory.Write(bw, new IntPtr(gameValues), length*sizeof (double));
                    }
                }
            }
        }

        private void LoadGameValues(int heroPos)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Loading game values for pos {0}", heroPos);
            }

            using (BinaryReader br = new BinaryReader(File.Open(_curSnapshotInfo.GameValuesFile[heroPos],
                FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                for (int i = 0; i < _actionGroups[heroPos].Length; ++i)
                {
                    double* gameValues = _actionGroups[heroPos][i].GameValues;
                    int actLength = _actionGroups[heroPos][i].GameValuesLength;
                    int length = br.ReadInt32();
                    if (length != actLength)
                    {
                        throw new ApplicationException("Wrong action group length");
                    }
                    if (gameValues != null)
                    {
                        UnmanagedMemory.Read(br, new IntPtr(gameValues), length * sizeof(double));
                    }
                }
            }
        }

        void SwitchSnapshot()
        {
            _snapshotSwitcher.GoToNextSnapshot();
            _curSnapshotInfo = new SnapshotInfo(_snapshotSwitcher.CurrentSnapshotPath, _playersCount, EqualCa);
        }

        private void SaveStrategies()
        {
            StrategyTree st = null;

            for (int pos = 0; pos < _playersCount; ++pos)
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Save strategy for pos {0}", pos);
                }
                if (!EqualCa || pos == 0)
                {
                    st = StrategyTree.Read<StrategyTree>(_loadedSnapshotInfo.StrategyFile[pos]);
                }
                WalkUFTreePP<StrategyTree, CreatePlayerTreeContext> wt =
                    new WalkUFTreePP<StrategyTree, CreatePlayerTreeContext>();
                wt.OnNodeBegin = (t, s, d) =>
                                     {
                                         Int64 n = s[d].NodeIdx;
                                         if (t.Nodes[n].IsPlayerAction(pos))
                                         {
                                             t.Nodes[n].Probab = _playerTrees[pos].Nodes[n].GetStrVar(this);
                                         }
                                     };
                wt.Walk(st);
                if (!EqualCa)
                {
                    st.Write(_curSnapshotInfo.StrategyFile[pos]);
                }
            }
            if (EqualCa)
            {
                st.Write(_curSnapshotInfo.StrategyFile[0]);
            }
        }

        void SaveSnapshotAuxData()
        {
            using (TextWriter tw = new StreamWriter(_curSnapshotInfo.HeaderFile))
            {
                for (int pos = 0; pos < _playersCount; ++pos)
                {
                    tw.WriteLine("{0}", IterationCounts[pos]);
                }
            }

            using (TextWriter tw = new StreamWriter(_curSnapshotInfo.EpsilonLog))
            {
                WriteEpsilonLog(tw);
            }

            using (TextWriter tw = new StreamWriter(_curSnapshotInfo.InfoFile))
            {
                PrintIterationStatus(tw);
            }
        }

        private void SaveSnapshot()
        {
            if (IsVerbose)
            {
                Console.WriteLine("Saving snapshot to {0}", _curSnapshotInfo.BaseDir);
            }
            SaveSnapshotAuxData();
            for (int p = 0; p < _playersCount; ++p)
            {
                SaveGameValues(p);
            }
        }

        public void LoadSnapshot()
        {
            if (IsVerbose)
            {
                Console.WriteLine("Loading snapshot from {0}", _curSnapshotInfo.BaseDir);
            }

            _loadedSnapshotInfo = _curSnapshotInfo;

            using (TextReader tr = new StreamReader(_curSnapshotInfo.HeaderFile))
            {
                string line;
                for (int pos = 0; pos < _playersCount; ++pos)
                {
                    line = tr.ReadLine();
                    IterationCounts[pos] = int.Parse(line);
                }
                line = tr.ReadLine();
            }

            _epsilonLog.Clear();
            if (File.Exists(_curSnapshotInfo.EpsilonLog))
            {
                using (TextReader tr = new StreamReader(_curSnapshotInfo.EpsilonLog))
                {
                    for (;;)
                    {
                        string line;
                        line = tr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        string[] data = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        _epsilonLog.Add(new EpsilonLogEntry
                                           {
                                               Epsilon = double.Parse(data[0]),
                                               TotalIterationsCount = int.Parse(data[1]),
                                           });
                    }
                }
            }
        }

        #endregion

        #region Trace

#if DEBUG_PRINT
        const bool IsDebugPrintOn = true;
#else
        const bool IsDebugPrintOn = false;
#endif

        UInt64 _finalBrLeavesCount;

        double _timeInBrValuesUp = 0;
        double _timeInBrFinalize = 0;

        private string GetTraceFileName(int pos, string kind, string substep, string ext)
        {
            string fileName = string.Format("{0}\\{1:00000}-{2}-{3}{4}.{5}",
                                            TraceDir, IterationCounts[pos], pos,
                                            kind,
                                            string.IsNullOrEmpty(substep) ? "" : "-" + substep,
                                            ext);
            return fileName;
        }

        bool IsIterationVerbose()
        {
            return IsVerbose &&
                IterationVerbosity > 0 &&
                (CurrentIterationCount % IterationVerbosity == 0);
        }

        private void PrintChanceInfo(int pos, string fileName)
        {
            using (TextWriter tw = new StreamWriter(fileName))
            {
                for (int i = 0; i < _chanceInfos[pos].Length; ++i)
                {
                    int maskIdx = (int)_chanceInfos[pos][i].ChanceMaskIdx;
                    tw.WriteLine("CI: {0,6} mask: {1,5} factor: {2,5} id in ag: {3, 5}", i,
                        maskIdx, _chanceInfos[pos][i].ChanceFactorIdx, _chanceInfos[pos][i].IdInActionGroup);
                    int masksCount = 0;
#if USE_CHANCE_MASKS
                    if (i == _chanceInfos[pos].Length - 1)
                    {
                        masksCount = (int)(_chanceMasks[pos].Length - maskIdx);
                    }
                    else
                    {
                        masksCount = (int)( _chanceInfos[pos][i + 1].ChanceMaskIdx - maskIdx);
                    }
#endif
                    int chanceFactorIdx = (int)_chanceInfos[pos][i].ChanceFactorIdx;
                    for (int m = maskIdx; m < maskIdx + masksCount; ++m)
                    {
                        int mask = 1;
#if USE_CHANCE_MASKS
                        mask = _chanceMasks[pos].Get((uint)m) ? 1 : 0;
#endif
                        tw.Write("Mask {0, 5}: {1}", m, mask);
#if USE_CHANCE_MAKSS
                        if (_chanceMasks[pos].Get((uint)m))
                        {
#endif
                            tw.Write(": cf nsd: {0:0.00000}, cf sd: {1:0.00000}",
                                _chanceFactors[pos][chanceFactorIdx], _chanceFactors[pos][chanceFactorIdx + 1]);
                            chanceFactorIdx += 2;
#if USE_CHANCE_MAKSS
                        }

                        else
                        {
                            tw.Write(": -----");
                        }
#endif
                        tw.WriteLine();
                    }
                }
            }
        }

        private void PrintActionGroups(int p, string fileName)
        {
            using (TextWriter tw = new StreamWriter(fileName))
            {
                int agCount = 0;
                for (int i = 0; i < _actionGroups[p].Length; ++i)
                {
                    if (_actionGroups[p][i].GameValues != null)
                    {
                        tw.WriteLine("AG: {0,5} At node: {1,5}, pf: {2:0.0000}", agCount, i, _actionGroups[p][i].PotFactor);
                        agCount++;
                        for (int g = 0; g < _actionGroups[p][i].GameValuesLength; ++g)
                        {
                            tw.WriteLine(" {0:0.000}", _actionGroups[p][i].GameValues[g]);
                        }
                    }
                }
            }
        }

        #endregion

    }
}
