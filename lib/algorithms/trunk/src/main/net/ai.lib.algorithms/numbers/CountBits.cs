/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.lut;

namespace ai.lib.algorithms.numbers
{
    /// <summary>
    /// Count bits in an integer.
    /// <remarks>
    /// It is difficult to make a high-performance bit counting class that will 
    /// work fast in all use cases. Experiments show that in C# the performance can vary
    /// significantly depending on many factors, such as if a function defined locally or in 
    /// another assembly or if you store locally a pointer locally to a look-up table defined 
    /// in another class.
    /// 
    /// At the moment this class uses a look-up table for 16 bit integers. Larger integers are split into 
    /// parts. This approach seems to be ok in most cases. The look-up table itself is also public
    /// and can be used to speed-up calculations (see ai.lib.algorithms.lut.LutBitCount).
    /// 
    /// So far the tests on both 32- and 64-bit architectures showed that the LUT approact is superior 
    /// to other approaches like MIT Hakmem 169.
    /// 
    /// It can be that this big LUT will decrease performance because of CPU cach issues. Then you
    /// can try other available algoritms such as MIT Hakmem 169. For an overview, see 
    /// http://gurmeetsingh.wordpress.com/2008/08/05/fast-bit-counting-routines/
    /// 
    /// </remarks>
    /// </summary>
    public unsafe static class CountBits
    {

        public static int Count(byte value)
        {
            return LutBitCount.T[value];
        }

        public static int Count(sbyte value)
        {
            return LutBitCount.T[(byte)value];
        }

        public static int Count(UInt16 value)
        {
            return LutBitCount.T[value];
        }

        public static int Count(Int16 value)
        {
            return LutBitCount.T[(UInt16)value];
        }

        public static int Count(UInt32 value)
        {
            return LutBitCount.T[value & 0xFFFF] + LutBitCount.T[value >> 16];
        }

        public static int Count(Int32 value)
        {
            return LutBitCount.T[(UInt32)value & 0xFFFF] + LutBitCount.T[(UInt32)value >> 16];
        }

        public static int Count(UInt64 value)
        {
            UInt32 half = (UInt32)value;
            int count = LutBitCount.T[half & 0xFFFF] + LutBitCount.T[half >> 16];
            half = (UInt32)(value >> 32);
            count += LutBitCount.T[half & 0xFFFF] + LutBitCount.T[half >> 16];
            return count;
        }

        public static int Count(Int64 value)
        {
            UInt32 half = (UInt32)value;
            int count = LutBitCount.T[half & 0xFFFF] + LutBitCount.T[half >> 16];
            half = (UInt32)(value >> 32);
            count += LutBitCount.T[half & 0xFFFF] + LutBitCount.T[half >> 16];
            return count;
        }

    }
}
