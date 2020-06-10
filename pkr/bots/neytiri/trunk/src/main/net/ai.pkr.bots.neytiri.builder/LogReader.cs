/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.metastrategy;
using System.Diagnostics;
using ai.pkr.stdpoker;

namespace ai.pkr.bots.neytiri.builder
{
    class LogReader
    {
        private ActionTreeNode _curNode;
        //private SuitEquivalence _suitEq = new SuitEquivalence();
        private int _oppPosition;
        private CardSet _oppPocket;
        private int _oppBucket;
        private int _oppBucketRound;
        private CardSet _board;
        

        public LogReader()
        {
        }

        public ActionTree ActionTree
        {
            set;
            get;
        }

        public string Opponent
        {
            set;
            get;
        }


        public void ReadPath(string path, string includeFiles)
        {
            DateTime start = DateTime.Now;
            
            GameLogParser logParser = new GameLogParser{ Verbose = true, IncludeFiles = includeFiles};

            logParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logParser_OnGameRecord);

            logParser.ParsePath(path);

            DateTime finish = DateTime.Now;
            double sec = (finish - start).TotalSeconds;

            Console.WriteLine("Parsed {0} files, {1:0,0} games, in {2:0.0} sec, {3:0.0} g/s, {4} error(s).", 
                logParser.FilesCount, logParser.GamesCount,
                sec, sec == 0 ? 0 : logParser.GamesCount / sec,
                logParser.ErrorCount);

        }

        void logParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            _oppPosition = -1;
            _board = CardSet.Empty;
            _oppPocket = CardSet.Empty;
            _oppBucketRound = -1;

            for(int p = 0; p < gameRecord.Players.Count; ++p)
            {
                if (gameRecord.Players[p].Name == Opponent)
                {
                    _oppPosition = p;
                    _curNode = ActionTree.Positions[_oppPosition];
                }
            }

            foreach (PokerAction action in gameRecord.Actions)
            {
                if (_oppPosition == -1)
                {
                    Console.WriteLine("Player" + Opponent + " was not found");
                    source.ErrorCount++;
                    return;
                }
                switch (action.Kind)
                {
                    case Ak.d:
                        if (action.Position == _oppPosition)
                        {
                            _oppPocket = StdDeck.Descriptor.GetCardSet(action.Cards);
                        }
                        break;
                    case Ak.s:
                        {
                            CardSet shared = StdDeck.Descriptor.GetCardSet(action.Cards);
                            _board.UnionWith(shared);
                        }
                        break;
                }
                Ak actionKind = (Ak) action.Kind;
                ActionTreeNode n = _curNode.FindChildByAction(actionKind);

                if (_oppPocket.bits != 0)
                {
                    if (_oppBucketRound != n.State.Round)
                    {
                        _oppBucket = ActionTree.Bucketizer.GetBucket(_oppPocket, _board, n.State.Round);
                        _oppBucketRound = n.State.Round;
                    }
                    n.OppBuckets.Update(_oppBucket, 1);
                }
                _curNode = n;

            }
            if (!gameRecord.IsGameOver)
            {
                Console.Write("Warning: unfinished game");
                source.ErrorCount++;
            }
        }
    }
}
