/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ai.lib.utils
{
    /// <summary>
    /// Various conversion from one type to another preserving physical representation.
    /// Is usually faster than the corresponding conversion of BitConverter.
    /// </summary>
    public unsafe static class BitwiseConvert
    {
        /// <summary>
        /// Converts from a double to an array of UInt32 (length at least 2).
        /// </summary>
        public static void ToUInt32Arr(double value, UInt32 [] result)
        {
            Debug.Assert(result.Length >= 2);
            fixed (UInt32* resultArray = result)
            {
                double* valueArray = (double*) resultArray;
                valueArray[0] = value;
            }
        }

        /// <summary>
        /// Converts from an UInt64 to an array of UInt32 (length at least 2).
        /// </summary>
        public static void ToUInt32Arr(UInt64 value, UInt32[] result)
        {
            Debug.Assert(result.Length >= 2);
            fixed (UInt32* resultArray = result)
            {
                UInt64* valueArray = (UInt64*)resultArray;
                valueArray[0] = value;
            }
        }

        /// <summary>
        /// Converts from an array of UInt32 (length at least 2) to double.
        /// </summary>
        public static double ToDouble(UInt32[] value)
        {
            Debug.Assert(value.Length >= 2);
            fixed (UInt32* valueArray = value)
            {
                double* resultArray = (double*)valueArray;
                return resultArray[0];
            }
        }

        /// <summary>
        /// Converts from an array of UInt32 (length at least 2) to UInt64.
        /// </summary>
        public static UInt64 ToUInt64(UInt32[] value)
        {
            Debug.Assert(value.Length >= 2);
            fixed (UInt32* valueArray = value)
            {
                UInt64* resultArray = (UInt64*)valueArray;
                return resultArray[0];
            }
        }

    }
}
