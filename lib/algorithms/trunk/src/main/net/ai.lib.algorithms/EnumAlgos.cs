/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms
{
    /// <summary>
    /// Algorithms for combinatorics enumerations.
    /// <remarks>The name is chosen to be short and clear and not to intersect with System.Enum</remarks>
    /// </summary>
    public static class EnumAlgos
    {
        public static Int64 Factorial(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException();
            Int64 p = 1;
            for (int i = 1; i <= n; i++)
                p *= i;
            return p;
        }

        public static Int64 CountCombin(int n, int k)
        {
            if (n < 0 || k < 0)
                throw new ArgumentOutOfRangeException();
            Int64 c = 1;
            if (n - k > k) 
                k = n - k;
            for (int i = k + 1; i <= n; i++)
                c *= i;
            for (int i = 2; i <= n - k; i++)
                c /= i;
            return c;
        }

        public static Int64 CountPermut(int n, int k)
        {
            if (n < 0 || k < 0)
                throw new ArgumentOutOfRangeException();
            Int64 c = 1;
            for (int i = n - k + 1; i <= n; i++)
                c *= i;
            return c;
        }

        /// <summary>
        /// Generates a list, containing lists with all permutations of numbers [0..n).
        /// The sequence at index 0 is guaranteed to be [0..n).
        /// Can be useful for a small n (&lt; 7).
        /// </summary>
        /// <returns></returns>
        public static List<List<int>> GetPermut(int n)
        {
            List<List<int>> result = new List<List<int>>((int)CountPermut(n, n));
            List<int> permut = new List<int>(n);
            for(int i = 0; i < n; ++i)
            {
                permut.Add(i);
            }
            GetPermutInternal(result, permut, n);
            return result;
        }

        private static void GetPermutInternal(List<List<int>> result, List<int> permut, int n)
        {
            if (n == 1)
            {
                result.Add(new List<int>(permut));
                return;
            }
            int start = permut.Count - n;
            for(int i = 0; i < n; ++i)
            {
                int tmp = permut[start + i];
                permut[start + i] = permut[start];
                permut[start] = tmp;
                GetPermutInternal(result, permut, n - 1);
                permut[start] = permut[start + i];
                permut[start + i] = tmp;
            }
        }
    }
}