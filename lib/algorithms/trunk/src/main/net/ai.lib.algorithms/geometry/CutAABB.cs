/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ai.lib.algorithms.geometry
{
    /// <summary>
    /// Given:
    /// <para>an n-dimentional AABB P0 specified by the center point c0 and vector of half-sizes s0.
    /// The AABB contains points satisfying: c0_i - s0_i &lt;= x_i &lt;= c0_i + s0_i. </para>
    /// <para>a hyperplane p(x) = a_0*x_0 + ... a_n-1*x_n-1 + b = 0</para>
    /// <para>Find a minimal AABB P1 containing intersection of P0 and the half-hyperspace p(x) &lt;= 0 or 
    /// tell that they do not intersect.</para>
    /// <para>P1 s1_i == 0 is also a positive answer, it can be that the intersection contains only one point.</para>
    /// <para>Complexity: O(n^2).</para>
    /// </summary>
    public class CutAABB
    {
        public static bool Cut(double [] c0, double [] s0, double [] p, double [] c1, double [] s1)
        {
            int n = c0.Length;
            Debug.Assert(s0.Length == n && p.Length == n + 1 && c1.Length == n && s1.Length == n);
            for(int i = 0; i < n; ++i)
            {
                if(p[i] == 0)
                {
                    // Hyperplane is parallel to x_i basis vector.
                    // Lets assume for now on that the intersection is not empty,
                    // then the new AABB will contain the whole i-size.
                    c1[i] = c0[i];
                    s1[i] = s0[i];
                }
                else
                {
                    double x = 0;
                    for(int j = 0; j < n; ++j)
                    {
                        if(i == j)
                        {
                            continue;
                        }
                        x += (c0[j] - Math.Sign(p[j])*s0[j])*p[j];
                    }
                    x += p[n];
                    x = -x/p[i];
                    double k;
                    if (p[i] >= 0)
                    {
                        if (x < c0[i] - s0[i])
                        {
                            // Intersection is empty. 
                            return false;
                        }
                        k = Math.Min(x, c0[i] + s0[i]);
                        s1[i] = (k - c0[i] + s0[i]) / 2;
                        c1[i] = (k + c0[i] - s0[i]) / 2;
                    }
                    else
                    {
                        if (x > c0[i] + s0[i])
                        {
                            // Intersection is empty. 
                            return false;
                        }
                        k = Math.Max(x, c0[i] - s0[i]);
                        s1[i] = (c0[i] + s0[i] - k) / 2;
                        c1[i] = (c0[i] + s0[i] + k) / 2;
                    }
                    
                }
            }
            return true;
        }
    }
}
