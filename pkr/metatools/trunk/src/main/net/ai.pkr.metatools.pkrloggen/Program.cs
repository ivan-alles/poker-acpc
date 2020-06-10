/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.random;
using ai.pkr.metabots;
using ai.pkr.metagame;
using System.IO;
using ai.lib.utils.commandline;
using ai.lib.utils;
using System.Diagnostics;

// Use-cases:
// 1. Generate fixed pockets for one player and random pockets for other players and random boards. 
//    Do this for single or multiple positions. 
//    Example:
//    0; p0{0 0 0} p1{0 0 0}; 0d{Tc 9d} 1d{? ?} s{? ? ?} s{?} s{?}.
//    1; p0{0 0 0} p1{0 0 0}; 0d{? ?} 1d{Tc 9d} s{? ? ?} s{?} s{?}.
//
// 
// 2. Generate fixes pockets for one player, enumerate all possible pockets for the other player and 
//    random boards. Do this for single or multiple positions.
//    0; p0{0 0 0} p1{0 0 0}; 0d{Tc 9d} 1d{#0 #0} s{? ? ?} s{?} s{?}.
//    1; p0{0 0 0} p1{0 0 0}; 0d{#0 #0} 1d{Tc 9d} s{? ? ?} s{?} s{?}.
//
// 3. Enumerate all possible boards and pockets. Useful for small experimental games
//    0; p0{0 0 0} p1{0 0 0}; 0d{#0} 1d{#1} s{#2}.
//    1; p0{0 0 0} p1{0 0 0}; 0d{#0} 1d{#1} s{#2}.
//

namespace ai.pkr.metatools.pkrloggen
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return 1;
            }

            if (_cmdLine.DebuggerLaunch)
            {
                Debugger.Launch();
            }

            _randomDealer = _cmdLine.RngSeed == 0 ? new SequenceRng() : new SequenceRng(_cmdLine.RngSeed);

            _deckDescr = XmlSerializerExt.Deserialize<DeckDescriptor>(Props.Global.Expand(_cmdLine.DeckDescriptorFile));

            GameLogParser logParser = new GameLogParser();
            logParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logParser_OnGameRecord);
            logParser.ParseFile(_cmdLine.ConfigFile);

            if(_cmdLine.EnumCount)
            {
                for(int r = 0; r < _dealRecords.Count; ++r)
                {
                    long[] counts = _dealRecords[r].EnumCombosCounts;
                    Console.Write("Rec #{0,2}: ", r);
                    if (counts.Length == 0)
                    {
                        Console.Write("N/A");
                    }
                    else
                    {
                        long total = 1;
                        for (int i = 0; i < counts.Length; ++i)
                        {
                            total *= counts[i];
                            Console.Write("{0}, ", counts[i]);
                        }
                        Console.Write("Total: {0}", total);
                    }
                    Console.WriteLine();
                }
                return 0;
            }

            int id = 0;
            for(int r = 0; r < _cmdLine.Repeat; ++r)
            {
                for(int g = 0; g < _dealRecords.Count; ++g)
                {
                    GameRecord completedDealRecord = _dealRecords[g].Generate(r);
                    completedDealRecord.Id = id.ToString();
                    ++id;
                    Console.Out.WriteLine(completedDealRecord);
                }
            }

            return 0;
        }

        static void logParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            _dealRecords.Add(new DealRecord(gameRecord, _deckDescr, _randomDealer));
        }

        static CommandLine _cmdLine = new CommandLine();
        static List<DealRecord> _dealRecords = new List<DealRecord>();
        private static SequenceRng _randomDealer; 
        static private DeckDescriptor _deckDescr;
    }
}
