/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

#if DEBUG

// If defined, prints some info with Debug.Write().
//#define DEBUG_PRINT

#endif

//#define USE_CPP_LIB


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
    /// Fictitious play using Monte Carlo card sampling. 
    /// For this implementation to work, the following constraints must be fulfilled:
    /// 1. Number of players must be 2.
    /// 2. All players must use the same chance abstraction.
    /// 3. It must be a PS game (private-shared, 1st round - only private cards are dealt, in the others - only shared cards).
    /// </summary>
    /// Developer nodes.
    /// Terms used in the documentation:
    /// Hero - the player currently playing BR.
    /// Opponent - any non-hero player.
    public unsafe class FictitiousPlayMc
    {
        #region Public types

        public unsafe class Vis : VisPkrTree<UFToUniAdapter, int, int, Vis.Context>
        {
            public class Context : VisPkrTreeContext<int, int>
            {
                public bool IsLeaf;
            }

            public Vis()
            {
                CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));

                ShowExpr.Clear();
                ShowExpr.Add(new ExprFormatter("s[d].GvId", "id:{1}"));
            }


            public FictitiousPlayMc Solver
            {
                set;
                get;
            }

            public int Position
            {
                set;
                get;
            }

            void Show(StrategyTree t)
            {
                UFToUniAdapter adapter = new UFToUniAdapter(t);
                Walk(adapter, 0);
            }

            public static void Show(FictitiousPlayMc solver, int position, string fileName)
            {
                using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
                {
                    Vis vis = new Vis { Output = w, Solver = solver, Position = position };
                    vis.Show(solver._pt);
                }
            }

            protected override void OnTreeBeginFunc(UFToUniAdapter tree, int root)
            {
                string heroHand = "";
                foreach(int c in Solver._hands[Solver._heroPos])
                {
                    heroHand += string.Format("{0} ", c);
                }
                GraphAttributes.label = string.Format("PT hero {0}, mc hand {1}", Position, heroHand);
                base.OnTreeBeginFunc(tree, root);
            }

            protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int n, List<Context> stack, int depth)
            {
                Context context = stack[depth];
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                context.IsLeaf = true;

                if (depth == 0)
                {
                    context.Round = -1;
                }
                else
                {
                    stack[depth - 1].IsLeaf = false;
                    context.Round = stack[depth - 1].Round;
                }
                // Deal action => new round.
                if (tree.Nodes[n].IsDealerAction)
                {
                    context.Round++;
                }
                int pos = tree.Nodes[n].Position;
                context.IsDealerAction = tree.Nodes[n].IsDealerAction;
                context.Position = pos;
                context.ActionLabel = context.Position.ToString();
                if (tree.Nodes[n].IsDealerAction)
                {
                    context.ActionLabel += "d" + tree.Nodes[n].Card;
                }
                else
                {
                    context.ActionLabel += "p" + tree.Nodes[n].Amount.ToString();
                }
                return base.OnNodeBeginFunc(aTree, n, stack, depth);
            }

            protected override void CustomizeNodeAttributes(UFToUniAdapter aTree, int n, List<Context> stack, int depth, NodeAttributeMap attr)
            {
                Context context = stack[depth];
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                base.CustomizeNodeAttributes(aTree, n, stack, depth, attr);
                string nodeLabel = attr.label;
                nodeLabel += string.Format("\\ngv:{0:0.000}", Solver._ptExt[Position][n].GameValue);
                if (!tree.Nodes[n].IsDealerAction && n > 0)
                {
                    nodeLabel += string.Format("\\ns:{0:0.0000}", tree.Nodes[n].Probab);
                }
                if (tree.Nodes[n].IsDealerAction)
                {
                    nodeLabel += string.Format("\\nc:{0}", tree.Nodes[n].Card);
                }
                if (context.IsLeaf)
                {
                    nodeLabel += string.Format("\\npf:{0:0.0000}", Solver._ptExt[Position][n].PotFactor);
                    nodeLabel += string.Format("\\nati:{0}", Solver._ptExt[Position][n].AtIdx);
                }
                if (n == 0)
                {
                    nodeLabel += string.Format("\\nGV:{0:0.000}", Solver.LastBrValues[Position]);
                }
                attr.label = nodeLabel;
            }

            protected override void CustomizeEdgeAttributes(UFToUniAdapter aTree, int n, int pn, List<Context> stack, int depth, EdgeAttributeMap attr)
            {
                base.CustomizeEdgeAttributes(aTree, n, pn, stack, depth, attr);
                /*PlayerTree tree = (PlayerTree)aTree.UfTree;

                if (tree.Nodes[pn].IsHeroActing && n == tree.Nodes[pn].BestBrNode)
                {
                    // Hero have chosen this edge - show thick
                    attr.penwidth = 3.0;
                }*/
            }
        }

        /// <summary>
        /// Information about persistent data of the algorithms. Contains no actual data, but just file names.
        /// </summary>
        public class SnapshotInfo
        {
            public SnapshotInfo(string baseDir, int playersCount)
            {
                BaseDir = baseDir;
                HeaderFile = Path.Combine(baseDir, GetSnapshotHeaderFileName());
                StrategyFile = Path.Combine(baseDir, "strategy.dat");
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

            public String StrategyFile
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

        public delegate bool OnIterationDoneDelegate(FictitiousPlayMc solver);

        #endregion

        #region Public properties

        public GameDefinition GameDef
        {
            set;
            get;
        }

        public int RngSeed
        {
            set;
            get;
        }

        public string ActionTreeFile
        {
            set;
            get;
        }

        /// <summary>
        /// Card count for each round. Is needed to build player trees.
        /// </summary>
        public int[] CardCount
        {
            set;
            get;
        }
         
        /// <summary>
        /// Chance abstraction (for all players). The cards in each round must be ordered from 0 to MaxCard.
        /// </summary>
        public IChanceAbstraction ChanceAbstraction
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
        /// Number of threads in a thread pool for parallel execution. 
        /// The main thread is not included in this count.
        /// Default: 0, in this case no thread pool is created.
        /// Todo: is not used not, left here for backwards compatibility with UTs.
        /// </summary>
        public int ThreadsCount
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
        public double[] LastBrValues
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

        public FictitiousPlayMc()
        {
            MaxIterationCount = -1;
            Epsilon = 0.001;
            // Set to a small value to reduce noise in tests
            EpsilonLogThreshold = 0.1;
            SnapshotsCount = 2;
            OutputPath = "./FictPlayMc";
            //JobsPerThread = 1;
        }

        public void Solve()
        {
            Initialize();
            DoIterations();
            //SwitchSnapshot();
            //SaveSnapshot();
            _pt.Write(_curSnapshotInfo.StrategyFile);
            CleanUp();
        }

        #endregion

        #region Implementation - main data types

        class EpsilonLogEntry
        {
            public double Epsilon;
            public double TotalIterationsCount;
        }

        /// <summary>
        /// Node of the players tree. Is made public to do some unit-testing.
        /// </summary>
        //[StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct Node
        {
            public double GameValue;
            public double PotFactor;
            public uint AtIdx;
        }

        // Todo: remove if not necessary
        enum CfKind
        {
            NoSd = 0,
            Sd = 1,
            _Count = 2
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
        StrategyTree _pt;
        Node[][] _ptExt;

        private ActionTree _at;


        /// <summary>
        /// Position of the player currently doing BR.
        /// </summary>
        int _heroPos;

        private double _iterationTime;

        Random _rng;
        private McDealer _mcDealer;

        /// <summary>
        /// Current mc hands of the players.
        /// </summary>
        private int[][] _hands;

        /// <summary>
        /// Hand size for each round.
        /// </summary>
        private int[] _handSizes;

        /// <summary>
        /// An array of the size equal to the number of nodes of the action tree. 
        /// Is updated during each iteration. Updated cells contain accumulated game values for this iteration.
        /// </summary>
        private double[] _oppGv;

        #endregion

        #region Implementation - initialization

        static FictitiousPlayMc()
        {
            //CppLib.Init();
        }

        /// <summary>
        /// Contains data required for initialization only
        /// </summary>
        class InitData
        {
            public InitData(FictitiousPlayMc solver)
            {
                int playersCount = solver._at.PlayersCount;
                AtChildrenIndex = new UFTreeChildrenIndex(solver._at);
                CreatePlayerCt(solver.CardCount);
                RoundsCount = solver.CardCount.Length;

            }

            void FillPlayerCt(int d, int [] cardCount, ref int n)
            {
                if(d >= cardCount.Length)
                {
                    return;
                }
                for(int c = 0; c < cardCount[d]; ++c)
                {
                    FillPlayerCt(d + 1, cardCount, ref n);
                    n++;
                    PlayerCt.Nodes[n].Card = c;
                    PlayerCt.SetDepth(n, (byte)(d+1));
                }
            }

            void CreatePlayerCt(int[] cardCount)
            {
                int nodeCount = 1;
                int power = 1;
                for (int r = 0; r < cardCount.Length; ++r)
                {
                    power *= cardCount[r];
                    nodeCount += power;
                }
                PlayerCt = new ChanceTree(nodeCount);
                PlayerCt.SetDepthsMemory(0);
                PlayerCt.SetNodesMemory(0);
                int n = 0;
                FillPlayerCt(0, cardCount, ref n);
            }

            public int RoundsCount;

            public ChanceTree PlayerCt;
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

             _at = ActionTree.Read<ActionTree>(ActionTreeFile);
             if (IsVerbose)
             {
                 Console.WriteLine("Action tree: {0}", _at.Version.ToString());
             }
            _init = new InitData(this);
            _playersCount = _at.PlayersCount;
            _epsilonLog = new List<EpsilonLogEntry>();
            _snapshotSwitcher = new SnapshotSwitcher(OutputPath, GetSnapshotHeaderFileName(), SnapshotsCount);
            _curSnapshotInfo = new SnapshotInfo(_snapshotSwitcher.CurrentSnapshotPath, _playersCount);
            IterationCounts = new int[_playersCount];
            LastBrValues = new double[_playersCount];
            _ptExt = new Node[_playersCount][];

            _rng = new System.Random(RngSeed);
            _mcDealer = new McDealer(GameDef, _rng);
            _hands = new int[_playersCount][].Fill(i => new int[_mcDealer.HandSize]);
            _handSizes = GameDef.GetHandSizes();
            _oppGv = new double[_at.NodesCount];

            bool isNewSnapshot = !_snapshotSwitcher.IsSnapshotAvailable;
            if (isNewSnapshot)
            {
                CreateNewSnapshot();
            }
            //LoadSnapshot();

            CreatePlayerTrees();

            for (int p = 0; p < _playersCount; ++p)
            {
                if (TraceDir != null)
                {
                    Vis.Show(this, p, GetTraceFileName(p, "tree", "init-pt", "gv"));
                }
            }
            if (TraceDir != null)
            {
                VisChanceTree.Show(_init.PlayerCt, GetTraceFileName(0, "pct", "", "gv"));
            }

            PrintInitDone();

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
                }
            }
            if (IsVerbose)
            {

                for(int p= 0; p < _playersCount; ++p)
                {
                    Console.WriteLine("Data structures pos {0}:", p);
                    Console.WriteLine("Player tree nodes: {0:#,#}", _pt.NodesCount);
                }
            }
        }

        class CreatePlayerTreeContext : WalkUFTreePPContext
        {
            public UInt32 AtIdx;
            /// <summary>
            /// Inpot of each player. Now support heads up only.
            /// </summary>
            public double[] InPot = new double[2]; 
        }

        private void CreatePlayerTrees()
        {
            
            //_pt = StrategyTree.Read<StrategyTree>(_curSnapshotInfo.StrategyFile[heroPos]);
            for (int heroPos = 0; heroPos < _playersCount; ++heroPos)
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Creating player tree for pos {0}", heroPos);
                }

                _ptExt[heroPos] = new Node[_pt.NodesCount];
                var wt = new WalkUFTreePP<StrategyTree, CreatePlayerTreeContext>();
                wt.OnNodeBegin = (t, s, d) =>
                                     {
                                         Int64 n = s[d].NodeIdx;
                                         if(d > 0)
                                         {
                                            s[d].AtIdx = s[d - 1].AtIdx;
                                            s[d - 1].InPot.CopyTo(s[d].InPot, 0);
                                         }
                                         if(!_pt.Nodes[n].IsDealerAction && n > 0)
                                         {
                                             s[d].InPot[_pt.Nodes[n].Position] += _pt.Nodes[n].Amount;

                                             s[d].AtIdx = FindActionTreeNodeIdx(t, n, s[d].AtIdx,
                                                                                s[d - 1].ChildrenCount - 1);
                                         }
                                         _ptExt[heroPos][n].AtIdx = s[d].AtIdx;
                                         /*if (d == 0)
                                         {
                                         }
                                         else
                                         {
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
                                         }*/
                                     };
                wt.OnNodeEnd = (t, s, d) =>
                                   {
                                       Int64 n = s[d].NodeIdx;
                                       if(s[d].ChildrenCount == 0)
                                       {
                                           // A leaf
                                           Debug.Assert(_playersCount == 2);
                                           _ptExt[heroPos][n].PotFactor = Math.Min(s[d].InPot[0], s[d].InPot[1]);
                                       }
                                   };

                wt.Walk(_pt);
            }
        }

        private uint FindActionTreeNodeIdx(StrategyTree tree, long sNodeIdx, long aNodeIdx, int hintChildIdx)
        {
            long actionTreeIdx = _at.FindChildByAmount(aNodeIdx, tree.Nodes[sNodeIdx].Amount,
                _init.AtChildrenIndex, hintChildIdx);
            if (actionTreeIdx == -1)
            {
                throw new ApplicationException(String.Format("Cannot find action tree node, strategy node {0}", sNodeIdx));
            }
            return (uint)actionTreeIdx;
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
            _pt = CreateStrategyTreeByChanceAndActionTrees.CreateS(_init.PlayerCt, _at);
            for (int p = 0; p < _playersCount; ++p)
            {
                SetInitialStrategy(p);
            }
            // Todo: no saving now
            //_pt.Write(_curSnapshotInfo.StrategyFile);
            // Do it at the end, where all snapshot-related fields are initialized.
            //SaveSnapshotAuxData();
        }

        class SetInitialStrategyContext : WalkUFTreePPContext
        {
            public List<Int64> HeroChildren = new List<long>();
            public double Probab = 1.0;
        }


        /// <summary>
        /// Set initial strategy.
        /// </summary>
        private void SetInitialStrategy(int pos)
        {
            IterationCounts[pos]++;
            WalkUFTreePP<StrategyTree, SetInitialStrategyContext> wt = new WalkUFTreePP<StrategyTree, SetInitialStrategyContext>();
            wt.OnNodeBegin = (t, s, d) =>
             {
                 Int64 n = s[d].NodeIdx;
                 s[d].HeroChildren.Clear();
                 if(d > 0)
                 {
                     s[d].Probab = s[d - 1].Probab;
                 }
                 if (d > _playersCount)
                 {
                     if (t.Nodes[n].IsPlayerAction(pos))
                     {
                         s[d - 1].HeroChildren.Add(n);
#if false
                         // Pure strategy
                         if (s[d].Probab > 0)
                         {
                             t.Nodes[n].Probab = s[d - 1].ChildrenCount == 1 ? 1 : 0;
                         }
                         s[d].Probab = t.Nodes[n].Probab;
#endif
                     }
                 }
             };
            wt.OnNodeEnd = (t, s, d) =>
             {
                 Int64 n = s[d].NodeIdx;
                 if(s[d].HeroChildren.Count > 0)
                 {
#if true
                     // Mixed strategy
                     double condProbab = 1.0 / s[d].HeroChildren.Count;
                     foreach(long ch in s[d].HeroChildren)
                     {
                         t.Nodes[ch].Probab = condProbab;
                     }
#endif

                 }
             };
            wt.Walk(_pt);
            ConvertCondToAbs.Convert(_pt, pos);
            string error;
            if (!VerifyAbsStrategy.Verify(_pt, pos, out error))
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

                _mcDealer.NextDeal(new int[][] { _hands[_heroPos] });
                DealOppCardsAndCalculateGV();                

                BestResponse();

                if (CurrentIterationCount >= _playersCount)
                {
                    // Calculate espsilon as soon as we have done SBR for each player at least once.
                    CurrentEpsilon = 0;
                    for (int p = 0; p < _playersCount; ++p)
                    {
                        CurrentEpsilon += LastBrValues[p];
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

        void DealOppCardsAndCalculateGV()
        {
            _oppGv.Fill(0);
            _hands[_heroPos].CopyTo(_hands[1 - _heroPos], 0);
#if true // full enum
            CardEnum.Combin(GameDef.DeckDescr, _handSizes[0], _hands[1 - _heroPos], 0, _hands[_heroPos],
                            _hands[_heroPos].Length, OnOppDeal, 0);
#else    // Random deal
            List<int> restDeck = new List<int>(GameDef.DeckDescr.FullDeckIndexes);
            foreach (int c in _hands[_heroPos])
            {
                restDeck.Remove(c);
            }
            // Deal one random card to the opponent.
            int oppCard = _rng.Next(restDeck.Count);
            _hands[1 - _heroPos][0] = oppCard;
            OnOppDeal(_hands[1 - _heroPos], 0);
#endif
        }

        class CalcOppValuesContext : WalkUFTreePPContext
        {
            public List<int> DealPath = new List<int>();
            public double StrProbab = 1.0;
        }

        void OnOppDeal(int [] cards, int param)
        {
            int oppPos = 1 - _heroPos;
            uint[] ranks = new uint[_playersCount]; 
            GameDef.GameRules.Showdown(GameDef, _hands, ranks);
            double oppShowdown = 0;
            if(ranks[_heroPos] > ranks[oppPos])
            {
                oppShowdown = -1;
            }
            else if(ranks[_heroPos] < ranks[oppPos])
            {
                oppShowdown = 1;
            }
            int[] oppAbstrCards = HandToAbstractCards(_hands[oppPos]);
            var wt = new WalkUFTreePP<StrategyTree, CalcOppValuesContext>();
            wt.OnNodeBegin = (t, s, d) =>
                                 {
                                     Int64 n = s[d].NodeIdx;
                                     if (d > 0)
                                     {
                                         s[d].DealPath = new List<int>(s[d - 1].DealPath);
                                         s[d].StrProbab = s[d - 1].StrProbab;
                                     }
                                     if (_pt.Nodes[n].IsDealerAction)
                                     {
                                         s[d].DealPath.Add(_pt.Nodes[n].Card);
                                     }
                                     else if (_pt.Nodes[n].IsPlayerAction(oppPos) && n > _playersCount)
                                     {
                                         s[d].StrProbab = _pt.Nodes[n].Probab;
                                     }
                                 };
            wt.OnNodeEnd = (t, s, d) =>
                               {
                                   Int64 n = s[d].NodeIdx;
                                   if (s[d].ChildrenCount == 0)
                                   {
                                       // A leaf.
                                       for(int r = 0; r < s[d].DealPath.Count; ++r)
                                       {
                                           if (s[d].DealPath[r] != oppAbstrCards[r])
                                           {
                                               // This deal paths is not of interest.
                                               return;
                                           }
                                       }
                                       uint atIdx = _ptExt[oppPos][n].AtIdx;
                                       UInt16 activePlayers = _at.Nodes[atIdx].ActivePlayers;
                                       UInt16 oppMask = (UInt16)(1 << oppPos);
                                       double gvPS = _ptExt[oppPos][n].PotFactor * s[d].StrProbab;
                                       double gv;
                                       if (activePlayers == (activePlayers & (~oppMask)))
                                       {
                                           // The opp folded and loses its inPot
                                           gv = -gvPS;
                                       }
                                       else if (activePlayers == oppMask)
                                       {
                                           // The hero. folds: opp wins
                                           gv = gvPS;
                                       }
                                       else
                                       {
                                           // Showdown (inpots are equal)
                                           gv = gvPS*oppShowdown;
                                       }
                                       _oppGv[atIdx] += gv;
                                   }
                               };
            wt.Walk(_pt);

        }

        private int [] HandToAbstractCards(int [] hand)
        {
            int[] abstractCards = new int[CardCount.Length];
            int handSize = 0;
            for(int r  = 0; r < abstractCards.Length; ++r)
            {
                handSize += _handSizes[r];
                abstractCards[r] = ChanceAbstraction.GetAbstractCard(hand, handSize);
            }
            return abstractCards;
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
            BestResponseValuesUp();
            BestResponseFinalize();

            if (TraceDir != null)
            {
                Vis.Show(this, _heroPos, GetTraceFileName(_heroPos, "tree", "", "gv"));
            }
        }

        class BrValuesUpContext : WalkUFTreePPContext
        {
            public List<int> DealPath = new List<int>();
            public double OldGv = 0;
        }

        void BestResponseValuesUp()
        {
            int[] heroAbstrCards = HandToAbstractCards(_hands[_heroPos]);
            var wt = new WalkUFTreePP<StrategyTree, BrValuesUpContext>();

            Node[] heroPtExt = _ptExt[_heroPos];

            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                s[d].OldGv = heroPtExt[n].GameValue;
                if (d > 0)
                {
                    s[d].DealPath = new List<int>(s[d - 1].DealPath);
                }
                if (_pt.Nodes[n].IsDealerAction)
                {
                    s[d].DealPath.Add(_pt.Nodes[n].Card);
                }
            };
            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                for (int r = 0; r < s[d].DealPath.Count; ++r)
                {
                    if (s[d].DealPath[r] != heroAbstrCards[r])
                    {
                        // This deal paths is not of interest.
                        return;
                    }
                }
                if (s[d].ChildrenCount == 0)
                {
                    // A leaf.
                    // Update game value. Overwrite the old one, because we have calculated
                    // a fully correct value for the current opponent strategy and this hero hand.
                    heroPtExt[n].GameValue = -_oppGv[heroPtExt[n].AtIdx];
                }
                // Now we know the game value of this node.
                if(n == 0)
                {
                    // Update total game values, counting the fact that actually the update is started from iteration 2.
                    // LastBrValues[_heroPos] = heroPtExt[n].GameValue / (IterationCounts[_heroPos] - 1);
                    LastBrValues[_heroPos] = heroPtExt[n].GameValue;
                    return;
                }
                long pn = s[d - 1].NodeIdx;
                if(_pt.Nodes[n].IsPlayerAction(_heroPos) && n > _playersCount)
                {
                    // maximize value
                    if(s[d-1].ChildrenCount == 1)
                    {
                        heroPtExt[pn].GameValue = heroPtExt[n].GameValue;
                    }
                    else
                    {
                        heroPtExt[pn].GameValue = Math.Max(heroPtExt[pn].GameValue, heroPtExt[n].GameValue);
                    }
                }
                else
                {
                    // Sum values by updating.
                    heroPtExt[pn].GameValue -= s[d].OldGv;
                    heroPtExt[pn].GameValue += heroPtExt[n].GameValue;
                }
            };
            wt.Walk(_pt);
        }


        class BrFinalizeContext : WalkUFTreePPContext
        {
            public List<int> DealPath = new List<int>();
            public bool ChildBrFound = false;
            public int ParentBrProbab = 1;
        }

        void BestResponseFinalize()
        {
            int[] heroAbstrCards = HandToAbstractCards(_hands[_heroPos]);
            var wt = new WalkUFTreePP<StrategyTree, BrFinalizeContext>();

            Node[] heroPtExt = _ptExt[_heroPos];

            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                s[d].ChildBrFound = false;
                if (d > 0)
                {
                    s[d].DealPath = new List<int>(s[d - 1].DealPath);
                    s[d].ParentBrProbab = s[d - 1].ParentBrProbab;
                }
                if (_pt.Nodes[n].IsDealerAction)
                {
                    s[d].DealPath.Add(_pt.Nodes[n].Card);
                }

                for (int r = 0; r < s[d].DealPath.Count; ++r)
                {
                    if (s[d].DealPath[r] != heroAbstrCards[r])
                    {
                        // This deal paths is not of interest.
                        return;
                    }
                }
                if (n == 0)
                {
                    return;
                }
                long pn = s[d - 1].NodeIdx;
                if (_pt.Nodes[n].IsPlayerAction(_heroPos) && n > _playersCount)
                {
                    int br = 0;
                    if (s[d].ParentBrProbab > 0)
                    {
                        if (heroPtExt[pn].GameValue == heroPtExt[n].GameValue && !s[d - 1].ChildBrFound)
                        {
                            // This was the BR
                            br = 1;
                            s[d - 1].ChildBrFound = true;
                        }
                    }
                    double p = _pt.Nodes[n].Probab * (IterationCounts[_heroPos] - 1);
                    p += br;
                    _pt.Nodes[n].Probab = p / IterationCounts[_heroPos];
                    s[d].ParentBrProbab = br;
                }
            };
            wt.OnNodeEnd = (t, s, d) =>
            {
 
            };
            wt.Walk(_pt);
        }

#if false
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
#endif

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
            output.Write("; last BR: ");
            for (int p = 0; p < LastBrValues.Length; ++p)
            {
                output.Write("{0}:{1:0.00000} ", p, LastBrValues[p]);
            }
            output.WriteLine();
        }

        #endregion

        #region Implementation - other

        void CleanUp()
        {
        }

        private void WriteEpsilonLog(TextWriter tw)
        {
            foreach (EpsilonLogEntry e in _epsilonLog)
            {
                tw.WriteLine("{0} {1}", e.Epsilon, e.TotalIterationsCount);
            }
        }

        void SwitchSnapshot()
        {
            _snapshotSwitcher.GoToNextSnapshot();
            _curSnapshotInfo = new SnapshotInfo(_snapshotSwitcher.CurrentSnapshotPath, _playersCount);
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

        #endregion
    }
}
