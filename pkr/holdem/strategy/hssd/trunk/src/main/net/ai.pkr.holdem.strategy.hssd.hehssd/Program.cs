/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.core.hecainfo;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.holdem.strategy.hssd;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.core;

namespace ai.pkr.holdem.strategy.hehssd
{
    class Program
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
            foreach (string handS in _cmdLine.Hands)
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
                Console.Write("HS/SD3/SD+1 of {0}:", StdDeck.Descriptor.GetCardNames(hand));
                for (int r = 0; r <= round; ++r)
                {
                    float [] result;
                    Console.Write(" round {0}: ", r);
                    result = HsSd.CalculateFast(hand, HeHelper.RoundToHandSize[r], HsSd.SdKind.Sd3);
                    Console.Write(" {0:0.000000}", result[0]);
                    Console.Write(" {0:0.000000}", result[1]);
                    if (r < 3)
                    {
                        result = HsSd.CalculateFast(hand, HeHelper.RoundToHandSize[r], HsSd.SdKind.SdPlus1);
                        Console.Write(" {0:0.000000}", result[1]);
                    }
                }
                Console.WriteLine();
            }

            return 0;
        }
    }
}
