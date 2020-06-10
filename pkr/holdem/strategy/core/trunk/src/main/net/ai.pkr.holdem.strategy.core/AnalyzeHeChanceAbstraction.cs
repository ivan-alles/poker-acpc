/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.algorithms;
using ai.pkr.stdpoker;
using ai.pkr.metastrategy;

namespace ai.pkr.holdem.strategy.core
{
    /// <summary>
    /// Shows some useful info about a HE chance abstraction:
    /// <para>- for each preflop abstract card the list of pockets converting to this card.</para>
    /// </summary>
    public static class AnalyzeHeChanceAbstraction
    {
        public static void PrintPreflopRanges(IChanceAbstraction ca)
        {
            List<HePocketKind>[] abstrRanges = new List<HePocketKind>[0];
            int[] abstrRangesSizes = new int[0];

            for (int p = 0; p < (int)HePocketKind.__Count; ++p)
            {
                HePocketKind kind = (HePocketKind)p;
                CardSet pocketCS = HePocket.KindToCardSet(kind);
                int [] pocketArr = StdDeck.Descriptor.GetIndexesAscending(pocketCS).ToArray();
                int abstrCard = ca.GetAbstractCard(pocketArr, pocketArr.Length);

                if (abstrCard >= abstrRanges.Length)
                {
                    Array.Resize(ref abstrRanges, abstrCard + 1);
                    Array.Resize(ref abstrRangesSizes, abstrCard + 1);
                }
                if (abstrRanges[abstrCard] == null)
                {
                    abstrRanges[abstrCard] = new List<HePocketKind>();
                }

                abstrRanges[abstrCard].Add(kind);
                abstrRangesSizes[abstrCard] += HePocket.KindToRange(kind).Length;
            }

            Console.WriteLine("Preflop ranges of CA: {0}", ca.Name);
            int total = 0;
            for (int i = abstrRanges.Length - 1; i >= 0; --i)
            {
                Console.Write("{0,2} ({1,4}):", i, abstrRangesSizes[i]);
                foreach (HePocketKind k in abstrRanges[i])
                {
                    Console.Write(" {0}", HePocket.KindToString(k));
                }
                Console.WriteLine();
                total += abstrRangesSizes[i];
            }
            Console.WriteLine("Total: {0}", total);
        }
    }
}
