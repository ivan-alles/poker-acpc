/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

#if DEBUG

// If defined, prints some info with Debug.Write().
//#define DEBUG_PRINT

#endif

//#define USE_CPP_LIB

//#define USE_ABSOLUTE_STATEGY // Not completely implemented


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
using ai.lib.algorithms.opt;

#if DEBUG_PRINT 
#warning Compiling with debug information influencing performance and memory requirements.
#endif

namespace ai.pkr.breb
{
    // Todo:
    // + Use cond. strategy (adjust initialization).
    // + Do manually a list of nodes to update in each leave.
    // + Do manually a showdown function, assume there is only one card per player.
    // + Write a function updating game values in leaves based on the opp. strategy  UpdateGv(heroPos)
    // + Verify all above by initializing for known Kuhn strategy.
    // - In each node define the nodes of opponent to calculate exploitation.
    // Iterations:
    // 1. UpdateHeroGv().
    // 2. Iterate through hero tree.
    // 4. Set up the GSS solver and run it.
    // 5. In GSS callback caluclate hero value v_h = sum(p_i * v_i)
    // 6. Run UpdateGv(oppPos).
    // 7. For each of opp. nodes to calculate exploitation calculate e = br - v_cur = max(v_i) - sum(p_i * v_i)
    // 8. Return v_h - e to the GSS solver.

    /// <summary>
    /// BR-exploit balancing algo prototype. Now implemented for Kuhn only.
    /// </summary>
    /// Developer nodes.
    /// Terms used in the documentation:
    /// Hero - the player currently updating his variables.
    /// Opponent - any non-hero player.
    public unsafe class Breb
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


            public Breb Solver
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

            public static void Show(Breb solver, int position, string fileName)
            {
                using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
                {
                    Vis vis = new Vis { Output = w, Solver = solver, Position = position };
                    vis.Show(solver._pt);
                }
            }

            protected override void OnTreeBeginFunc(UFToUniAdapter tree, int root)
            {
                GraphAttributes.label = string.Format("PT hero {0}", Position);
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

                bool isChangingVar = false;
                foreach (int vi in Solver._vars[1][Solver._vgi])
                {
                    if (n == vi)
                    {
                        isChangingVar = true;
                        break;
                    }
                }

                base.CustomizeNodeAttributes(aTree, n, stack, depth, attr);
                string nodeLabel = attr.label;
                nodeLabel += string.Format("\\ngv:{0:0.000}", Solver._ptExt[Position][n].GameValue);
                nodeLabel += string.Format("\\nbr:{0:0.000}", Solver._ptExt[Position][n].Br);
                //nodeLabel += string.Format("\\nve:{0:0.0000}", Solver._ptExt[Position][n].Velocity);

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
                    //nodeLabel += string.Format("\\nati:{0}", Solver._ptExt[Position][n].AtIdx);
                }
                if (n == 0)
                {
                    nodeLabel += string.Format("\\nGV:{0:0.000}", Solver.LastBrValues[Position]);
                }
                attr.label = nodeLabel;
                if (isChangingVar)
                {
                    attr.penwidth = 3;
                }
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

        public delegate bool OnIterationDoneDelegate(Breb solver);

        #endregion

        #region Public properties

        public GameDefinition GameDef
        {
            set;
            get;
        }

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

        public Breb()
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
            //DoIterations();
            //SwitchSnapshot();
            //SaveSnapshot();
            //_pt.Write(_curSnapshotInfo.StrategyFile);
            //CleanUp();
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
            public int AtIdx;
            public double PotFactor;
            public int[] OppExploitationNodes;

            public double Br;
            public double GameValue;
            public double Velocity;
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

        ChanceTree _ct;

        /// <summary>
        /// Current index of the variable group of the hero.
        /// </summary>
        int _vgi;

        /// <summary>
        /// For each position: an array containting arrays of sibling variables.
        /// </summary>
        int[][][] _vars;

        private ActionTree _at;

        /// <summary>
        /// An array of size of action tree. For each leave it will contain the list of the 
        /// corresponding opponent variables.
        /// </summary>
        public int[][] _oppLeaves;

        /// <summary>
        /// An array of size of action tree. For each leave it will contain the list of the cards that the opponent can hold.
        /// corresponding opponent variables.
        /// </summary>
        public string[][] _oppHands;

        public Dictionary<string, int> _ctIdx;


        /// <summary>
        /// Position of the player currently doing BR.
        /// </summary>
        int _heroPos;

        private double _iterationTime;



        #endregion

        #region Implementation - initialization

        static Breb()
        {
            //CppLib.Init();
        }

        /// <summary>
        /// Contains data required for initialization only
        /// </summary>
        class InitData
        {
            public InitData(Breb solver)
            {
                int playersCount = solver._at.PlayersCount;
                AtChildrenIndex = new UFTreeChildrenIndex(solver._at);
                PlayerCt = ExtractPlayerChanceTree.ExtractS(solver._ct, 0);
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
             _ct = ChanceTree.Read<ChanceTree>(ChanceTreeFile);
             _oppLeaves = new int[_at.NodesCount][];
             _oppHands = new string[_at.NodesCount][];
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

            _vars = new int[_playersCount][][];
            for (int p = 0; p < _playersCount; ++p)
            {
                _vars[p] = new int[0][];
            }

            bool isNewSnapshot = !_snapshotSwitcher.IsSnapshotAvailable;
            if (isNewSnapshot)
            {
                CreateNewSnapshot();
            }
            //LoadSnapshot();

            CreatePlayerTrees();
            IndexChanceTree();

#if USE_ABSOLUTE_STATEGY
            for (int p = 0; p < 2; ++p)
            {
                ConvertCondToAbs.Convert(_pt, p);
                string error;
                if (!VerifyAbsStrategy.Verify(_pt, p, out error))
                {
                    throw new ApplicationException(String.Format("Initial strategy inconsistent: {0}", error));
                }
            }
#else
            for (int p = 0; p < 2; ++p)
            {
                string error;
                if (!VerifyCondStrategy.Verify(_pt, p, out error))
                {
                    throw new ApplicationException(String.Format("Initial strategy inconsistent: {0}", error));
                }
            }
#endif

            int freezeCount = 0;
            Random rnd = new Random();

            for (int i = 0; i < 100000; ++i)
            {
                for (int heroPos = 0; heroPos < 2; ++heroPos)
                {
                    int oppPos = 1 - heroPos;
                    _vgi = i%_vars[heroPos].Length;

                    //CalculateVelocity(1, new List<int>(_vars[1][_vgi]));
                    //CalculateVelocity(0, new List<int>(new int[] { 4, 9 }));

                    double v0 = _pt.Nodes[_vars[heroPos][_vgi][1]].Probab;

                    GoldenSectionSearch solver = new GoldenSectionSearch();
                    solver.F = (double x) =>
                                   {
                                       _pt.Nodes[_vars[heroPos][_vgi][0]].Probab = 1 - x;
                                       _pt.Nodes[_vars[heroPos][_vgi][1]].Probab = x;
                                       //double heroGv = CalculateGv(heroPos);

                                       //ConvertCondToAbs.Convert(_pt, 0);
                                       //ConvertCondToAbs.Convert(_pt, 1);
                                       //StrategyTree [] st = new StrategyTree[]{_pt, _pt};
                                       //GameValue hgv = new GameValue { ActionTree = _at, ChanceTree = _ct, Strategies = st};
                                       //hgv.Solve();
                                       double oppGv = CalculateGv(oppPos);
                                       double oppBr = CalculateBr(oppPos);
                                       double f = oppBr + oppGv;
                                       return f;
                                   };

                    double v, func;
                    bool noDelta = false;
                    try
                    {
                        v = solver.Solve(0, 1, 0.0000001, true);
#if false
                        func = solver.F(v);
                        for (double eps = 0.5; eps >= 0.005; eps *= 0.5 )
                        {
                            if (v + eps <= 1.0)
                            {
                                if (FloatingPoint.AreEqual(solver.F(v + eps), func, 0.0000001))
                                {
                                    eps *= rnd.NextDouble()*0.5;
                                    if (FloatingPoint.AreEqual(solver.F(v + eps), func, 0.0000001))
                                    {
                                        v += eps;
                                        noDelta = true;
                                        break;
                                    }
                                }
                            }
                            else if (v - eps >= 0.0)
                            {
                                if (FloatingPoint.AreEqual(solver.F(v - eps), func, 0.0000001))
                                {
                                    eps *= rnd.NextDouble() * 0.5;
                                    if (FloatingPoint.AreEqual(solver.F(v - eps), func, 0.0000001))
                                    {
                                        v -= eps;
                                        noDelta = true;
                                        break;
                                    }
                                }
                            }
                        }
#endif
                    }
                    catch (ApplicationException)
                    {
                        double f0 = solver.F(0);
                        double f1 = solver.F(1);
                        v = f0 < f1 ? 0 : 1;
                    }
                    func = solver.F(v);
                    double heroGv1 = CalculateGv(heroPos);

                    Console.WriteLine("Pos: {0}, it: {1,3}, vid: {2,3}, var: {3:0.00000}, delta: {4:0.00000}, gv: {5:0.0000000}, f: {6:0.0000000}", 
                        heroPos, i, _vgi, v, v - v0, heroGv1, func);


                    if (TraceDir != null)
                    {
                        IterationCounts[heroPos] = i;
                        Vis.Show(this, heroPos, GetTraceFileName(heroPos, "h", "", "gv"));
                        Vis.Show(this, heroPos, GetTraceFileName(heroPos, "o", "", "gv"));
                    }

                    freezeCount++;
                    if (!noDelta && Math.Abs(v - v0) > 1e-5)
                    {
                        freezeCount = 0;
                    }
                    if (freezeCount > _vars[0].Length + _vars[1].Length)
                    {
                        goto end;
                    }
                }
            }
            end:

            //PrintInitDone();

            // Clean-up
            _init = null;
            double time = (DateTime.Now - startTime).TotalSeconds;
            if (IsVerbose)
            {
                Console.WriteLine("Initialization done in {0:0.0} s", time);
            }
        }

        class IndexCtContext: WalkUFTreePPContext
        {
            public string [] Hands = new string[2] {"", ""};
        }

        private void IndexChanceTree()
        {
            _ctIdx = new Dictionary<string, int>();
            var wt = new WalkUFTreePP<ChanceTree, IndexCtContext>();
            wt.OnNodeBegin = (t, s, d) =>
             {
                 Int64 n = s[d].NodeIdx;
                 int pos = t.Nodes[n].Position;
                 if (d > 0)
                 {
                     s[d].Hands[pos] = s[d - 1].Hands[pos] + "." + t.Nodes[n].Card.ToString();
                     s[d].Hands[1 - pos] = s[d - 1].Hands[1 - pos];
                     if (d % _playersCount == 0)
                     {
                         _ctIdx.Add(s[d].Hands[0] + "-" + s[d].Hands[1], (int)n); 
                     }
                 }
             };
            wt.Walk(_ct);
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
            public int VarsIdx = -1;
            public string Hand = "";
        }

        static void PushBack<T>(ref T[] a, T v)
        {
            int size = a.Length;
            Array.Resize(ref a, size + 1);
            a[size] = v;
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
                                            s[d].VarsIdx = -1;
                                            s[d].Hand = s[d-1].Hand;
                                         }
                                         if (_pt.Nodes[n].IsDealerAction)
                                         {
                                             s[d].Hand += "." + _pt.Nodes[n].Card;
                                         }
                                         else if (n > 0)
                                         {
                                             s[d].InPot[_pt.Nodes[n].Position] += _pt.Nodes[n].Amount;

                                             s[d].AtIdx = FindActionTreeNodeIdx(t, n, s[d].AtIdx,
                                                                                s[d - 1].ChildrenCount - 1);

                                             if (heroPos == 0)
                                             {
                                                 if (n > _playersCount)
                                                 {
                                                     if (s[d - 1].VarsIdx == -1)
                                                     {
                                                         s[d - 1].VarsIdx = _vars[_pt.Nodes[n].Position].Length;
                                                         PushBack(ref _vars[_pt.Nodes[n].Position], new int[0]);
                                                     }
                                                     PushBack(ref _vars[_pt.Nodes[n].Position][s[d - 1].VarsIdx], (int)n);
                                                 }
                                             }
                                         }
                                         _ptExt[heroPos][n].AtIdx = (int)s[d].AtIdx;
                                     };
                wt.OnNodeEnd = (t, s, d) =>
                                   {
                                       Int64 n = s[d].NodeIdx;
                                       if(s[d].ChildrenCount == 0)
                                       {
                                           // A leaf
                                           Debug.Assert(_playersCount == 2);
                                           _ptExt[heroPos][n].PotFactor = Math.Min(s[d].InPot[0], s[d].InPot[1]);
                                           if (heroPos == 0)
                                           {
                                               int atIdx = _ptExt[heroPos][n].AtIdx;
                                               if (_oppLeaves[atIdx] == null)
                                               {
                                                   _oppLeaves[atIdx] = new int[0];
                                                   _oppHands[atIdx] = new string[0];
                                               }
                                               PushBack(ref _oppLeaves[atIdx], (int)n);
                                               PushBack(ref _oppHands[atIdx], s[d].Hand);
                                           }
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

                DoBreb();

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


        class CalculateGvContext: WalkUFTreePPContext
        {
            public double AbsStrProbab = 1.0;
            public string Hand = "";
        }

        double CalculateGv(int heroPos)
        {
            int oppPos = 1 - heroPos;
            // Clear game values of the hero
            for(int n = 0; n < _ptExt[heroPos].Length; ++n)
            {
                _ptExt[heroPos][n].GameValue = 0;
            }
            // Iterate through the opp tree and update the game values in the leaves of the hero.
            var wt = new WalkUFTreePP<StrategyTree, CalculateGvContext>();
            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (d > 0)
                {
                    s[d].Hand = s[d - 1].Hand;
                    s[d].AbsStrProbab = s[d - 1].AbsStrProbab;
                }
                if (_pt.Nodes[n].IsDealerAction)
                {
                    s[d].Hand += "." + _pt.Nodes[n].Card.ToString();
                }
                else if (_pt.Nodes[n].IsPlayerAction(oppPos) && n > _playersCount)
                {
#if USE_ABSOLUTE_STATEGY
                    s[d].AbsStrProbab = _pt.Nodes[n].Probab;
#else
                    s[d].AbsStrProbab *= _pt.Nodes[n].Probab;
#endif
                }
            };
            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (s[d].ChildrenCount == 0)
                {
                    // A leaf.
                    int atIdx = _ptExt[oppPos][n].AtIdx;
                    UInt16 activePlayers = _at.Nodes[atIdx].ActivePlayers;
                    UInt16 oppMask = (UInt16)(1 << oppPos);
                    double gvPS = _ptExt[oppPos][n].PotFactor * s[d].AbsStrProbab;
                    int[] leaves = _oppLeaves[_ptExt[oppPos][n].AtIdx];
                    string[] hands = _oppHands[_ptExt[oppPos][n].AtIdx];
                    for (int h = 0; h < hands.Length; ++h)
                    {
                        double oppGv;
                        //KuhnShowdown(h, s[d].Card, out chanceProbab, out valH, out valO);
                        string ctKey = hands[h] + "-" + s[d].Hand;
                        int ctNode;
                        if (!_ctIdx.TryGetValue(ctKey, out ctNode))
                        {
                            // Impossible combination
                            continue;
                        }
                        double chanceProbab = _ct.Nodes[ctNode].Probab;
                        if (activePlayers == (activePlayers & (~oppMask)))
                        {
                            // The opp folded and loses its inPot
                            oppGv = -gvPS * chanceProbab;
                        }
                        else if (activePlayers == oppMask)
                        {
                            // The hero. folds: opp wins
                            oppGv = gvPS * chanceProbab;
                        }
                        else
                        {
                            // Showdown (inpots are equal)
                            double [] potShares = new double[2];
                            _ct.Nodes[ctNode].GetPotShare(0x3, potShares); 
                            oppGv = 2.0 * gvPS * (potShares[1] - 0.5) * chanceProbab;
                        }

                        int heroNode = leaves[h];
                        _ptExt[heroPos][heroNode].GameValue -= oppGv;
                    }
                }
            };
            wt.Walk(_pt);

            // Now we know the game values in
            // Iterate through the hero tree and propagate the values up the tree.
            wt = new WalkUFTreePP<StrategyTree, CalculateGvContext>();
            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if(n == 0)
                {
                    return;
                }
                Int64 pn = s[d-1].NodeIdx;

                if (_pt.Nodes[n].IsPlayerAction(heroPos) && n > _playersCount)
                {
                    double probab = _pt.Nodes[n].Probab;
                    _ptExt[heroPos][pn].GameValue += probab * _ptExt[heroPos][n].GameValue;
                }
                else
                {
                    _ptExt[heroPos][pn].GameValue += _ptExt[heroPos][n].GameValue;
                }
            };
            wt.Walk(_pt);
            return _ptExt[heroPos][0].GameValue;
        }


        class CalculateBrContext : WalkUFTreePPContext
        {
        }

        /// <summary>
        /// Calculates br for the given position assuming that the values in leaves are already calculated.
        /// </summary>
        /// <param name="heroPos"></param>
        double CalculateBr(int heroPos)
        {
            int oppPos = 1 - heroPos;
            // Clear game values of the hero
            for (int n = 0; n < _ptExt[heroPos].Length; ++n)
            {
                _ptExt[heroPos][n].Br = 0;
            }

            var wt = new WalkUFTreePP<StrategyTree, CalculateBrContext>();
            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (n == 0)
                {
                    return;
                }
                if (s[d].ChildrenCount == 0)
                {
                    // A leaf
                    _ptExt[heroPos][n].Br = _ptExt[heroPos][n].GameValue;
                }
                Int64 pn = s[d - 1].NodeIdx;
                if (s[d - 1].ChildrenCount == 1)
                {
                    _ptExt[heroPos][pn].Br = _ptExt[heroPos][n].Br;
                }
                else
                {
                    if (_pt.Nodes[n].IsPlayerAction(heroPos) && n > _playersCount)
                    {
                        _ptExt[heroPos][pn].Br = Math.Max(_ptExt[heroPos][pn].Br, _ptExt[heroPos][n].Br);
                    }
                    else
                    {
                        _ptExt[heroPos][pn].Br += _ptExt[heroPos][n].Br;
                    }
                }
            };
            wt.Walk(_pt);
            return _ptExt[heroPos][0].Br;
        }
#if false
        class CalculateVelocityContext : WalkUFTreePPContext
        {
            public int Card;
            public int HeroNodeIdx = -1;
        }

        /// <summary>
        /// Updates velocity for the given hero and node. The updated velocities are written to the opp. tree.
        /// </summary>
        void CalculateVelocity(int heroPos, List<int> heroNodes)
        {
            Console.Write(String.Format("Calcualte velocity for hero {0}, vars: ", heroPos));
            foreach (int hn in heroNodes)
            {
                Console.Write(String.Format("{0} ", hn));
            }
            Console.WriteLine("");

            int oppPos = 1 - heroPos;
            for (int n = 0; n < _ptExt[heroPos].Length; ++n)
            {
                _ptExt[oppPos][n].Velocity = 0;
            }
            // Iterate through the hero tree and update the velocities in the opp tree.
            var wt = new WalkUFTreePP<StrategyTree, CalculateVelocityContext>();
            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (d > 0)
                {
                    s[d].Card = s[d - 1].Card;
                    s[d].HeroNodeIdx = s[d - 1].HeroNodeIdx;
                }
                if (_pt.Nodes[n].IsDealerAction)
                {
                    s[d].Card = _pt.Nodes[n].Card;
                }
                else if (_pt.Nodes[n].IsPlayerAction(heroPos) && n > _playersCount)
                {
                    if (s[d].HeroNodeIdx == -1)
                    {
                        s[d].HeroNodeIdx = heroNodes.IndexOf((int)n);
                    }
                }
            };
            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (s[d].ChildrenCount == 0)
                {
                    if (s[d].HeroNodeIdx != -1)
                    {
                        int atIdx = _ptExt[heroPos][n].AtIdx;
                        UInt16 activePlayers = _at.Nodes[atIdx].ActivePlayers;
                        UInt16 heroMask = (UInt16) (1 << heroPos);
                        double vPS = _ptExt[heroPos][n].PotFactor;
                        int[] oppLeaves = _oppLeaves[_ptExt[oppPos][n].AtIdx];
                        for (int oppCard = 0; oppCard < oppLeaves.Length; ++oppCard)
                        {
                            double heroV;
                            double valH, valO, chanceProbab;
                            KuhnShowdown(s[d].Card, oppCard, out chanceProbab, out valH, out valO);
                            if (activePlayers == (activePlayers & (~heroMask)))
                            {
                                // The hero folded and loses its inPot
                                heroV = -vPS*chanceProbab;
                            }
                            else if (activePlayers == heroMask)
                            {
                                // The opp folds: hero wins
                                heroV = vPS*chanceProbab;
                            }
                            else
                            {
                                // Showdown (inpots are equal)
                                heroV = vPS*valH;
                            }

                            int oppNode = oppLeaves[oppCard];
                            Debug.Assert(_ptExt[oppPos][oppNode].Velocity == 0, "Each velocity must be updated once");
                            if (s[d].HeroNodeIdx == 0)
                            {
                                // The first variable is dependent, so change the sign.
                                heroV = -heroV;
                            }
                            _ptExt[oppPos][oppNode].Velocity = heroV;
                        }
                    }
                }
            };
            wt.Walk(_pt);
#if false
            // Now we know the game values in
            // Iterate through the hero tree and propagate the values up the tree.
            wt = new WalkUFTreePP<StrategyTree, CalculateGvContext>();
            wt.OnNodeEnd = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (n == 0)
                {
                    return;
                }
                Int64 pn = s[d - 1].NodeIdx;

                if (_pt.Nodes[n].IsPlayerAction(heroPos) && n > _playersCount)
                {
                    double probab = _pt.Nodes[n].Probab;
                    _ptExt[heroPos][pn].GameValue += probab * _ptExt[heroPos][n].GameValue;
                }
                else
                {
                    _ptExt[heroPos][pn].GameValue += _ptExt[heroPos][n].GameValue;
                }
            };
            wt.Walk(_pt);
#endif
        }

#endif

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

        private void DoBreb()
        {
            if (TraceDir != null)
            {
                Vis.Show(this, _heroPos, GetTraceFileName(_heroPos, "tree", "", "gv"));
            }
        }

#if false
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
#endif
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
