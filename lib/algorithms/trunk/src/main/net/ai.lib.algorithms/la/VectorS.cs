/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ai.lib.algorithms.la
{
    /// <summary>
    /// Static operations on vectors, represented as arrays.
    /// </summary>
    public static class VectorS
    {

        /// <summary>
        /// Volume.
        /// </summary>
        public static double Volume(double[] v)
        {
            double vol = 1;
            for (int i = 0; i < v.Length; ++i)
            {
                vol *= v[i];
            }
            return vol;
        }

        public static bool AreEqual(double[] v1, double[] v2)
        {
            if (v1.Length != v2.Length)
            {
                return false;
            }
            for (int i = 0; i < v1.Length; i++)
            {
                if (v1[i] != v2[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Squared euclidian distance.
        /// </summary>
        public static double SquaredDistance(double[] v1, double[] v2) 
        {
            Debug.Assert(v1.Length == v2.Length);
            double dist = 0;
            for(int i = 0; i < v1.Length; ++i)
            {
                double d = v1[i] - v2[i];
                dist += d * d;
            }
            return dist;
        }

        /// <summary>
        /// Euclidian distance.
        /// </summary>
        public static double Distance(double[] v1, double[] v2)
        {
            Debug.Assert(v1.Length == v2.Length);
            return Math.Sqrt(SquaredDistance(v1, v2));
        }

        /// <summary>
        /// Returns v1 + v2.
        /// </summary>
        public static double[] Add(double[] v1, double[] v2)
        {
            Debug.Assert(v1.Length == v2.Length);
            double[] v = new double[v1.Length];
            for (int i = 0; i < v1.Length; ++i)
            {
                v[i] = v1[i] + v2[i];
            }
            return v;
        }

        /// <summary>
        /// Returns v1 - v2.
        /// </summary>
        public static double[] Sub(double[] v1, double[] v2)
        {
            Debug.Assert(v1.Length == v2.Length);
            double[] v = new double[v1.Length];
            for (int i = 0; i < v1.Length; ++i)
            {
                v[i] = v1[i] - v2[i];
            }
            return v;
        }

        /// <summary>
        /// Updates each coordinate of min, if the corresponding coordinate of v is less.
        /// </summary>
        public static void UpdateMin(double[] v, double[] min)
        {
            Debug.Assert(min.Length == v.Length);
            for (int i = 0; i < v.Length; ++i)
            {
                if(min[i] > v[i])
                {
                    min[i] = v[i];
                }
            }
        }

        /// <summary>
        /// Updates each coordinate of max, if the corresponding coordinate of v is less.
        /// </summary>
        public static void UpdateMax(double[] v, double[] max)
        {
            Debug.Assert(max.Length == v.Length);
            for (int i = 0; i < v.Length; ++i)
            {
                if (max[i] < v[i])
                {
                    max[i] = v[i];
                }
            }
        }

        /// <summary>
        /// Normalizes v. If for a coodinate min[i] == max[i], no normalization for this coordinate is done.
        /// </summary>
        public static void Normalize(double[] v, double[] min, double[] max)
        {
            Debug.Assert(min.Length == v.Length);
            Debug.Assert(max.Length == v.Length);

            for (int i = 0; i < v.Length; ++i)
            {
                double d = max[i] - min[i];
                if (d != 0)
                {
                    v[i] = (v[i] - min[i])/d;
                }
            }
        }

        /// <summary>
        /// Normalizes v. If a coodinate of minMaxDiff is 0, no normalization for this coordinate is done.
        /// </summary>
        public static void NormalizeByDiff(double[] v, double[] min, double[] minMaxDiff)
        {
            Debug.Assert(min.Length == v.Length);
            Debug.Assert(minMaxDiff.Length == v.Length);

            for (int i = 0; i < v.Length; ++i)
            {
                if (minMaxDiff[i] != 0)
                {
                    v[i] = (v[i] - min[i])/minMaxDiff[i];
                }
            }
        }
    }
}
