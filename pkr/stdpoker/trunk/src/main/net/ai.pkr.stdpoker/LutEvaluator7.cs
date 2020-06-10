/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using System.Reflection;
using System.IO;

namespace ai.pkr.stdpoker
{
    /// <summary>
    /// 7-card LUT evaluator.
    /// </summary>
    public static unsafe class LutEvaluator7
    {
        /// <summary>
        /// Used internally (inter-assembly) to verify file format.
        /// </summary>
        public const UInt32 LutFileFormatID = 0;

        /// <summary>
        /// Path of the LUT file. Can be used by LUT generation.
        /// </summary>
        public static string LutPath
        {
            get
            {
                string assemblyDir = AssemblyHelper.GetShortName(Assembly.GetExecutingAssembly());
                string LutPath = Props.Global.Expand("${bds.DataDir}") + assemblyDir;
                LutPath = Path.Combine(LutPath, LutFileName);
                return LutPath;
            }
        }

        /// <summary>
        /// LUT version read from the LUT file.
        /// </summary>
        public static readonly BdsVersion Version;

        /// <summary>
        /// Evaluate cards indexes in an array.
        /// </summary>
        public static UInt32 Evaluate(int[] cards)
        {
            UInt32 value = pLut[cards[0]];
            value = pLut[value + cards[1]];
            value = pLut[value + cards[2]];
            value = pLut[value + cards[3]];
            value = pLut[value + cards[4]];
            value = pLut[value + cards[5]];
            return pLut[value + cards[6]];
        }

        /// <summary>
        /// Evaluate cards indexes.
        /// </summary>
        public static UInt32 Evaluate(int c0, int c1, int c2, int c3, int c4, int c5, int c6)
        {
            UInt32 value = pLut[c0];
            value = pLut[value + c1];
            value = pLut[value + c2];
            value = pLut[value + c3];
            value = pLut[value + c4];
            value = pLut[value + c5];
            return pLut[value + c6];
        }

        /// <summary>
        /// Pointer to the LUT.
        /// </summary>
        public static readonly UInt32 * pLut;

        #region Implementation

        const int CARDS_COUNT = 7;

        /// <summary>
        /// Used internally (inter-assembly) to save/load the LUT.
        /// </summary>
        static readonly string LutFileName = "LutEvaluator7.dat";

        static readonly SmartPtr _lutPtr;


        static LutEvaluator7()
        {
            string fileName = LutPath;
            Exception exception = null;

            try
            {
                using (FileStream file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader r = new BinaryReader(file))
                    {
                        BdsVersion version = new BdsVersion();
                        version.Read(r);
                        UInt32 formatId = r.ReadUInt32();
                        UInt32 lutSize = r.ReadUInt32();
                        UInt32 lutByteSize = lutSize * 4;
                        _lutPtr = UnmanagedMemory.AllocHGlobalExSmartPtr(lutByteSize);
                        pLut = (UInt32*)_lutPtr;
                        UnmanagedMemory.Read(r, _lutPtr, lutByteSize);
                    }
                }
            }
            catch(Exception e)
            {
                exception = e;
            }
            if (exception != null)
            {
                if (_lutPtr != null)
                {
                    _lutPtr.Dispose();
                    _lutPtr = null;
                }
                pLut = (UInt32*)IntPtr.Zero.ToPointer();

                // If IO error occured leave the class uninitialized. This is a normal case for table generation.
                // If the application tries to use an uninitialized class, an exception will be thrown somewhere.
                // Otherwise rethrow the exception.
                if(!(exception is IOException))
                {
                    throw exception;
                }
            }
        }

        #endregion
    }
}
