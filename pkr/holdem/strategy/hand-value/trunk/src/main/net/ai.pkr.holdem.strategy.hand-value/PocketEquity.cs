/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using ai.lib.algorithms;
using System.Diagnostics;
using ai.pkr.metastrategy.algorithms;
using ai.lib.utils;
using System.IO;
using System.Reflection;

namespace ai.pkr.holdem.strategy.core
{
    public static unsafe class PocketEquity
    {

        #region Public API

        public struct Result
        {
            /// <summary>
            /// Number of possible matchups pocket vs pocket. For example, AA vs AA: 1, AKs vs 67s: 16.
            /// </summary>
            public uint Count;

            /// <summary>
            /// Average equity from the point of view of the 1st hand.
            /// </summary>
            public float Equity;
        }

        public static Result Calculate(CardSet[] csRange1, CardSet[] csRange2)
        {
            // Make sure all cardsets are correct pockets.
            foreach (CardSet cs in csRange1)
            {
                if (cs.CountCards() != 2)
                {
                    throw new ApplicationException(string.Format("Wrong pocket {0}", StdDeck.Descriptor.GetCardNames(cs)));
                }
            }
            foreach (CardSet cs in csRange2)
            {
                if (cs.CountCards() != 2)
                {
                    throw new ApplicationException(string.Format("Wrong pocket {0}", StdDeck.Descriptor.GetCardNames(cs)));
                }
            }
            return CalculateNoVerify(csRange1, csRange2);
        }

        public static Result Calculate(HePocketKind p1, HePocketKind p2)
        {
            CardSet[] csRange1 = HePocket.KindToRange(p1);
            CardSet[] csRange2 = HePocket.KindToRange(p2);
            return CalculateNoVerify(csRange1, csRange2);
        }

        public static Result CalculateFast(HePocketKind p1, HePocketKind p2)
        {
            if (_lut == null)
            {
                LoadLut();
            }
            Precalculated key = new Precalculated();
            bool reverse = p1 > p2;
            if (reverse)
            {
                key.PocketKind1 = (byte)p2;
                key.PocketKind2 = (byte)p1;
            }
            else
            {
                key.PocketKind1 = (byte)p1;
                key.PocketKind2 = (byte)p2;
            }
            int i = Array.BinarySearch(_lut, key, _precalculatedComparer);
            Result r = new Result { Count = _lut[i].Count };
            if (reverse)
            {
                r.Equity = 1 - _lut[i].Equity;
            }
            else
            {
                r.Equity = _lut[i].Equity;
            }
            return r;
        }

        public static Result CalculateFast(HePocketKind[] pockets1, HePocketKind [] pockets2)
        {
            double totalEquity = 0;
            uint totalCount = 0;
            Result r;
            foreach(HePocketKind pk1 in pockets1)
            {
                foreach(HePocketKind pk2 in pockets2)
                {
                    r = CalculateFast(pk1, pk2);
                    totalEquity += (double)r.Equity*r.Count;
                    totalCount += r.Count;
                }
            }
            r = new Result {Equity = (float) (totalEquity/totalCount), Count = totalCount};
            return r;
        }

        public static void Precalculate()
        {

            DateTime startTime = DateTime.Now;

            string lutPath = GetLutPath();
            if(File.Exists(lutPath))
            {
                // Do not ovewriting an existing file to save time.
                Console.WriteLine("LUT file {0} already exist, exiting. Delete the file to recalculate.", lutPath);
                return;
            }

            Precalculated[] precalcArray = new Precalculated[HePocket.Count*(HePocket.Count+1)/2];
            int i = 0;
            Console.WriteLine("Calculating for");
            for (int pk1 = 0; pk1 < HePocket.Count; pk1++)
            {
                Console.Write(" {0}", (HePocketKind)pk1);
                for (int pk2 = pk1; pk2 < HePocket.Count; pk2++)
                {
                    Result r = Calculate((HePocketKind)pk1, (HePocketKind)pk2);
                    precalcArray[i].PocketKind1 = (byte)pk1;
                    precalcArray[i].PocketKind2 = (byte)pk2;
                    precalcArray[i].Count = (byte)r.Count;
                    precalcArray[i].Equity = r.Equity;
                    ++i;
                }
            }
            Console.WriteLine();

            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());
            BdsVersion fileVersion = new BdsVersion();
            fileVersion.Major = assemblyVersion.Major;
            fileVersion.Minor = assemblyVersion.Minor;
            fileVersion.Revision = assemblyVersion.Revision;
            fileVersion.ScmInfo = assemblyVersion.ScmInfo;
            fileVersion.Description = "HE heads-up pocket equity LUT.";

            string dir = Path.GetDirectoryName(lutPath);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (BinaryWriter wr = new BinaryWriter(File.Open(lutPath, FileMode.Create)))
            {
                fileVersion.Write(wr);
                wr.Write(precalcArray.Length);
                for (i = 0; i < precalcArray.Length; ++i)
                {
                    precalcArray[i].Write(wr);
                }
            }
            Console.WriteLine("Calculated in {0} s", (DateTime.Now - startTime).TotalSeconds);
        }

        #endregion

        #region Implementation

        struct Precalculated
        {
            public byte PocketKind1;
            public byte PocketKind2;
            public byte Count;
            public float Equity;

            public void Read(BinaryReader r)
            {
                PocketKind1 = r.ReadByte();
                PocketKind2 = r.ReadByte();
                Count = r.ReadByte();
                Equity = r.ReadSingle();
            }

            public void Write(BinaryWriter w)
            {
                w.Write(PocketKind1);
                w.Write(PocketKind2);
                w.Write(Count);
                w.Write(Equity);
            }
        }

        class PrecalculatedIComparer: IComparer<Precalculated>
        {
            public int Compare(Precalculated x, Precalculated y)
            {
                if(x.PocketKind1 < y.PocketKind1)
                {
                    return -1;
                }
                if(x.PocketKind1 > y.PocketKind1)
                {
                    return 1;
                }
                if (x.PocketKind2 < y.PocketKind2)
                {
                    return -1;
                }
                if (x.PocketKind2 > y.PocketKind2)
                {
                    return 1;
                }
                return 0;
            }
        }

        private static string GetLutPath()
        {
            string dataDir = Props.Global.Get("bds.DataDir");
            dataDir = Path.Combine(dataDir, DATA_SUBDIR);
            return Path.Combine(dataDir, _lutName);
        }

        /// <summary>
        /// Load precalcuation table for fast calculation.
        /// If not called by user, will be called automatically as soon as 
        /// it is necessary (which may slow-down the very first calculation).
        /// </summary>
        public static void LoadLut()
        {
            string path = GetLutPath();

            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());

            using (BinaryReader r = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                BdsVersion fileVersion = new BdsVersion();
                fileVersion.Read(r);
                if (assemblyVersion.Major != fileVersion.Major ||
                    assemblyVersion.Minor != fileVersion.Minor ||
                    assemblyVersion.Revision != fileVersion.Revision)
                {
                    throw new ApplicationException(
                        String.Format("Wrong file version: expected: {0:x8}, was: {1:x8}, file: {2}",
                                      assemblyVersion, fileVersion, path));
                }
                int count = r.ReadInt32();
                _lut = new Precalculated[count];

                for (int i = 0; i < count; ++i)
                {
                    _lut[i].Read(r);
                }
            }
        }

        public static Result CalculateNoVerify(CardSet[] csRange1, CardSet[] csRange2)
        {
            UInt64 totalValue = 0;
            uint count = 0;
            NormSuit ns = new NormSuit();
            Dictionary<CardSet, UInt32> knonwValues = new Dictionary<CardSet, UInt32>();
            foreach (CardSet cs1 in csRange1)
            {
                foreach (CardSet cs2 in csRange2)
                {
                    if (cs1.IsIntersectingWith(cs2))
                    {
                        continue;
                    }
                    CardSet union = ns.Convert(cs1);
                    union.UnionWith(ns.Convert(cs2));
                    ns.Reset();
                    Debug.Assert(union.CountCards() == 4);
                    UInt32 knownValue;
                    if(!knonwValues.TryGetValue(union, out knownValue))
                    {
                        int[] h1 = StdDeck.Descriptor.GetIndexesAscending(cs1).ToArray();
                        int[] h2 = StdDeck.Descriptor.GetIndexesAscending(cs2).ToArray();
                        knownValue = CalculateMatchupValue(h1, h2);
                        knonwValues.Add(union, knownValue);
                    }
                    totalValue += knownValue;
                    count++;
                }
            }
            Result r = new Result();
            r.Equity = (float) totalValue/count/_boardsCount/2;
            r.Count = count;
            return r;
        }

        /// <summary>
        /// Calculate a value h1 vs h2. Every win of h1 adds 2, every tie: 1, otherwise 0.
        /// </summary>
        static UInt32 CalculateMatchupValue(int[] h1, int[] h2)
        {
            int[] deckCopy = StdDeck.Descriptor.FullDeckIndexes.ShallowCopy();
            for (int i = 0; i < 2; ++i)
            {
                deckCopy[h1[i]] = -1;
                deckCopy[h2[i]] = -1;
            }
            int[] restDeck = RemoveDeadCards(deckCopy, 4);

            UInt32 result = 0;
            UInt32 * pLut = LutEvaluator7.pLut;
            UInt32 lut10 = pLut[pLut[h1[0]] + h1[1]];
            UInt32 lut20 = pLut[pLut[h2[0]] + h2[1]];

            for (int c1 = 0; c1 < restDeck.Length - 4; c1++)
            {
                UInt32 lut11 = pLut[lut10 + restDeck[c1]];
                UInt32 lut21 = pLut[lut20 + restDeck[c1]];
                for (int c2 = c1+1; c2 < restDeck.Length - 3; c2++)
                {
                    UInt32 lut12 = pLut[lut11 + restDeck[c2]];
                    UInt32 lut22 = pLut[lut21 + restDeck[c2]];
                    for (int c3 = c2+1; c3 < restDeck.Length - 2; c3++)
                    {
                        UInt32 lut13 = pLut[lut12 + restDeck[c3]];
                        UInt32 lut23 = pLut[lut22 + restDeck[c3]];
                        for (int c4 = c3+1; c4 < restDeck.Length - 1; c4++)
                        {
                            UInt32 lut14 = pLut[lut13 + restDeck[c4]];
                            UInt32 lut24 = pLut[lut23 + restDeck[c4]];
                            for (int c5 = c4+1; c5 < restDeck.Length; c5++)
                            {
                                UInt32 lut15 = pLut[lut14 + restDeck[c5]];
                                UInt32 lut25 = pLut[lut24 + restDeck[c5]];
                                if (lut15 > lut25)
                                {
                                    result+=2;
                                }
                                else if (lut15 == lut25)
                                {
                                    result++;
                                }
                            }
                        }
                    }
                }
            }
            return result;
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

        static readonly int _boardsCount = (int) EnumAlgos.CountCombin(48, 5);

        static readonly PrecalculatedIComparer _precalculatedComparer = new PrecalculatedIComparer();

        static readonly string _lutName = "hu-pocket-equity.dat";

        // ai.ver.major - update version if necessary
        static readonly string DATA_SUBDIR = "ai.pkr.holdem.strategy.hand-value";

        private static Precalculated[] _lut;

        #endregion

    }

}
