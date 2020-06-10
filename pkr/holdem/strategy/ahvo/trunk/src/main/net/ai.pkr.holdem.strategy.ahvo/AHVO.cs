/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.stdpoker;
using ai.lib.algorithms;
using ai.lib.utils;
using System.Reflection;
using System.IO;
using ai.pkr.holdem.strategy.core;

namespace ai.pkr.holdem.strategy.ahvo
{
    /// <summary>
    /// Calculates average hand value ordinal for a given board.
    /// </summary>
    public static class AHVO
    {
        #region Public API

        public static float Calculate(int[] board)
        {
            return Calculate(board, 0, board.Length);
        }

        public static float Calculate(int[] board, int length)
        {
            return Calculate(board, 0, length);
        }

        public static float Calculate(int[] board, int start, int length)
        {
            CardSet cs = StdDeck.Descriptor.GetCardSet(board, start, length);
            return Calculate(cs);
        }

        public static float Calculate(CardSet board)
        {
            CalulateParam param = new CalulateParam();
            int boardSize = board.CountCards();
            int toDeal = 7 - boardSize;
            CardEnum.Combin(StdDeck.Descriptor, toDeal, board, CardSet.Empty, OnDeal, param);
            return (float)(param.Sum/EnumAlgos.CountCombin(52 - boardSize, toDeal));
        }

        public static float CalculateFast(int[] board)
        {
            return CalculateFast(board, 0, board.Length);
        }

        public static float CalculateFast(int[] board, int length)
        {
            return CalculateFast(board, 0, length);
        }

        public static float CalculateFast(int[] board, int start, int length)
        {
            CardSet cs = StdDeck.Descriptor.GetCardSet(board, start, length);
            return CalculateFast(cs);
        }

        public static float CalculateFast(CardSet board)
        {
            NormSuit ns = new NormSuit();
            Entry searchEntry = new Entry();
            searchEntry.CardSet = ns.Convert(board).bits;
            int round = HeHelper.HandSizeToRound[board.CountCards() + 2];
            Entry [] lut = _luts[round - 1];
            int idx = Array.BinarySearch(lut, searchEntry);
            if (idx < 0)
            {
                throw new ApplicationException(string.Format("Cannot find LUT entry for board: '{0}'", StdDeck.Descriptor.GetCardNames(board)));
            }
            return lut[idx].Ahvo;
        }

        public static void Precalculate(int round)
        {
            if(round < 1 || round > 3)
            {
                throw new ApplicationException(string.Format("Wrong round: {0}", round));
            }
            Console.WriteLine("Generating LUT for round {0}...", round);

            string lutPath = GetLutPath(round);
            if (File.Exists(lutPath))
            {
                Console.WriteLine("LUT {0} already exists, won't overwrite. Delete to create a new LUT.", lutPath);
                return;
            }
            PrecalulateParam p = new PrecalulateParam();
            int boardSize = HeHelper.RoundToHandSize[round] - 2;
            CardEnum.Combin(StdDeck.Descriptor, boardSize, CardSet.Empty, CardSet.Empty, OnPrecalculateBoard, p);
            Entry[] lut = p.Entries.ToArray();
            WriteLut(lut, lutPath);
            Console.WriteLine("LUT {0} created.", lutPath);
        }

        #endregion 

        #region Implementation

        static AHVO()
        {
            for (int r = 1; r <= 3; ++r)
            {
                try
                {
                    _luts[r - 1] = ReadLut(GetLutPath(r));
                }
                catch (IOException)
                {
                    // Do nothing. This is a normal case for LUT generation, 
                    // and for normal usage a null-pointer exception will be thrown later.
                }
            }
        }

        class CalulateParam
        {
            public double Sum;
        }

        static void OnDeal(ref CardSet hand, CalulateParam p)
        {
            p.Sum += HandValueToOrdinal.GetOrdinal7(CardSetEvaluator.Evaluate(ref hand));
        }

        struct Entry: IComparable<Entry>
        {
            public UInt64 CardSet;
            public float Ahvo;

            #region IComparable<Entry> Members

            public int CompareTo(Entry other)
            {
                return CardSet.CompareTo(other.CardSet);
            }

            #endregion
        }


        class PrecalulateParam
        {
            public List<Entry> Entries = new List<Entry>();
            public NormSuit NormSuit = new NormSuit();
        }

        static void OnPrecalculateBoard(ref CardSet board, PrecalulateParam p)
        {
            CardSet lastBoard = new CardSet();
            if(p.Entries.Count > 0)
            {
                lastBoard.bits = p.Entries[p.Entries.Count - 1].CardSet;
            }
            p.NormSuit.Reset();
            CardSet nBoard = p.NormSuit.Convert(board);
            if(nBoard.bits > lastBoard.bits)
            {
                Entry newEntry;
                newEntry.CardSet = nBoard.bits;
                newEntry.Ahvo = Calculate(nBoard);
                p.Entries.Add(newEntry);
            }
        }
        static readonly int SER_FMT_VER = 1;

        static void WriteLut(Entry[] lut, string path)
        {
            BdsVersion fileVersion = new BdsVersion();
            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());
            fileVersion.Major = assemblyVersion.Major;
            fileVersion.Minor = assemblyVersion.Minor;
            fileVersion.Revision = assemblyVersion.Revision;
            fileVersion.Build = assemblyVersion.Build;
            fileVersion.ScmInfo = assemblyVersion.ScmInfo;

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (BinaryWriter wr = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                DataSerializationHelper.WriteHeader(wr, fileVersion, SER_FMT_VER);
                wr.Write(lut.Length);
                for (int i = 0; i < lut.Length; ++i)
                {
                    wr.Write(lut[i].CardSet);
                    wr.Write(lut[i].Ahvo);
                }
            }
        }

        static Entry[] ReadLut(string path)
        {
            Entry[] lut = null;
            using (BinaryReader r = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                BdsVersion fileVersion;
                int serFmtVersion;
                DataSerializationHelper.ReadHeader(r, out fileVersion, out serFmtVersion, SER_FMT_VER);
                int count = r.ReadInt32();
                lut = new Entry[count];
                for (int i = 0; i < count; ++i)
                {
                    lut[i].CardSet = r.ReadUInt64();
                    lut[i].Ahvo = r.ReadSingle();
                }
            }
            return lut;
        }

        private static string GetLutPath(int round)
        {
            string dataDir = BdsDirHelper.DataDir(Assembly.GetExecutingAssembly());
            return Path.Combine(dataDir, string.Format("AHVO-{0}.dat", round));
        }

        static readonly Entry[][] _luts = new Entry[3][];

        #endregion
    }
}
