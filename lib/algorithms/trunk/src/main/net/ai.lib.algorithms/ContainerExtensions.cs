/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;

namespace ai.lib.algorithms
{
    /// <summary>
    /// Contains various extensions for container classes, such as System.Array, etc.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Get the array slice.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int length)
        {
            T[] dst = new T[length];
            for (int i = 0; i < length; ++i)
                dst[i] = source[start + i];
            return dst;
        }

        /// <summary>
        /// Compares two arrays.
        /// </summary>
        public static bool EqualsTo<T>(this T[] o1, T[] o2)
        {
            if (o1.Length != o2.Length)
                return false;
            for(int i = 0; i < o1.Length; ++i)
            {
                if(!o1[i].Equals(o2[i]))
                    return false;
            }
            return true;
        }

        public delegate T FillDelegate<T>(int i);
        public delegate T CombineWithDelegate<T>(T my, T other);

        /// <summary>
        /// Fills an array with the given value.
        /// </summary>
        /// <returns>The array specified by arr.</returns>
        public static T[] Fill<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = value;
            return arr;
        }

        /// <summary>
        /// Fills an array with the result of the call to filler.
        /// </summary>
        /// <returns>The array specified by arr.</returns>
        public static T[] Fill<T>(this T[] arr, FillDelegate<T> filler)
        {
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = filler(i);
            return arr;
        }

        /// <summary>
        /// Creates a shallow copy of array.
        /// </summary>
        public static T[] ShallowCopy<T>(this T[] arr)
        {
            T[] result = new T[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                result[i] = arr[i];
            }
            return result;
        }

        /// <summary>
        /// For each element executes func on this element (first argument) and the corresponding element of the other array (2nd argument).
        /// Example: add a2 to a1: a1.CombineWith(a2, (x,y) => x+y).
        /// </summary>
        public static void CombineWith<T>(this T[] arr, T[] other, CombineWithDelegate<T> func)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = func(arr[i], other[i]);
            }
        }

        /// <summary>
        /// Creates a deep copy of array, using ai.utils.DeepCopy.
        /// </summary>
        public static T[] DeepCopy<T>(this T[] arr)
        {
            T[] result = new T[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                result[i] = arr[i].DeepCopy();
            }
            return result;
        }


        /// <summary>
        /// Rotates a list. 
        /// </summary>
        /// <seealso cref="RotateMinCopy"/>
        /// <param name="shift">Position of the 0-th element after rotating. Must be in [0, seq.Count)</param>
        public static void Rotate<T>(this List<T> seq, int shift)
        {
            seq.Reverse();
            seq.Reverse(0, shift);
            seq.Reverse(shift, seq.Count - shift);
        }

        /// <summary>
        /// Rotates a list. 
        /// <remarks> This algorithm minimizes copying of elements.
        /// It is faster than Rotate() if the list contains classes or structs, 
        /// but slower if the list contains integers.
        /// </remarks>
        /// </summary>
        /// <param name="shift">Position of the 0-th element after rotating. Must be in [0, seq.Count)</param>
        public static void RotateMinCopy<T>(this List<T> seq, int shift)
        {
            if (shift == 0)
                return;

            int count = 0;
            int start = 0;
            do
            {
                T buf = seq[start];
                int pos = start;
                for (; ; )
                {
                    int nextPos = pos - shift;
                    if (nextPos < 0)
                        nextPos += seq.Count;
                    if (nextPos == start)
                        break;
                    seq[pos] = seq[nextPos];
                    count++;
                    pos = nextPos;
                }
                seq[pos] = buf;
                start++;
            } while (++count < seq.Count);
        }
    }
}
