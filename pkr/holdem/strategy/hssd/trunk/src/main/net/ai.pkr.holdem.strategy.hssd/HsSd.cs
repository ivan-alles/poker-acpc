/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.hs;
using ai.pkr.metastrategy.algorithms;
using ai.lib.algorithms.numbers;
using System.Diagnostics;
using ai.pkr.holdem.strategy.core;
using ai.lib.utils;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using ai.lib.algorithms;

namespace ai.pkr.holdem.strategy.hssd
{
    public static class HsSd
    {
        #region Public API

        public static float[] Calculate(int[] hand, int handLength, int sdRound)
        {
            VerifyParameters(handLength, sdRound);
            float[] hssd = new float[2];
            hssd[0] = HandStrength.CalculateFast(hand, handLength);   

            if(handLength == 7)
            {
                // Nothing more to do on the river.
                return hssd;
            }

            int sdHandLength = HeHelper.RoundToHandSize[sdRound];

            CardSet cs = StdDeck.Descriptor.GetCardSet(hand, 0, handLength);
            Params p = new Params { Hand = new int[sdHandLength]};
            for (int i = 0; i < handLength; ++i)
            {
                p.Hand[i] = hand[i];
            }

            p.Hs = hssd[0];

            CardEnum.Combin<Params>(StdDeck.Descriptor, sdHandLength - handLength, p.Hand, handLength, p.Hand, handLength, OnDeal, p);
            Debug.Assert(FloatingPoint.AreEqual(p.Hs, p.SumHs / p.Count, 0.00001));
            hssd[1] = (float)Math.Sqrt(p.SumDiff / p.Count);
            return hssd;
        }

        public static float[] Calculate(int[] hand, int sdRound)
        {
            return Calculate(hand, hand.Length, sdRound);
        }

        public static float[] CalculateFast(int[] hand, SdKind sdKind)
        {
            return CalculateFast(hand, hand.Length, sdKind);
        }

        public enum SdKind
        {
            SdPlus1,
            Sd3
        };

        public static float[] CalculateFast(int[] hand, int handLength, SdKind sdKind)
        {
            Debug.Assert(handLength >= 0 && handLength <= 7);
            if (handLength == 7)
            {
                // SdKind.SdPlus1 will throw an exception, this is exactly what we want.
                return Calculate(hand, handLength, sdKind == SdKind.SdPlus1 ? 4 : 3);
            }

            if (_lut2 == null)
            {
                LoadLuts();
            }

            float[] hssd = new float[2];

            int round = HeHelper.HandSizeToRound[handLength];
            CardSet pocket = StdDeck.Descriptor.GetCardSet(hand, 0, 2);
            CardSet board = StdDeck.Descriptor.GetCardSet(hand, 2, handLength - 2);
            NormSuit se = new NormSuit();
            CardSet sePocket = se.Convert(pocket);
            CardSet seBoard = se.Convert(board);

            if(round == 2)
            {

                Entry2 keyEntry = new Entry2(HePocket.CardSetToKind(sePocket), seBoard);
                int idx = Array.BinarySearch(_lut2, keyEntry);
                if (idx < 0)
                {
                    ThrowNoEntryException(sePocket, seBoard);
                }
                hssd[0] = _lut2[idx].Hs;
                // For turn, there is no difference between SD kinds.
                hssd[1] = _lut2[idx].SdPlus1;
            }
            else
            {
                Entry01 keyEntry = new Entry01(HePocket.CardSetToKind(sePocket), seBoard);
                int idx = Array.BinarySearch(_lut01[round], keyEntry);
                if (idx < 0)
                {
                    ThrowNoEntryException(sePocket, seBoard);
                }
                // For turn, there is no difference between SD kinds.
                hssd[0] = _lut01[round][idx].Hs;
                hssd[1] = sdKind == SdKind.SdPlus1 ? _lut01[round][idx].SdPlus1 : _lut01[round][idx].Sd3;
            }
            return hssd;
        }

        /// <summary>
        /// Precalculate and store tables. If the output already exists, will not overwrite.
        /// <remarks>Long-running. </remarks>
        /// </summary>
        /// <param name="round">Round (0, 1 or 2).</param>
        public static void Precalculate(int round)
        {
            DateTime startTime = DateTime.Now;

            string lutPath = GetLutPath(round);
            if(File.Exists(lutPath))
            {
                // Do not ovewriting an existing file to save time.
                Console.WriteLine("LUT file {0} already exist, exiting. Delete the file to recalculate.", lutPath);
                return;
            }

            int POCKETS_COUNT = (int)HePocketKind.__Count;
            //POCKETS_COUNT = 1; // Test

            PrecalculationContext context = new PrecalculationContext { Round = round };
            int[] listSize = new int[] { 169, 1361802, 15111642 };
            context.list = round < 2 ? (object)new List<Entry01>(listSize[round]) : (object)new List<Entry2>(listSize[round]);

            Console.WriteLine("Calculating for round {0}: ", round);

            int boardSize = HeHelper.RoundToHandSize[round] - 2;

            for (int p = 0; p < POCKETS_COUNT; ++p)
            {
                context.pocketKind = (HePocketKind)p;
                context.pocket = HePocket.KindToCardSet((HePocketKind)p);
                context.pocketSei.Reset();
                context.pocketSei.Convert(context.pocket);
                Console.Write("{0} ", HePocket.KindToString((HePocketKind)p));
                CardEnum.Combin(StdDeck.Descriptor, boardSize, CardSet.Empty, context.pocket, OnPrecalculateBoard, context);
            }
            Console.WriteLine();
            Debug.Assert(EnumAlgos.CountCombin(50, boardSize) * POCKETS_COUNT == context.count);
            if (round < 2)
            {
                WriteTable((List<Entry01>)context.list, lutPath);
            }
            else
            {
                WriteTable((List<Entry2>)context.list, lutPath);
            }
            Console.WriteLine("LUT file {0} written, calculated in {1:0.0} s", lutPath, (DateTime.Now - startTime).TotalSeconds);
        }

        #endregion

        #region Implementation

        #region Calculation
        class Params
        {
            public int[] Hand;
            public int Count;
            public double Hs;
            public double SumHs;
            public double SumDiff;
        }

        private static void VerifyParameters(int handLength, int sdRound)
        {
            if (handLength < 2 || handLength > 7)
            {
                throw new ArgumentOutOfRangeException("Wrong hand length.");
            }
            if (sdRound < 1 || sdRound > 3)
            {
                throw new ArgumentOutOfRangeException("Wrong round.");
            }
            if (HeHelper.HandSizeToRound[handLength] > sdRound)
            {
                throw new ArgumentException("Hand round must be <= SD round.");
            }
        }

        static void OnDeal(int [] hand, Params p)
        {
            double hs = HandStrength.CalculateFast(p.Hand, p.Hand.Length);
            double d = hs - p.Hs;
            p.SumDiff += d * d;
            p.SumHs += hs;
            p.Count++;
        }
        #endregion
        #region Precalculation

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

        interface IPersistent
        {
            void Write(BinaryWriter w);
            void Read(BinaryReader r);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Entry2 : IComparable<Entry2>, IPersistent
        {
            public UInt32 Key;
            public float Hs;
            public float SdPlus1;

            public Entry2(HePocketKind pocketKind, CardSet board)
            {
                Key = GetKey((int)pocketKind, board);
                Hs = SdPlus1 = 0;
            }

            public int CompareTo(Entry2 other)
            {
                return Key.CompareTo(other.Key);
            }

            public void  Write(BinaryWriter w)
            {
                w.Write(Key);
                w.Write(Hs);
                w.Write(SdPlus1);
            }

            public void  Read(BinaryReader r)
            {
                Key = r.ReadUInt32();
                Hs = r.ReadSingle();
                SdPlus1 = r.ReadSingle();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Entry01 : IComparable<Entry01>, IPersistent
        {
            public UInt32 Key;
            public float Hs;
            public float SdPlus1;
            public float Sd3;

            public Entry01(HePocketKind pocketKind, CardSet board)
            {
                Key = GetKey((int)pocketKind, board);
                Hs = Sd3 = SdPlus1 = 0;
            }

            public int CompareTo(Entry01 other)
            {
                return Key.CompareTo(other.Key);
            }

            public void Write(BinaryWriter w)
            {
                w.Write(Key);
                w.Write(Hs);
                w.Write(SdPlus1);
                w.Write(Sd3);
            }

            public void Read(BinaryReader r)
            {
                Key = r.ReadUInt32();
                Hs = r.ReadSingle();
                SdPlus1 = r.ReadSingle();
                Sd3 = r.ReadSingle();
            }
        }

        class PrecalculationContext
        {
            public int Round;
            public HePocketKind pocketKind;
            public CardSet pocket;
            public int count = 0;
            public NormSuit pocketSei = new NormSuit();
            public object list;
        }

        static readonly string[] _lutNames = {
                                                  "hssd-lut.0.dat",
                                                  "hssd-lut.1.dat",
                                                  "hssd-lut.2.dat"
                                              };

        // Version of precalculated files, should be the same as the version
        // of the pom.
        private static readonly BdsVersion Version = new BdsVersion(1, 0, 0);

        /// <summary>
        /// Look-up tables for each round from 0 to 1.
        /// </summary>
        private static Entry01[][] _lut01 = new Entry01[2][];

        /// <summary>
        /// Look-up table for round 2.
        /// </summary>
        private static Entry2[] _lut2;

        /// <summary>
        /// Load precalcuation table for fast calculation.
        /// If not called by user, will be called automatically as soon as 
        /// it is necessary (which may slow-down the very first calculation).
        /// </summary>
        public static void LoadLuts()
        {
            for (int r = 0; r < 2; ++r)
            {
                _lut01[r] = ReadTable<Entry01>(GetLutPath(r));
            }
            _lut2 = ReadTable<Entry2>(GetLutPath(2));
        }

        private static void ThrowNoEntryException(CardSet sePocket, CardSet seBoard)
        {
            throw new ApplicationException(String.Format(
                                               "No entry in lookup table for pocket '{0}' board '{1}'",
                                               sePocket, seBoard));
        }

        static void WriteTable<T>(List<T> table, string path) where T: IPersistent
        {
            BdsVersion fileVersion = new BdsVersion(Version);
            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());
            fileVersion.ScmInfo = assemblyVersion.ScmInfo;

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using(BinaryWriter wr = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                fileVersion.Write(wr);
                wr.Write(table.Count);
                for(int i = 0; i < table.Count; ++i)
                {
                    table[i].Write(wr);
                }
            }
        }

        static T[] ReadTable<T>(string path) where T: IPersistent, new()
        {
            T[] table = null;
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
                table = new T[count];

                for (int i = 0; i < count; ++i)
                {
                    table[i].Read(r);
                }
            }
            return table;
        }


        static void OnPrecalculateBoard(ref CardSet board, PrecalculationContext d)
        {
            NormSuit sei = new NormSuit(d.pocketSei);
            CardSet seBoard = sei.Convert(board);
            List<Entry01> list01 = null;
            List<Entry2> list2 = null;
            UInt32 key = GetKey((int)d.pocketKind, seBoard);
            bool addNew = false;
            // A key will be either present in the table or go to the end (greater than the rest).
            // This is due to the order of dealt boards in CardEnum
            if (d.Round < 2)
            {
                list01 = (List<Entry01>) d.list;
                addNew = list01.Count == 0 || list01[list01.Count -1].Key < key;
            }
            else
            {
                list2 = (List<Entry2>)d.list;
                addNew = list2.Count == 0 || list2[list2.Count - 1].Key < key;
            }

            if (addNew)
            {
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
                float[] sdhs;
                if (d.Round < 2)
                {
                    Entry01 newEntry = new Entry01 {Key = key};
                    sdhs = Calculate(hand, d.Round + 1);
                    newEntry.Hs = sdhs[0];
                    newEntry.SdPlus1 = sdhs[1];
                    sdhs = Calculate(hand, 3);
                    newEntry.Sd3 = sdhs[1];
                    list01.Add(newEntry);
                }
                else
                {
                    Entry2 newEntry = new Entry2 { Key = key };
                    sdhs = Calculate(hand, d.Round + 1);
                    newEntry.Hs = sdhs[0];
                    newEntry.SdPlus1 = sdhs[1];
                    list2.Add(newEntry);                    
                }
            }
            d.count++;
        }

        private static string GetLutPath(int round)
        {
            string dataDir = BdsDirHelper.DataDir(Assembly.GetExecutingAssembly());
            return Path.Combine(dataDir, _lutNames[round]);
        }

        #endregion
        #endregion
    }
}
