/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ai.lib.algorithms.color
{
    /// <summary>
    /// Calculates a gradient of colors.
    /// </summary>
    public class Gradient
    {
        public static byte Calculate(byte c1, byte c2, double gr)
        {
            double result = c1 * gr + c2 * (1 - gr);
            return (byte)(Math.Min(255, result));
        }

        public static Color Calculate(Color c1, Color c2, double gr)
        {
            byte a = Calculate(c1.A, c2.A, gr);
            byte r = Calculate(c1.R, c2.R, gr);
            byte g = Calculate(c1.G, c2.G, gr);
            byte b = Calculate(c1.B, c2.B, gr);
            Color result = Color.FromArgb(a, r, g, b);
            return result;
        }
    }
}
