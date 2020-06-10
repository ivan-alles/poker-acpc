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
using System.Diagnostics;
using System.Globalization;

namespace ai.pkr.luck
{
    /// <summary>
    /// Caluclates HE HS deviation from expected.
    /// </summary>
    public class HeHsDeviationReport : IGameLogReport
    {

        #region IGameLogReport Members

        public void ShowHelp(TextWriter tw)
        {
            tw.WriteLine("Caluclates deviation of HE HS from expected value for each round. Positive is lucky, negative: unlucky.");
            tw.WriteLine("Parameters:");
            tw.WriteLine("HeroName (string, optional, default: ''): hero name.");
            tw.WriteLine("PrintHeroChartData (bool, optional, default: false): prints chart data for the hero");
        }

        public void Configure(Props pm)
        {
            _playerData = new Dictionary<string, HeHsDeviation>();
            _heroName = pm.GetDefault("HeroName", "");
            _printHeroChartData = bool.Parse(pm.GetDefault("PrintHeroChartData", "false"));
            _heroResult = 0;
            if (_printHeroChartData)
            {
                _heroChartData = new List<ChartData>();
            }
        }

        public string Name
        {
            get;
            set;
        }

        public void Print(TextWriter tw)
        {
            foreach (KeyValuePair<string, HeHsDeviation> kvp in _playerData)
            {
                tw.WriteLine("Player: {0}", kvp.Key);
                for (int r = 0; r < 4; ++r)
                {
                    HeHsDeviation dev = kvp.Value;
                    tw.WriteLine("Round {0}: hand count {1,10:#,#}, acc HS dev: {2,10:0.0000}, av HS dev: {3,12:0.0000000}", r,
                        dev.HandCount[r], dev.AccDeviation[r], dev.AvDeviation[r]);
                }
                tw.WriteLine();
            }
            if (_printHeroChartData)
            {
                Console.WriteLine("Chart data for hero: '{0}'", _heroName);
                Console.WriteLine("Result AccDev0 AccDev1 AccDev2 AccDev3");
                foreach (ChartData cd in _heroChartData)
                {
                    Console.WriteLine("{0} {1} {2} {3} {4}", cd.Result, cd.AccDeviation[0], cd.AccDeviation[1], cd.AccDeviation[2], cd.AccDeviation[3]);
                }
            }
        }

        public void Update(GameRecord gameRecord)
        {
            GameState gs = new GameState(gameRecord, null);
            foreach(PlayerState ps in gs.Players)
            {
                if (ps.Hand == null)
                {
                    continue;
                }
                string[] handS = ps.Hand.Split(_cardSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (handS.Length >= 2 && handS[0] != "?" && handS[1] != "?")
                {
                    HeHsDeviation dev;
                    if (!_playerData.TryGetValue(ps.Name, out dev))
                    {
                        dev = new HeHsDeviation();
                        _playerData.Add(ps.Name, dev);
                    }
                    int[] hand = StdDeck.Descriptor.GetIndexes(ps.Hand);
                    dev.ProcessHand(hand);
                    if(ps.Name == _heroName)
                    {
                        _heroResult += ps.Result;
                        if (_printHeroChartData)
                        {
                            ChartData cd = new ChartData();
                            cd.Result = _heroResult;
                            cd.AccDeviation = dev.AccDeviation.ShallowCopy();
                            _heroChartData.Add(cd);
                        }
                    }
                }
            }
        }

        #endregion

        struct ChartData
        {
            public double Result;
            public double[] AccDeviation;
        }

        Dictionary<string, HeHsDeviation> _playerData;
        string _heroName;
        bool _printHeroChartData;
        List<ChartData> _heroChartData;
        double _heroResult;
        static readonly char[] _cardSeparators = new char[] { ' ' };
    }
}
