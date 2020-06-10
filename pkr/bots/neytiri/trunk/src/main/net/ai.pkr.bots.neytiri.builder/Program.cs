/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ai.lib.utils.commandline;
using ai.lib.algorithms.tree;
using ai.pkr.holdem;
using ai.pkr.metagame;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using ai.lib.utils;
using ai.pkr.stdpoker;
using ai.pkr.holdem.strategy;

namespace ai.pkr.bots.neytiri.builder
{
    class Program
    {
        static GameDefinition _gameDef = null;
        private static int _boardSize = 5;
        private static ActionTree _oppActionTree;
        private static ActionTree _neytiri;
        private static Bucketizer _bucketizer;
        static CommandLine _cmdLine = new CommandLine();
        private static List<CardSet> _pockets;

        static void Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return;
            }

            _pockets = new List<CardSet>();
            for (int i = 0; i < _cmdLine.pockets.Length / 4; ++i)
            {
                String pocket = _cmdLine.pockets.Substring(i * 4, 2) + " " +
                                _cmdLine.pockets.Substring(i * 4 + 2, 2);
                _pockets.Add(StdDeck.Descriptor.GetCardSet(pocket));
            }

            if(_cmdLine.bucketizer != "")
            {
                _bucketizer = XmlSerializerExt.Deserialize<Bucketizer>(_cmdLine.bucketizer);
            }

            if (!String.IsNullOrEmpty(_cmdLine.gameDef))
            {
                XmlSerializerExt.Deserialize(out _gameDef, _cmdLine.gameDef);
                _boardSize = 0;
                for (int r = 0; r < _gameDef.RoundsCount; ++r)
                    _boardSize += _gameDef.SharedCardsCount[r];
            }

            if (!String.IsNullOrEmpty(_cmdLine.oppActionTreeFile) && File.Exists(_cmdLine.oppActionTreeFile))
            {
                Console.WriteLine("Reading opponent action tree from: {0} ...", _cmdLine.oppActionTreeFile);
                XmlSerializerExt.Deserialize(out _oppActionTree, _cmdLine.oppActionTreeFile);
            }

            if (_cmdLine.neytiri != "")
            {
                Console.WriteLine("Reading Neytiri strategy from {0} ...", _cmdLine.neytiri);
                XmlSerializerExt.Deserialize(out _neytiri, _cmdLine.neytiri);
            }

            if (_cmdLine.processLogs)
            {
                UpdateActionTree();
            }

            if(_cmdLine.monteCarlo)
            {
                MonteCarlo();
            }

            if(_cmdLine.showOppActionTree)
            {
                showOppActionTree();
            }

            if (_cmdLine.printNeytiryPf)
                PrintNeytiryPreflop();

            if (_cmdLine.dumpNode)
                DumpNode();
        }

        static WalkTree<ActionTree, ActionTreeNode, int> _createPreflopValues =
        new WalkTree<ActionTree, ActionTreeNode, int>
        {
            OnNodeBegin = (t, n, s, d) =>
            {
                if (n.State.Round > 0) return false;
                n.PreflopValues = new double[(int)HePocketKind.__Count];
                return true;
            }
        };

        private static void MonteCarlo()
        {
            // Create new arrays for preflop values
            for(int pos = 0; pos < 2; ++pos)
            {
                _createPreflopValues.Walk(_neytiri, _neytiri.Positions[pos]);
            }

            DateTime start = DateTime.Now;

            for (int ourPos = 0; ourPos < _neytiri.Positions.Length; ++ourPos)
            {
                Console.WriteLine("Position {0}", ourPos);

                ActionTreeNode root = _neytiri.Positions[1 - ourPos];
                List<ActionTreeNode> strategyPath = new List<ActionTreeNode>(100);
                strategyPath.Add(root);
                // Advance to the node where we get cards
                while (strategyPath[strategyPath.Count - 1].State.CurrentActor != ourPos)
                {
                    strategyPath.Add(strategyPath[strategyPath.Count - 1].Children[0]);
                } 
                for(HePocketKind pocketKind = 0; pocketKind < HePocketKind.__Count; ++pocketKind)
                {
                    CardSet pocket = HePockets.PocketKindToCardSet(pocketKind);

                    if (_pockets.Count > 0 && !_pockets.Contains(pocket))
                    {
                        // Skip the pocket if command line specifies which pockets to include and it's not there.
                        continue;
                    }

                    Console.Write("{0} ", pocketKind.ToString().Substring(1));

                    MonteCarloStrategyFinder.DoMonteCarlo(_neytiri,
                        ourPos, pocket, 0, "",
                        strategyPath, _cmdLine.mcCount);

                    WalkTree<ActionTree, ActionTreeNode, int> copyValues =
                    new WalkTree<ActionTree, ActionTreeNode, int>
                    {
                        OnNodeBegin = (t, n, s, d) =>
                        {
                            if (n.State.Round > 0) return false;
                            n.PreflopValues[(int)pocketKind] = n.Value;
                            return true;
                        }
                    };
                    copyValues.Walk(_neytiri, root);
                }
                Console.WriteLine();
            }
            DateTime finish = DateTime.Now;
            double sec = (finish - start).TotalSeconds;
            Console.WriteLine("Done {0} monte-carlo trials for every pocket in each position in {1:0.0} sec",
                _cmdLine.mcCount, sec);
            Console.WriteLine("Writing Neytiri strategy to {0} ...", _cmdLine.neytiri);
            _neytiri.XmlSerialize(_cmdLine.neytiri, new XmlWriterSettings { Indent = false });
        }

        private static void CreateOppActionTree()
        {
            Console.WriteLine("Creating new Opponent action tree from game def: {0} ...", _cmdLine.gameDef);

            _oppActionTree = new ActionTree(_gameDef, _bucketizer);

            _oppActionTree.Build();
            Console.WriteLine("Opponent action tree created.");

            // Workaround for disabled Assert dialog
            DefaultTraceListener trListener = (DefaultTraceListener)Debug.Listeners[0];
            trListener.AssertUiEnabled = true;
        }

        static void SaveActionTree()
        {
            Console.WriteLine("Writing opponent action tree to {0} ...", _cmdLine.oppActionTreeFile);

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = false;
            XmlSerializerExt.Serialize(_oppActionTree, _cmdLine.oppActionTreeFile, xws);
        }

        private static void UpdateActionTree()
        {
            if (string.IsNullOrEmpty(_cmdLine.opponent))
            {
                Console.WriteLine("Opponent name is not set");
                return;
            }

            if (_oppActionTree == null)
                CreateOppActionTree();

            LogReader lr = new LogReader
                               {
                                   Opponent = _cmdLine.opponent,
                                   ActionTree = _oppActionTree
                               };
            lr.ReadPath(_cmdLine.gameLogsPath, _cmdLine.includeLogs);
            
            if(_cmdLine.oppActionTreeFile != "")
                SaveActionTree();
        }

        private static void showOppActionTree()
        {
            Console.WriteLine("Creating action tree graphviz files ...");

            string fileBase = Path.Combine(Path.GetDirectoryName(_cmdLine.oppActionTreeFile),
                                           Path.GetFileNameWithoutExtension(_cmdLine.oppActionTreeFile));

            for (int p = 0;  p < _oppActionTree.Positions.Length; ++p)
            {
                String fileName = String.Format("{0}-{1}.gv", fileBase, p);
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    ActionTreeVisualizer tv = new ActionTreeVisualizer
                                                  {
                                                      MatchPath = _cmdLine.matchPath,
                                                      ShowBuckets = _cmdLine.showBuckets,
                                                      PruneIf = c => c.Node.State.Round > _cmdLine.maxRound
                                                  };
                    tv.Write(_oppActionTree, _oppActionTree.Positions[p], sw);
                    sw.Close();
                }
            }
        }

        static void PrintNeytiryPreflop()
        {
            double totalForAllPositions = 0;
            for (int ourPos = 0; ourPos < _neytiri.Positions.Length; ++ourPos)
            {
                Console.WriteLine("Neytiri preflop play for position {0}", ourPos);
                double[] ra = _neytiri.Positions[1 - ourPos].Children[0].Children[0].PreflopValues;
                double totalValue = 0;
                for (int pocket = 0; pocket < ra.Length; ++pocket)
                {
                    totalValue += ra[pocket];
                }

                totalForAllPositions += totalValue;

                for (int pocket = 0; pocket < ra.Length; ++pocket)
                {
                    string cards = ((HePocketKind)pocket).ToString().Substring(1);
                    Console.WriteLine("{0};{1:0.0000}", cards, ra[pocket]);
                }
                Console.WriteLine("TOTAL VALUE: {0:0.0000000}", totalValue);
                Console.WriteLine();
            }
            Console.WriteLine("TOTAL VALUE FOR ALL POSITIONS: {0:0.0000000}", totalForAllPositions);
        }

        static void DumpNode()
        {
            string[] idElements = _cmdLine.nodeId.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            int pos = int.Parse(idElements[0]);
            int id = int.Parse(idElements[1]);
            ActionTreeNode node = FindFirstPreOrder.Find<ActionTree, ActionTreeNode, int>(
                _oppActionTree, _oppActionTree.Positions[pos], n => n.Id == id);
            node.Children.Clear();
            string fileBase = Path.Combine(Path.GetDirectoryName(_cmdLine.oppActionTreeFile),
                               Path.GetFileNameWithoutExtension(_cmdLine.oppActionTreeFile));
            string fileName = fileBase + "." + _cmdLine.nodeId + ".xml";
            node.XmlSerialize(fileName);
        }
    }
}
