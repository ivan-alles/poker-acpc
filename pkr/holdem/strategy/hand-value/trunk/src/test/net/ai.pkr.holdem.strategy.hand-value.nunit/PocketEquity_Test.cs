/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.IO;
using ai.lib.algorithms;
using ai.pkr.metagame;

namespace ai.pkr.holdem.strategy.core.nunit
{
    /// <summary>
    /// Unit tests for PocketEquity. 
    /// Pokerstove was used to verify result.
    /// A list of all heads-up matchups: http://www.pokerstove.com/analysis/preflop-matchups.txt.gz.
    /// </summary>
    [TestFixture]
    public class PocketEquity_Test
    {
        #region Tests

        /// <summary>
        /// Do at least a short test.
        /// </summary>
        [Test]
        public void Test_Calculate_Range()
        {
            DeckDescriptor d = StdDeck.Descriptor;
            CardSet[] range1 = new CardSet[] { d.GetCardSet("Ac Ah"), d.GetCardSet("Kc Kd") };
            CardSet[] range2 = new CardSet[] { d.GetCardSet("7h 6h"), d.GetCardSet("5c 4d") };

            PocketEquity.Result r = PocketEquity.Calculate(range1, range2);
            Assert.AreEqual(0.79960, r.Equity, 0.000005);
            Assert.AreEqual(4, r.Count);
        }

        [Test]
        public void Test_Calculate_Pocket()
        {
            PocketEquity.Result r1;
            PocketEquity.Result r2;

            // Calculate for all possible types of suits and pairs.

            // Pocket vs same pocket
            r1 = PocketEquity.Calculate(HePocketKind._KK, HePocketKind._KK);
            Assert.AreEqual(6, r1.Count);
            Assert.AreEqual(0.5, r1.Equity, 0.0000005);

            // Suited vs same suited
            r1 = PocketEquity.Calculate(HePocketKind._KQs, HePocketKind._KQs);
            Assert.AreEqual(12, r1.Count);
            Assert.AreEqual(0.5, r1.Equity, 0.0000005);

            // Offsuit vs same offsuit
            r1 = PocketEquity.Calculate(HePocketKind._KQo, HePocketKind._KQo);
            Assert.AreEqual(12*(3+2*2), r1.Count);
            Assert.AreEqual(0.5, r1.Equity, 0.0000005);

            // Pocket vs offsuit not intersecting
            r1 = PocketEquity.Calculate(HePocketKind._AA, HePocketKind._72o);
            r2 = PocketEquity.Calculate(HePocketKind._72o, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 12, r1.Count);
            Assert.AreEqual(0.881996, r1.Equity, 0.0000005);

            // Pocket vs offsuit intersecting
            r1 = PocketEquity.Calculate(HePocketKind._AA, HePocketKind._AKo);
            r2 = PocketEquity.Calculate(HePocketKind._AKo, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 6, r1.Count);
            Assert.AreEqual(0.931719, r1.Equity, 0.0000005);

            // Pocket vs suited not intersecting
            r1 = PocketEquity.Calculate(HePocketKind._AA, HePocketKind._65s);
            r2 = PocketEquity.Calculate(HePocketKind._65s, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 4, r1.Count);
            Assert.AreEqual(0.775012, r1.Equity, 0.0000005);

            // Pocket vs suited intersecting
            r1 = PocketEquity.Calculate(HePocketKind._AA, HePocketKind._AKs);
            r2 = PocketEquity.Calculate(HePocketKind._AKs, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 2, r1.Count);
            Assert.AreEqual(0.878595, r1.Equity, 0.0000005);

            // Offsuit vs offsuit not intersecting
            r1 = PocketEquity.Calculate(HePocketKind._Q4o, HePocketKind._T7o);
            r2 = PocketEquity.Calculate(HePocketKind._T7o, HePocketKind._Q4o);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(12 * 12, r1.Count);
            Assert.AreEqual(0.573232, r1.Equity, 0.0000005);

            // Offsuit vs offsuit intersecting
            r1 = PocketEquity.Calculate(HePocketKind._QJo, HePocketKind._J4o);
            r2 = PocketEquity.Calculate(HePocketKind._J4o, HePocketKind._QJo);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(12 * 9, r1.Count);
            Assert.AreEqual(0.755700, r1.Equity, 0.0000005);

            // Suited vs suited not intersecting
            r1 = PocketEquity.Calculate(HePocketKind._75s, HePocketKind._32s);
            r2 = PocketEquity.Calculate(HePocketKind._32s, HePocketKind._75s);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 4, r1.Count);
            Assert.AreEqual(0.634678, r1.Equity, 0.0000005);

            // Suited vs suited intersecting
            r1 = PocketEquity.Calculate(HePocketKind._JTs, HePocketKind._T9s);
            r2 = PocketEquity.Calculate(HePocketKind._T9s, HePocketKind._JTs);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 3, r1.Count);
            Assert.AreEqual(0.701062, r1.Equity, 0.0000005);

            // Suited vs offsuit not intersecting
            r1 = PocketEquity.Calculate(HePocketKind._AKs, HePocketKind._65o);
            r2 = PocketEquity.Calculate(HePocketKind._65o, HePocketKind._AKs);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 12, r1.Count);
            Assert.AreEqual(0.639171, r1.Equity, 0.0000005);

            // Suited vs offsuit intersecting
            r1 = PocketEquity.Calculate(HePocketKind._JTs, HePocketKind._T9o);
            r2 = PocketEquity.Calculate(HePocketKind._T9o, HePocketKind._JTs);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 9, r1.Count);
            Assert.AreEqual(0.741168, r1.Equity, 0.0000005);

            // Suited vs same offsuit
            r1 = PocketEquity.Calculate(HePocketKind._98s, HePocketKind._98o);
            r2 = PocketEquity.Calculate(HePocketKind._98o, HePocketKind._98s);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 6, r1.Count);
            Assert.AreEqual(0.524921, r1.Equity, 0.0000005);
        }

        [Test]
        public void Test_CalculateFast()
        {
            PocketEquity.Result r1;
            PocketEquity.Result r2;

            // CalculateFast for all possible types of suits and pairs.

            // Pocket vs same pocket
            r1 = PocketEquity.CalculateFast(HePocketKind._KK, HePocketKind._KK);
            Assert.AreEqual(6, r1.Count);
            Assert.AreEqual(0.5, r1.Equity, 0.0000005);

            // Suited vs same suited
            r1 = PocketEquity.CalculateFast(HePocketKind._KQs, HePocketKind._KQs);
            Assert.AreEqual(12, r1.Count);
            Assert.AreEqual(0.5, r1.Equity, 0.0000005);

            // Offsuit vs same offsuit
            r1 = PocketEquity.CalculateFast(HePocketKind._KQo, HePocketKind._KQo);
            Assert.AreEqual(12 * (3 + 2 * 2), r1.Count);
            Assert.AreEqual(0.5, r1.Equity, 0.0000005);

            // Pocket vs offsuit not intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._AA, HePocketKind._72o);
            r2 = PocketEquity.CalculateFast(HePocketKind._72o, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 12, r1.Count);
            Assert.AreEqual(0.881996, r1.Equity, 0.0000005);

            // Pocket vs offsuit intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._AA, HePocketKind._AKo);
            r2 = PocketEquity.CalculateFast(HePocketKind._AKo, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 6, r1.Count);
            Assert.AreEqual(0.931719, r1.Equity, 0.0000005);

            // Pocket vs suited not intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._AA, HePocketKind._65s);
            r2 = PocketEquity.CalculateFast(HePocketKind._65s, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 4, r1.Count);
            Assert.AreEqual(0.775012, r1.Equity, 0.0000005);

            // Pocket vs suited intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._AA, HePocketKind._AKs);
            r2 = PocketEquity.CalculateFast(HePocketKind._AKs, HePocketKind._AA);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(6 * 2, r1.Count);
            Assert.AreEqual(0.878595, r1.Equity, 0.0000005);

            // Offsuit vs offsuit not intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._Q4o, HePocketKind._T7o);
            r2 = PocketEquity.CalculateFast(HePocketKind._T7o, HePocketKind._Q4o);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(12 * 12, r1.Count);
            Assert.AreEqual(0.573232, r1.Equity, 0.0000005);

            // Offsuit vs offsuit intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._QJo, HePocketKind._J4o);
            r2 = PocketEquity.CalculateFast(HePocketKind._J4o, HePocketKind._QJo);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(12 * 9, r1.Count);
            Assert.AreEqual(0.755700, r1.Equity, 0.0000005);

            // Suited vs suited not intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._75s, HePocketKind._32s);
            r2 = PocketEquity.CalculateFast(HePocketKind._32s, HePocketKind._75s);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 4, r1.Count);
            Assert.AreEqual(0.634678, r1.Equity, 0.0000005);

            // Suited vs suited intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._JTs, HePocketKind._T9s);
            r2 = PocketEquity.CalculateFast(HePocketKind._T9s, HePocketKind._JTs);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 3, r1.Count);
            Assert.AreEqual(0.701062, r1.Equity, 0.0000005);

            // Suited vs offsuit not intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._AKs, HePocketKind._65o);
            r2 = PocketEquity.CalculateFast(HePocketKind._65o, HePocketKind._AKs);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 12, r1.Count);
            Assert.AreEqual(0.639171, r1.Equity, 0.0000005);

            // Suited vs offsuit intersecting
            r1 = PocketEquity.CalculateFast(HePocketKind._JTs, HePocketKind._T9o);
            r2 = PocketEquity.CalculateFast(HePocketKind._T9o, HePocketKind._JTs);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 9, r1.Count);
            Assert.AreEqual(0.741168, r1.Equity, 0.0000005);

            // Suited vs same offsuit
            r1 = PocketEquity.CalculateFast(HePocketKind._98s, HePocketKind._98o);
            r2 = PocketEquity.CalculateFast(HePocketKind._98o, HePocketKind._98s);
            Assert.AreEqual(1, r1.Equity + r2.Equity, 0.0000005);
            Assert.AreEqual(r1.Count, r2.Count);
            Assert.AreEqual(4 * 6, r1.Count);
            Assert.AreEqual(0.524921, r1.Equity, 0.0000005);
        }

        
        [Test]
        public void Test_CalculateFast_VerifyAll()
        {
            double totalEquity = 0;
            uint totalCount = 0;
            int matchupsCount = 0;
            for (int pk1 = 0; pk1 < HePocket.Count; pk1++)
            {
                for (int pk2 = 0; pk2 < HePocket.Count; pk2++)
                {
                    PocketEquity.Result r = PocketEquity.CalculateFast((HePocketKind)pk1, (HePocketKind)pk2);
                    totalEquity += r.Equity;
                    totalCount += r.Count;
                    matchupsCount++;
                }
            }
            int expTotalCount = (int) (EnumAlgos.CountCombin(52, 2)*EnumAlgos.CountCombin(50, 2));
            Assert.AreEqual(totalCount, expTotalCount);
            Assert.AreEqual((double)169*169/2, totalEquity, 0.00001);
        }

        [Test]
        public void Test_CalculateFast_Array()
        {
            HePocketKind[] pockets1 = new HePocketKind[] { HePocketKind._55, HePocketKind._66 };
            HePocketKind[] pockets2 = new HePocketKind[] { HePocketKind._76s, HePocketKind._76o };

            PocketEquity.Result r = PocketEquity.CalculateFast(pockets1, pockets2);
            Assert.AreEqual(0.56058, r.Equity, 0.000005);
            Assert.AreEqual(246571776L / EnumAlgos.CountCombin(48,5), r.Count);

            pockets1 = new HePocketKind[] { HePocketKind._AKs, HePocketKind._AQs };
            pockets2 = new HePocketKind[] { HePocketKind._88, HePocketKind._77 };

            r = PocketEquity.CalculateFast(pockets1, pockets2);
            Assert.AreEqual(0.47681, r.Equity, 0.000005);
            Assert.AreEqual(164381184L / EnumAlgos.CountCombin(48, 5), r.Count);
        }


        [Test]
        [Category("Explicit")]
        public void Test_Precalculate()
        {
            PocketEquity.Precalculate();
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Calculate()
        {
            uint totalCount = 0;
            int callsCount = 0;

            DateTime startTime = DateTime.Now;


            // Calculate for all possible types of suits and pairs.

            totalCount += PocketEquity.Calculate(HePocketKind._KK, HePocketKind._KK).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._KQs, HePocketKind._KQs).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._KQo, HePocketKind._KQo).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._AA, HePocketKind._72o).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._AA, HePocketKind._AKo).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._AA, HePocketKind._65s).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._AA, HePocketKind._AKs).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._Q4o, HePocketKind._T7o).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._QJo, HePocketKind._J4o).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._75s, HePocketKind._32s).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._JTs, HePocketKind._T9s).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._AKs, HePocketKind._65o).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._JTs, HePocketKind._T9o).Count;
            callsCount++;
            totalCount += PocketEquity.Calculate(HePocketKind._98s, HePocketKind._98o).Count;
            callsCount++;

            double runTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Calls: {0}, total matchups: {1}, time: {2:0.0} s, {3:0.0} calls/s, {4:0.0} matchups/s",
                              callsCount, totalCount, runTime, callsCount / runTime, totalCount / runTime);

        }
        #endregion

        #region Implementation
        #endregion
    }
}
