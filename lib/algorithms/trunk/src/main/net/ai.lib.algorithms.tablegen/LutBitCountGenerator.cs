/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.utils;

namespace ai.lib.algorithms.tablegen
{
    class LutBitCountGenerator: TableGenerator
    {
        static readonly UInt32 _elementCount = 1 << 16;

        internal LutBitCountGenerator()
        {
            Namespace = "ai.lib.algorithms.lut";
            Comment = "// Number of bits in a word (0x0000 - 0xFFFF).";
            Name = "LutBitCount";
            Type = "byte";
            UseUnmanagedMemory = true;
        }

        static UInt32 GetBitCount(UInt32 value)
        {
            UInt32 count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        protected override void PrintContent(TextWriter wr)
        {
            // As this is a big table, use compact formatting
            for(UInt32 i = 0; i < _elementCount; ++i)
            {
                if(i % 64 == 0)
                {
                    if(i != 0) wr.WriteLine();
                }
                wr.Write("{0,2}, ", GetBitCount(i));
            }
            wr.WriteLine();
        }
    }
}