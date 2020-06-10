/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ai.lib.algorithms.numbers
{
    /// <summary>
    /// Stores a real number from [0..1] an an UInt32 integer. This is a compact and fast represntation
    /// of such numbers. This can be useful, for example, to work with probabilities.
    /// </summary>
    public struct Real01
    {
        #region Constructors and conversions

        public const double EPSILON = (MAX - MIN)/(1u << (SIZE - 1));

        public Real01(Real01 r)
        {
            Data = r.Data;
        }

        public Real01(Double v)
        {
            Data = FromDouble(v);
        }

        /// <summary>
        /// Implicit conversion from double.
        /// </summary>
        public static implicit operator Real01(double value)  
        {
            return new Real01(value);
        }


        /// <summary>
        /// Converts to double.
        /// </summary>
        public double ToDouble()
        {
            return ToDouble(Data);
        }

        /// <summary>
        /// Allows explicit conversion to double.
        /// No implicit conversion to avoid auto-conversion in arithmetic operations.
        /// </summary>
        public static explicit operator double(Real01 r)
        {
            return r.ToDouble();
        }

        /// <summary>
        /// Converts the internal representation to double.
        /// </summary>
        public static double ToDouble(UInt32 data)
        {
            double result = 0;
            for (int i = 0; data > 0 && i < SIZE; ++i)
            {
                UInt32 bit = _bits[i];
                if ((data & bit) != 0)
                {
                    result += _intervals[i];
                    data &= ~bit;
                }
            }
            return result;
        }

        /// <summary>
        /// Converts double to internal representation.
        /// </summary>
        public static UInt32 FromDouble(double v)
        {
            Debug.Assert(v >= MIN && v <= MAX);
            UInt32 data = 0;
            for (int i = 0; v > 0 && i < SIZE; ++i)
            {
                double interval = _intervals[i];
                if (v >= interval)
                {
                    data |= _bits[i];
                    v -= interval;
                }
            }
            return data;
        }

        #endregion

        #region Properties

        public UInt32 Data;

        #endregion

        #region Methods

        
        #endregion

        #region Implementation

        private static readonly double[] _intervals = new double[SIZE];
        private static readonly UInt32[] _bits = new UInt32[SIZE];

        private const int SIZE = 32;
        private const double MIN = 0.0;
        private const double MAX = 1.0;

        static Real01()
        {
            UInt32 bit = 1u << (SIZE-1);
            double interval = MAX;
            for(int i = 0; i < SIZE; ++i)
            {
                _bits[i] = bit;
                _intervals[i] = interval;
                bit >>= 1;
                interval *= 0.5;
            }
        }

        #endregion
    }
}
