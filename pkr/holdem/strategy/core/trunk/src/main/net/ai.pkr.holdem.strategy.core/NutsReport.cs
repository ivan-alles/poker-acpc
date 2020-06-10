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
using System.Diagnostics;

namespace ai.pkr.holdem.strategy.core
{
    /// <summary>
    /// Reports results about playing nuts.
    /// </summary>
    public class NutsReport : IGameLogReport
    {
        #region IGameLogReport Members

        public void ShowHelp(TextWriter tw)
        {
            tw.WriteLine("Reports results about playing nuts.");
            tw.WriteLine("Parameters:");
            tw.WriteLine("HeroName (string, required): name of the player to report about.");
            tw.WriteLine("PrintNuts (bool, optional, default: false): prints nuts to console.");
        }
        
        public void Configure(Props pm)
        {
            _hero = pm.Get("HeroName");
            Name = "Nuts play";
            string printNuts = pm.Get("PrintNuts");
            if (!string.IsNullOrEmpty(printNuts))
            {
                _printNuts = bool.Parse(printNuts);
            }
        }

        public string Name
        {
            get;
            set;
        }

        public void Print(TextWriter tw)
        {
            tw.WriteLine("{0}", Name);

            tw.WriteLine("Pos        Games      Result   Nuts  Folds       Lost");

            Record totalRecord = new Record();

            for (int pos = 0; pos < _positions.Count; pos++)
            {
                Record r = _positions[pos];

                PrintRecord(tw, pos.ToString(), r);

                totalRecord.TotalGames += r.TotalGames;
                totalRecord.TotalResult += r.TotalResult;
                totalRecord.TotalNuts += r.TotalNuts;
                totalRecord.FoldedNuts += r.FoldedNuts;
                totalRecord.LostInFoldedNuts += r.LostInFoldedNuts;
            }

            PrintRecord(tw, "ALL", totalRecord);
        }

        private void PrintRecord(TextWriter tw, string name, Record r)
        {
            tw.WriteLine("{0,-3} {1,12:#,#} {2,11:0.00} {3,6} {4,6}  {5,9:0.00}",
                name, r.TotalGames, r.TotalResult, r.TotalNuts, r.FoldedNuts, r.LostInFoldedNuts);
        }

        public void Update(GameRecord gameRecord)
        {
            int[] hand = new int[7];
            for (int pos = 0; pos < gameRecord.Players.Count; ++pos)
            {
                GameRecord.Player player = gameRecord.Players[pos];
                if (player.Name == _hero)
                {
                    Allocate(pos);
                    _positions[pos].TotalGames++;
                    _positions[pos].TotalResult += gameRecord.Players[pos].Result;
                    int c = 0;
                    Ak lastHeroAction = Ak.b;
                    foreach (PokerAction action in gameRecord.Actions)
                    {
                        if (action.Kind == Ak.d && (action.Position == pos || action.Position == -1))
                        {
                            int [] cards = StdDeck.Descriptor.GetIndexes(action.Cards); 
                            cards.CopyTo(hand, c);
                            c+= cards.Length;
                        }
                        if (action.IsPlayerAction() && action.Position == pos)
                        {
                            lastHeroAction = action.Kind;
                        }
                    }
                    if(c == 7)
                    {
                        if (!HeHelper.CanLose(hand))
                        {
                            if (_printNuts)
                            {
                                Console.WriteLine("{0}: {1}", pos, gameRecord.ToGameString());
                            }
                            _positions[pos].TotalNuts++;
                            if (lastHeroAction == Ak.f)
                            {
                                _positions[pos].FoldedNuts ++;
                                Debug.Assert(gameRecord.Players[pos].Result < 0);
                                _positions[pos].LostInFoldedNuts += Math.Abs(gameRecord.Players[pos].Result);
                            }
                        }
                    }

                    break;
                }
            }
        }

        #endregion

        void Allocate(int pos)
        {
            for (int p = _positions.Count; p < pos + 1; ++p)
            {
                _positions.Add(new Record());
            }
        }

        class Record
        {
            public int TotalGames;
            public double TotalResult;
            public int TotalNuts;
            public int FoldedNuts;
            public double LostInFoldedNuts;
        }

        private string _hero;
        bool _printNuts = false;
        private List<Record> _positions = new List<Record>(2);
    }
}
