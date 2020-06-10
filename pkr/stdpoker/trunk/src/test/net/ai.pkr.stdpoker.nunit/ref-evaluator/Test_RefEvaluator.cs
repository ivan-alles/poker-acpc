/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ai.pkr.stdpoker.nunit.ref_evaluator
{
    [TestFixture] 
    public class Test_RefEvaluator
    {
        /// <summary>
        /// This function evaluates all possible 5 card poker hands and tallies the
        /// results. The results should come up with know values. If not there is either
        /// and error in the iterator function Hands() or the EvaluateType() function.
        /// </summary>
        [Test]
        public void Test5CardHands()
        {
            int[] handtypes = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int count = 0;

            // Iterate through all possible 5 card hands
            foreach (ulong mask in RefEvaluator.Hands(5))
            {
                handtypes[(int)RefEvaluator.EvaluateType(mask, 5)]++;
                count++;
            }

            // Validate results.
            Assert.AreEqual(1302540, handtypes[(int)RefEvaluator.HandTypes.HighCard], "HighCard Returned Incorrect Count");
            Assert.AreEqual(1098240, handtypes[(int)RefEvaluator.HandTypes.Pair], "Pair Returned Incorrect Count");
            Assert.AreEqual(123552, handtypes[(int)RefEvaluator.HandTypes.TwoPair], "TwoPair Returned Incorrect Count");
            Assert.AreEqual(54912, handtypes[(int)RefEvaluator.HandTypes.Trips], "Trips Returned Incorrect Count");
            Assert.AreEqual(10200, handtypes[(int)RefEvaluator.HandTypes.Straight], "Trips Returned Incorrect Count");
            Assert.AreEqual(5108, handtypes[(int)RefEvaluator.HandTypes.Flush], "Flush Returned Incorrect Count");
            Assert.AreEqual(3744, handtypes[(int)RefEvaluator.HandTypes.FullHouse], "FullHouse Returned Incorrect Count");
            Assert.AreEqual(624, handtypes[(int)RefEvaluator.HandTypes.FourOfAKind], "FourOfAKind Returned Incorrect Count");
            Assert.AreEqual(40, handtypes[(int)RefEvaluator.HandTypes.StraightFlush], "StraightFlush Returned Incorrect Count");
            Assert.AreEqual(2598960, count, "Count Returned Incorrect Value");
        }

        /// <summary>
        /// This function evaluates all possible 7 card poker hands and tallies the
        /// results. The results should come up with know values. If not there is either
        /// and error in the iterator function Hands() or the EvaluateType() function.
        /// </summary>
        [Test]
        public void Test7CardHands()
        {
            int[] handtypes = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int count = 0;

            // Iterate through all possible 7 card hands
            foreach (ulong mask in RefEvaluator.Hands(7))
            {
                handtypes[(int)RefEvaluator.EvaluateType(mask, 7)]++;
                count++;
            }

            Assert.AreEqual(58627800, handtypes[(int)RefEvaluator.HandTypes.Pair], "Pair Returned Incorrect Value");
            Assert.AreEqual(31433400, handtypes[(int)RefEvaluator.HandTypes.TwoPair], "TwoPair Returned Incorrect Value");
            Assert.AreEqual(6461620, handtypes[(int)RefEvaluator.HandTypes.Trips], "Trips Returned Incorrect Value");
            Assert.AreEqual(6180020, handtypes[(int)RefEvaluator.HandTypes.Straight], "Straight Returned Incorrect Value");
            Assert.AreEqual(4047644, handtypes[(int)RefEvaluator.HandTypes.Flush], "Flush Returned Incorrect Value");
            Assert.AreEqual(3473184, handtypes[(int)RefEvaluator.HandTypes.FullHouse], "FullHouse Returned Incorrect Value");
            Assert.AreEqual(224848, handtypes[(int)RefEvaluator.HandTypes.FourOfAKind], "FourOfAKind Returned Incorrect Value");
            Assert.AreEqual(41584, handtypes[(int)RefEvaluator.HandTypes.StraightFlush], "StraightFlush Returned Incorrect Value");
            Assert.AreEqual(133784560, count, "Count Returned Incorrect Value");

        }
        /// <summary>
        /// Tests the Parser and the ToString for masks.
        /// </summary>
        [Test]
        public void TestParserWith5Cards()
        {
            int count = 0;

            for (int i = 0; i < 52; i++)
            {
                Assert.AreEqual(RefEvaluator.ParseCard(RefEvaluator.CardTable[i]), i, "Make sure parser and text match");
            }

            foreach (ulong mask in RefEvaluator.Hands(5))
            {
                string hand = RefEvaluator.MaskToString(mask);
                ulong testmask = RefEvaluator.ParseHand(hand);
                Assert.AreEqual(RefEvaluator.BitCount(testmask), 5, "Parsed Results should be 5 cards");
 
                //System.Diagnostics.Debug.Assert(mask == testmask);

                Assert.AreEqual(mask, testmask, "Make sure that MaskToString() and ParseHand() return consistant results");
                count++;
            }
        }

        /// <summary>
        /// Tests the Parser and the ToString for masks.
        /// </summary>
        [Test]
        public void TestParserWith7Cards()
        {
            foreach (ulong mask in RefEvaluator.RandomHands(7, 20.0))
            {
                string hand = RefEvaluator.MaskToString(mask);
                ulong testmask = RefEvaluator.ParseHand(hand);
                Assert.AreEqual(RefEvaluator.BitCount(testmask), 7, "Parsed Results should be 7 cards");
                Assert.AreEqual(mask, testmask, "Make sure that MaskToString() and ParseHand() return consistant results");
            }
        }


        /// <summary>
        /// C# Interop call to Win32 QueryPerformanceCount. This function should be removed
        /// if you need an interop free class definition.
        /// </summary>
        /// <param name="lpPerformanceCount">returns performance counter</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        /// C# Interop call to Win32 QueryPerformanceFrequency. This function should be removed
        /// if you need an interop free class definition.
        /// </summary>
        /// <param name="lpFrequency">returns performance frequence</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestRandomIterators()
        {
            long start, freq, curtime;
            int count = 0;
            // Test Random Hand Trials Iteration
            foreach (ulong mask in RefEvaluator.RandomHands(7, 20000))
            {
                count++;
            }
            Assert.AreEqual(count, 20000, "Should match the requested number of hands");

            QueryPerformanceFrequency(out freq);
            QueryPerformanceCounter(out start);

            count = 0;
            foreach (ulong mask in RefEvaluator.RandomHands(7, 2.5))
            {
                count++;
            }
            QueryPerformanceCounter(out curtime);

            Assert.Greater(((curtime - start) / ((double)freq)), 2.5, "Make sure ran the correct amount of time");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestSuitConsistancy()
        {
            ulong mask = RefEvaluator.ParseHand("Ac Tc 2c 3c 4c");
            uint sc = (uint)((mask >> (RefEvaluator.CLUB_OFFSET)) & 0x1fffUL);
            Assert.AreEqual(RefEvaluator.BitCount(sc), 5, "Club consistancy check");

            mask = RefEvaluator.ParseHand("Ad Td 2d 3d 4d");
            uint sd = (uint)((mask >> (RefEvaluator.DIAMOND_OFFSET)) & 0x1fffUL);
            Assert.AreEqual(RefEvaluator.BitCount(sd), 5, "Diamond consistancy check");

            mask = RefEvaluator.ParseHand("Ah Th 2h 3h 4h");
            uint sh = (uint)((mask >> (RefEvaluator.HEART_OFFSET)) & 0x1fffUL);
            Assert.AreEqual(RefEvaluator.BitCount(sh), 5, "Hearts consistancy check");

            mask = RefEvaluator.ParseHand("As Ts 2s 3s 4s");
            uint ss = (uint)((mask >> (RefEvaluator.SPADE_OFFSET)) & 0x1fffUL);
            Assert.AreEqual(RefEvaluator.BitCount(ss), 5, "Spade consistancy check");
        }

      
       
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestBasicIterators()
        {
            long count = 0;
            foreach (ulong mask in RefEvaluator.Hands(1))
                count++;
            Assert.AreEqual(count, 52, "Check one card hand count");

            count = 0;
            foreach (ulong mask in RefEvaluator.Hands(2))
                count++;
            Assert.AreEqual(count, 1326, "Check two card hand count");

            count = 0;
            foreach (ulong mask in RefEvaluator.Hands(3))
                count++;
            Assert.AreEqual(count, 22100, "Check three card hand count");

            count = 0;
            foreach (ulong mask in RefEvaluator.Hands(4))
                count++;
            Assert.AreEqual(count, 270725, "Check four card hand count");

            count = 0;
            foreach (ulong mask in RefEvaluator.Hands(5))
                count++;
            Assert.AreEqual(count, 2598960, "Check five card hand count");

            count = 0;
            foreach (ulong mask in RefEvaluator.Hands(6))
                count++;
            Assert.AreEqual(count, 20358520, "Check six card hand count");
        }


        [Test]
        public void TestInstanceOperators()
        {
            foreach (ulong pocketmask in RefEvaluator.Hands(2))
            {
                string pocket = RefEvaluator.MaskToString(pocketmask);
                foreach (ulong boardmask in RefEvaluator.RandomHands(0UL, pocketmask, 5, 0.001))
                {
                    string board = RefEvaluator.MaskToString(boardmask);
                    RefEvaluator hand1 = new RefEvaluator(pocket, board);
                    RefEvaluator hand2 = new RefEvaluator();
                    hand2.PocketMask = pocketmask;
                    hand2.BoardMask = boardmask;
                    Assert.IsTrue(hand1 == hand2, "Equality test failed");
                }
            }
        }

        [Test]
        public void TestMoreInstanceOperators()
        {
            string board = "2d kh qh 3h qc";
            // Create a hand with AKs plus board
            RefEvaluator h1 = new RefEvaluator("ad kd", board);
            // Create a hand with 23 unsuited plus board
            RefEvaluator h2 = new RefEvaluator("2h 3d", board);
       
            Assert.IsTrue(h1 > h2);
            Assert.IsTrue(h1 >= h2);
            Assert.IsTrue(h2 <= h1);
            Assert.IsTrue(h2 < h1);
            Assert.IsTrue(h1 != h2);
        }
    }
}
