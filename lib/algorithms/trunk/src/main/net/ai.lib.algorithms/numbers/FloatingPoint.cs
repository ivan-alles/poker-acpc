/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.numbers
{
    public static class FloatingPoint
    {
        /// <summary>
        /// Checks is abs(v1-v2) &lt;= epsilon.
        /// </summary>
        public static bool AreEqual(double v1, double v2, double epsilon)
        {
            return Math.Abs(v1 - v2) <= Math.Abs(epsilon);
        }

        /// <summary>
        /// Compares 2 double numbers with a given relative precision.
        /// Relative precision is usually a number [0..1].
        /// It is like percentage with the difference that percentage is by 100 times larger.
        /// </summary>
        public static bool AreEqualRel(double v1, double v2, double relativeEpsilon)
        {
            if (AreEqual(v1, v2, v1 * relativeEpsilon))
                return true;
            return AreEqual(0, v1, relativeEpsilon) && AreEqual(0, v2, relativeEpsilon);
        }
    }
}