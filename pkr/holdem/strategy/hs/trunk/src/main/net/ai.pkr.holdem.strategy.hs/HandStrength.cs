/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.pkr.holdem;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using ai.pkr.metastrategy;
using System.Reflection;
using ai.lib.utils;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.holdem.strategy.core;
using System.Runtime.InteropServices;

namespace ai.pkr.holdem.strategy.hs
{
    /// <summary>
    /// Calcutates hands strenghts for a 2-players game and 
    /// any round from 0 to 3 given a pocket and a board.
    /// To determine the hand strength, all possible missing board cards are dealt, than all possible opponent pockets are 
    /// dealt and a showdown is done. HS = (wins * 2 + ties) / (2 * (wins + ties + loses)), 
    /// it is a real number [0..1].
    /// </summary>
    public unsafe static class HandStrength
    {
        static readonly string[] _lutNames  = {
                                                  "hs-lut.preflop.dat",
                                                  "hs-lut.flop.dat",
                                                  "hs-lut.turn.dat"
                                              };

        static readonly string DATA_SUBDIR = "ai.pkr.holdem.strategy.hs-lut";

        // Version of precalculated files, should be the same as the version
        // of hs-lut pom.
        private static readonly BdsVersion Version = new BdsVersion(1, 1, 1);

        /// <summary>
        /// Look-up tables for each round from 0 to 2, round 3 is too large.
        /// </summary>
        private static Entry[][] _luts = new Entry[3][];   

        /// <summary>
        /// Load precalcuation table for fast calculation.
        /// If not called by user, will be called automatically as soon as 
        /// it is necessary (which may slow-down the very first calculation).
        /// </summary>
        public static void LoadPrecalculationTables()
        {
            string dataDir = Props.Global.Get("bds.DataDir");
            dataDir = Path.Combine(dataDir, DATA_SUBDIR);
            for (int r = 0; r < 3; ++r)
            {
                _luts[r] = ReadTable(Path.Combine(dataDir, _lutNames[r]));
            }
        }

        public static float Calculate(int[] hand)
        {
            return Calculate(hand, hand.Length);
        }

        public static float Calculate(int[] hand, int handLength)
        {
            Debug.Assert(handLength >= 2 && handLength <= 7);
            int[] deckCopy = StdDeck.Descriptor.FullDeckIndexes.ShallowCopy();

            for (int i = 0; i < handLength; ++i)
            {
                deckCopy[hand[i]] = -1;
            }

            int[] restDeck = RemoveDeadCards(deckCopy, handLength);
            int boardSize = handLength - 2;
            UInt32 result = 0;
            UInt32 boardStart = 0;
            for (int i = 0; i < boardSize; ++i)
            {
                boardStart = LutEvaluator7.pLut[boardStart + hand[2 + i]];
            }
            DealBoard(hand, restDeck, deckCopy, boardStart, 0, restDeck.Length + boardSize - 4, ref result);
            UInt32 count = (UInt32)EnumAlgos.CountCombin(52 - handLength, 5 - boardSize) * (UInt32)EnumAlgos.CountCombin(45, 2);
            return (float)result / count / 2;
        }

        public static float CalculateFast(int[] hand)
        {
            return CalculateFast(hand, hand.Length);
        }

        public static float CalculateFast(int[] hand, int handLength)
        {
            Debug.Assert(handLength >= 0 && handLength <= 7);
            if (handLength == 7)
                return Calculate(hand, handLength);

            CardSet pocket = StdDeck.Descriptor.GetCardSet(hand, 0, 2);
            CardSet board = StdDeck.Descriptor.GetCardSet(hand, 2, handLength - 2);

            return CalculateFast(pocket, board, handLength);
        }

        public static float CalculateFast(CardSet pocket, CardSet board)
        {
            int boardSize = board.CountCards();
            if(boardSize == 5)
            {
                int[] hand = new int[7];
                List<int> pocketL = StdDeck.Descriptor.GetIndexesAscending(pocket);
                List<int> boardL = StdDeck.Descriptor.GetIndexesAscending(board);
                pocketL.CopyTo(hand, 0);
                boardL.CopyTo(hand, 2);
                return Calculate(hand, 7);
            }
            return CalculateFast(pocket, board, boardSize + 2);
        }

        private static float CalculateFast(CardSet pocket, CardSet board, int handLength)
        {
            if (_luts[1] == null)
            {
                LoadPrecalculationTables();
            }

            int round = HeHelper.HandSizeToRound[handLength];
            Entry[] lookup = _luts[round];

            NormSuit se = new NormSuit();
            CardSet sePocket = se.Convert(pocket);
            CardSet seBoard = se.Convert(board);
            Entry keyEntry = new Entry(HePocket.CardSetToKind(sePocket), seBoard);
            int idx = Array.BinarySearch<Entry>(lookup, keyEntry);
            if (idx < 0)
            {
                throw new ApplicationException(String.Format("No entry in lookup table for pocket {{{0}}} board {{{1}}}",
                                                             sePocket, seBoard));
            }
            return lookup[idx].value;
        }

        /// <summary>
        /// Calculates min and max values of hand strenght for a given round.
        /// </summary>
        public static void CalculateBounds(int round, out float min, out float max)
        {
            if(round == 3)
            {
                // No lut for turn, but we know that:
                // There are hands that beat everything on the turn (royal flush) => max == 1
                // Minimum is very close to 0, example 
                // Pocket 3d 2c on board 9c 8s 7c 5d 4h has strength 0,0045
                min = 0.0f;
                max = 1.0f;
                return;
            }
            if (_luts[1] == null)
            {
                LoadPrecalculationTables();
            }

            Entry[] lookup = _luts[round];
            min = float.MaxValue;
            max = float.MinValue;
            for(int i = 0; i < lookup.Length; ++i)
            {
                min = Math.Min(min, lookup[i].value);
                max = Math.Max(max, lookup[i].value);
            }
        }


        /// <summary>
        /// Returns an array with dead card removed. Dead cards must be marked by -1.
        /// </summary>
        static int[] RemoveDeadCards(int[] deck, int deadCount)
        {
            int[] result = new int[deck.Length - deadCount];
            int cnt = 0;
            for (int i = 0; i < deck.Length; ++i)
            {
                if (deck[i] >= 0)
                {
                    result[cnt++] = deck[i];
                }
            }
            Debug.Assert(cnt == result.Length);
            return result;
        }

        private static void DealBoard(int[] hand, int[] restDeck, int[] deckCopy, uint boardCur, int begin, int end, ref uint result)
        {
            if (end > restDeck.Length)
            {
                DealOppPocket(hand, restDeck, deckCopy, boardCur, ref result);
                return;
            }
            for (int i = begin; i < end; ++i)
            {
                int c = restDeck[i];
                deckCopy[c] = -1;
                UInt32 boardVal0 = LutEvaluator7.pLut[boardCur + c];
                DealBoard(hand, restDeck, deckCopy, boardVal0, i + 1, end + 1, ref result);
                deckCopy[c] = c;
            }
        }


        private static void DealOppPocket(int[] hand, int[] restDeck, int[] deckCopy, uint boardVal, ref uint result)
        {
            UInt32 heroRank = LutEvaluator7.pLut[boardVal + hand[0]];
            heroRank = LutEvaluator7.pLut[heroRank + hand[1]];

            // Remove dead cards for all rounds except turn.
            if (restDeck.Length > 52 - 7)
            {
                restDeck = RemoveDeadCards(deckCopy, 7); 
            }
            for (int i0 = 0; i0 < restDeck.Length - 1; ++i0)
            {
                int c0 = restDeck[i0];
                UInt32 oppRank0 = LutEvaluator7.pLut[boardVal + c0];
                for (int i1 = i0 + 1; i1 < restDeck.Length; ++i1)
                {
                    UInt32 oppRank = LutEvaluator7.pLut[oppRank0 + restDeck[i1]];
                    if (heroRank > oppRank)
                        result += 2;
                    else if (heroRank == oppRank)
                        result++;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry : IComparable<Entry>
        {
            [XmlAttribute("k")]
            public UInt32 key;
            [XmlAttribute("v")]
            public float value;

            public Entry(HePocketKind pocketKind, CardSet board)
            {
                key = GetKey((int)pocketKind, board);
                value = 0;
            }

            static UInt32 GetKey(int pocketKind, CardSet board)
            {
                UInt32 key = (UInt32)(pocketKind << 24);
                List<int> cards = StdDeck.Descriptor.GetIndexesAscending(board);
                Debug.Assert(cards.Count <= 4);
                for (int i = 0; i < cards.Count; ++i)
                {
                    key |= (UInt32)(cards[i] << (i * 6));
                }
                return key;
            }

            #region IComparable<Entry> Members

            public int CompareTo(Entry other)
            {
                return key.CompareTo(other.key);
            }

            #endregion
        }

        class PrecalculationContext
        {
            public HePocketKind pocketKind;
            public CardSet pocket;
            public int count = 0;
            public NormSuit pocketSei = new NormSuit();
            public List<Entry> list;
        }

        static void WriteTable(List<Entry> table, string path)
        {
            BdsVersion fileVersion = new BdsVersion(Version);
            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());
            fileVersion.ScmInfo = assemblyVersion.ScmInfo;

            using(BinaryWriter wr = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                fileVersion.Write(wr);
                wr.Write(table.Count);
                for(int i = 0; i < table.Count; ++i)
                {
                    wr.Write(table[i].key);
                    wr.Write(table[i].value);
                }
            }
        }

        static Entry[] ReadTable(string path)
        {
            Entry[] table = null;
            using (BinaryReader r = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                BdsVersion fileVersion = new BdsVersion();
                fileVersion.Read(r);
                if (Version.Major != fileVersion.Major ||
                    Version.Minor != fileVersion.Minor ||
                    Version.Revision != fileVersion.Revision)
                {
                    throw new ApplicationException(
                        String.Format("Wrong file version: expected: {0:x8}, was: {1:x8}, file: {2}",
                                      Version, fileVersion, path));
                }
                int count = r.ReadInt32();
                table = new Entry[count];

                for (int i = 0; i < count; ++i)
                {
                    table[i].key = r.ReadUInt32();
                    table[i].value = r.ReadSingle();
                }
            }
            return table;
        }

        /// <summary>
        /// Precalculate tables. 
        /// <remarks>Long-running (~10 hours).
        /// </remarks>
        /// </summary>
        /// <param name="outputDir">Output directory. A subdir named after the library will be created in it,
        /// and files will be stored there.</param>
        /// <param name="round">Calculate for one round only (0, 1 or 2). Specify -1 to calculate for all.</param>
        public static void PrecalcuateTables(string outputDir, int round)
        {
            List<Entry> table;
            int[] boardSizes = {0, 3, 4, 5};
            int firstRound = round == -1 ? 0 : round;
            int lastRound = round == -1 ?  2 : round;

            for (int r = firstRound; r <= lastRound; ++r)
            {
                table = Precalculate(boardSizes[r]);
                WriteTable(table, Path.Combine(outputDir, _lutNames[r]));
            }
        }

        static List<Entry> Precalculate(int boardSize)
        {
            int POCKETS_COUNT = (int)HePocketKind.__Count;
            //POCKETS_COUNT = 1; // Test

            PrecalculationContext context = new PrecalculationContext();
            int[] listSize = new int[] {169, -1, -1, 1361802, 15111642};
            context.list = new List<Entry>(listSize[boardSize]);

            for (int p = 0; p < POCKETS_COUNT; ++p)
            {
                context.pocketKind = (HePocketKind)p;
                context.pocket = HePocket.KindToCardSet((HePocketKind)p);
                context.pocketSei.Reset();
                context.pocketSei.Convert(context.pocket);
                Console.WriteLine("Calculating for board size {0}, pocket {1}", boardSize, context.pocket);
                CardEnum.Combin(StdDeck.Descriptor, boardSize, CardSet.Empty, context.pocket, OnPrecalculateBoard, context);
            }

            Debug.Assert(EnumAlgos.CountCombin(50, boardSize) * POCKETS_COUNT == context.count);
            return context.list;
        }

        static void OnPrecalculateBoard(ref CardSet board, PrecalculationContext d)
        {
            NormSuit sei = new NormSuit(d.pocketSei);
            CardSet seBoard = sei.Convert(board);
            Entry keyEntry = new Entry(d.pocketKind, seBoard);
            // Actually there is no BinarySearch necessary, because a key will be either present in the table
            // or go to the end (greater than the rest).
            // It's left here just to verify the algorithm.
            int idx = d.list.BinarySearch(keyEntry);
            if (idx < 0)
            {
                if(d.list.Count > 0 && d.list[d.list.Count - 1].key >= keyEntry.key)
                {
                    throw new ApplicationException(
                        "Algorithm error, new value must be greater than all existing values.");
                }
                List<int> pocketIdxs = StdDeck.Descriptor.GetIndexesAscending(d.pocket);
                List<int> boardIdxs = StdDeck.Descriptor.GetIndexesAscending(seBoard);
                int[] hand = new int[pocketIdxs.Count + boardIdxs.Count];
                int h = 0;
                for (int i = 0; i < pocketIdxs.Count; ++i)
                {
                    hand[h++] = pocketIdxs[i];
                }
                for (int i = 0; i < boardIdxs.Count; ++i)
                {
                    hand[h++] = boardIdxs[i];
                }
                keyEntry.value = Calculate(hand);
                d.list.Add(keyEntry);
            }
            d.count++;
        }
    }
}