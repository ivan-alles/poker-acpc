/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.lib.utils;
using ai.pkr.metastrategy;
using ai.pkr.metagame;

namespace ai.pkr.holdem.strategy.core.hecainfo
{
    static class Program
    {
        static CommandLine _cmdLine = new CommandLine();

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

            foreach (string propsFile in _cmdLine.PropsFiles)
            {
                Console.WriteLine("Input file: {0}", propsFile);
                IChanceAbstraction ca = ChanceAbstractionHelper.CreateFromPropsFile(propsFile);
                if (_cmdLine.PreflopRanges)
                {
                    AnalyzeHeChanceAbstraction.PrintPreflopRanges(ca);
                }
                ShowHands(ca);
                Console.WriteLine();
            }

            return 0;
        }

        private static void ShowHands(IChanceAbstraction ca)
        {
            if (_cmdLine.Hands == null)
            {
                return;
            }

            foreach(string handS in _cmdLine.Hands)
            {
                handS.Trim();
                if (handS.Length % 2 != 0)
                {
                    Console.WriteLine("Wrong HE hand: {0}", handS);
                    continue;
                }
                string cards = "";
                for (int i = 0; i < handS.Length; i += 2)
                {
                    cards += handS.Substring(i, 2) + " ";
                }
                int[] hand = null;
                try
                {
                    hand = StdDeck.Descriptor.GetIndexes(cards);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Wrong HE hand: {0}, {1}", cards, e.ToString());
                    continue;
                } 
                int round = HeHelper.HandSizeToRound[hand.Length];
                if (round == -1)
                {
                    Console.WriteLine("Wrong HE hand size: {0}", handS);
                    continue;
                }
                Console.Write("Abstract cards for hand {0}:", StdDeck.Descriptor.GetCardNames(hand));
                for (int r = 0; r <= round; ++r)
                {
                    int abstrCard = ca.GetAbstractCard(hand, HeHelper.RoundToHandSize[r]);
                    Console.Write(" {0}", abstrCard);
                }
                Console.WriteLine();
            }
        }
    }
}
