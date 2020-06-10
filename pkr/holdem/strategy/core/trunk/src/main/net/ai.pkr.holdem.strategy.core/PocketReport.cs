/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metatools;
using System.IO;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.lib.algorithms;
using ai.pkr.stdpoker;

namespace ai.pkr.holdem.strategy.core
{
    /// <summary>
    /// Reports results by HE preflop pockets.
    /// </summary>
    public class PocketReport: IGameLogReport
    {
        #region IGameLogReport Members

        public void ShowHelp(TextWriter tw)
        {
            tw.WriteLine("Reports results by HE preflop pockets.");
            tw.WriteLine("Parameters:");
            tw.WriteLine("HeroName (string, required): name of the player to report about.");
        }

        public void Configure(Props pm)
        {
            _hero = pm.Get("HeroName");
        }

        public string Name
        {
            get;
            set;
        }

        public void Print(TextWriter tw)
        {
            tw.WriteLine("{0}", Name);
            int[] totalCount = new int[_positions.Count];
            for (int pos = 0; pos < _positions.Count; pos++)
            {
                foreach (Record r in _positions[pos])
                {
                    totalCount[pos] += r.count;
                }
            }

            tw.Write("Pos ");
            for (int pos = 0; pos < _positions.Count; pos++)
            {
                tw.Write("                                    {0}", pos);
            }
            tw.WriteLine();

            tw.Write("Poc ");
            for (int pos = 0; pos < _positions.Count; pos++)
            {
                tw.Write("     Count    Result     Av,mb   Freq");
            }
            tw.WriteLine();

            Record [] totalRecord = new Record[_positions.Count].Fill(i => new Record());

            for (HePocketKind pocketKind = 0; pocketKind < HePocketKind.__Count; ++pocketKind)
            {
                Console.Write("{0,-3} ", pocketKind.ToString().Substring(1));
                for (int pos = 0; pos < _positions.Count; pos++)
                {
                    Record r = _positions[pos][(int) pocketKind];
                    totalRecord[pos].result += r.result;
                    totalRecord[pos].count += r.count;
                    PrintRecord(tw, totalCount[pos], r);
                }
                Console.WriteLine();
            }
            Console.Write("ALL ");
            for (int pos = 0; pos < _positions.Count; pos++)
            {
                PrintRecord(tw, totalCount[pos], totalRecord[pos]);
            }
            Console.WriteLine();
        }

        private void PrintRecord(TextWriter tw, int totalCount, Record r)
        {
            double averageValue = r.count == 0 ? 0 : r.result / r.count;
            double freq = totalCount == 0 ? 0.0 : (double)r.count / totalCount;
            tw.Write("    {0,6} {1,9:0.0} {2,9:0.00} {3,6:0.0}",
                r.count, r.result, averageValue * 1000, freq * 1000);
        }

        public void Update(GameRecord gameRecord)
        {
            for(int pos = 0; pos < gameRecord.Players.Count; ++pos)
            {
                GameRecord.Player player = gameRecord.Players[pos];
                if(player.Name == _hero)
                {
                    Allocate(pos);
                    foreach(PokerAction action in gameRecord.Actions)
                    {
                        if(action.Kind == Ak.d && action.Position == pos)
                        {
                            CardSet pocket = StdDeck.Descriptor.GetCardSet(action.Cards);
                            HePocketKind pocketKind = HePocket.CardSetToKind(pocket);
                            _positions[pos][(int) pocketKind].count++;
                            _positions[pos][(int)pocketKind].result += player.Result;
                        }
                    }
                    break;
                }
            }
        }

        #endregion

        void Allocate(int pos)
        {
            for(int p = _positions.Count; p < pos + 1; ++p)
            {
                _positions.Add(new Record[(int)HePocketKind.__Count]);
            }
        }

        struct Record
        {
            public int count;
            public double result;
        }

        private string _hero;
        private List<Record[]> _positions = new List<Record[]>(11);
    }
}
