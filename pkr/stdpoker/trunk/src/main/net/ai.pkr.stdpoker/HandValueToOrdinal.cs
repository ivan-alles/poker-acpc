/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.utils;
using System.Reflection;

namespace ai.pkr.stdpoker
{
    /// <summary>
    /// Converts a hand value returned by an evaluator to an integer in range [0 .. Nd-1], where Nd is the number of
    /// distinct hand values for a given hand size.
    /// </summary>
    public static class HandValueToOrdinal
    {
        #region Public API

        public static int GetOrdinal7(uint handValue7)
        {
            int ordinal = Array.BinarySearch(_lut7, handValue7);
            if (ordinal < 0)
            {
                throw new ApplicationException(string.Format("Hand value 7 is not found: {0:X08}", handValue7));
            }
            return ordinal;
        }

        /// <summary>
        /// Precalculate and store LUT for 7-card hands. Should not be called by applications.
        /// </summary>
        /// <param name="handSize">size of the hand (7-cards hands are suppoted now)</param>
        public static void Precalculate(int handSize)
        {
            DateTime startTime = DateTime.Now;

            if (handSize != 7)
            {
                throw new ApplicationException(String.Format("Hand size {0} is not supported", handSize));
            }

            string lutPath = GetLutPath(handSize);
            if (File.Exists(lutPath))
            {
                // Do not ovewriting an existing file to save time.
                Console.WriteLine("LUT file {0} already exist, exiting. Delete the file to recalculate.", lutPath);
                return;
            }

            uint[] lut = CaluculateLut7();
            WriteTable(lut, lutPath);
            Console.WriteLine("LUT file {0} written, calculated in {1:0.0} s", lutPath, (DateTime.Now - startTime).TotalSeconds);
        }

        #endregion

        #region Implementation

        static HandValueToOrdinal()
        {
            try
            {
                _lut7 = ReadTable(GetLutPath(7));
            }
            catch (IOException )
            {
                // Do noting, this is a normal case for table generator,
                // and for the regular usage it will throw a null-pointer exception in the 1st lut usage.
            }
        }

        private static string GetLutPath(int handSize)
        {
            return Path.Combine(BdsDirHelper.DataDir(Assembly.GetExecutingAssembly()), string.Format("HandValueToOrdinal-{0}.dat", handSize));
        }

        private unsafe static UInt32[] CaluculateLut7()
        {
            HashSet<UInt32> distinct = new HashSet<uint>();
            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                UInt32 v1 = LutEvaluator7.pLut[c1];
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    UInt32 v2 = LutEvaluator7.pLut[v1 + c2];
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        UInt32 v3 = LutEvaluator7.pLut[v2 + c3];
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            UInt32 v4 = LutEvaluator7.pLut[v3 + c4];
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                UInt32 v5 = LutEvaluator7.pLut[v4 + c5] + (uint)c5 - 1;
                                for (int c6 = c5 - 1; c6 >= 1; --c6, --v5)
                                {
                                    UInt32 v6 = LutEvaluator7.pLut[v5] + (uint)c6 - 1;
                                    for (int c7 = c6 - 1; c7 >= 0; --c7, --v6)
                                    {
                                        UInt32 rank = LutEvaluator7.pLut[v6];
                                        distinct.Add(rank);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            UInt32 [] lut = new uint[distinct.Count];
            int i = 0;
            foreach(UInt32 value in distinct)
            {
                lut[i++] = value;
            }
            Array.Sort(lut);
            return lut;
        }

        static void WriteTable(UInt32[] table, string path)
        {
            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());
            BdsVersion fileVersion = new BdsVersion();
            fileVersion.Major = assemblyVersion.Major;
            fileVersion.Minor = assemblyVersion.Minor;
            fileVersion.Revision = assemblyVersion.Revision;
            fileVersion.ScmInfo = assemblyVersion.ScmInfo;

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (BinaryWriter wr = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                fileVersion.Write(wr);
                wr.Write(table.Length);
                for (int i = 0; i < table.Length; ++i)
                {
                    wr.Write(table[i]);
                }
            }
        }

        static UInt32[] ReadTable(string path) 
        {
            BdsVersion assemblyVersion = new BdsVersion(Assembly.GetExecutingAssembly());

            UInt32 [] table = null;
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
                table = new UInt32[count];

                for (int i = 0; i < count; ++i)
                {
                    table[i] = r.ReadUInt32();
                }
            }
            return table;
        }

        static readonly uint[] _lut7;

        #endregion
    }
}
