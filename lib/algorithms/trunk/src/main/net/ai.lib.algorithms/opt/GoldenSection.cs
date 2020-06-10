/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.lib.algorithms.numbers;

namespace ai.lib.algorithms.opt
{
    /// <summary>
    /// Performs a golden section search for the minimum of a unimodal function R->R.
    /// See http://en.wikipedia.org/wiki/Golden_section_search
    /// </summary>
    public class GoldenSectionSearch
    {
        public delegate double Func(double x);

        public Func F
        {
            set;
            get;
        }

        public double Solve(double a, double b, double epsilon, bool verifyFunctionShape)
        {
            if(a > b)
            {
                ShortSequence.Swap(ref a, ref b);
            }

            double x1 = b - (b-a)/GR;
            double x2 = a + (b-a)/GR;
            double f1 = F(x1);
            double f2 = F(x2);
            double fStart = 0, fEnd = 0;
            if (verifyFunctionShape)
            {
                fStart = F(a);
                fEnd = F(b);
                if(!IsShapeCorrect(fStart, fEnd, f1) || !IsShapeCorrect(fStart, fEnd, f2))
                {
                    throw new ApplicationException("Wrong function shape");
                }
            }

            for (;;)
            {
                if(b - a <= epsilon)
                {
                    return (b + a) * 0.5;
                }
                if(f1 > f2)
                {
                    // Replace a
                    a = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = a + (b-a)/GR;
                    f2 = F(x2);
                    if(verifyFunctionShape)
                    {
                        if (!IsShapeCorrect(fStart, fEnd, f2))
                        {
                            throw new ApplicationException("Wrong function shape");
                        }
                    }
                }
                else
                {
                    // Replace b
                    b = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = b - (b-a)/GR;
                    f1 = F(x1);
                    if(verifyFunctionShape)
                    {
                        if (!IsShapeCorrect(fStart, fEnd, f1))
                        {
                            throw new ApplicationException("Wrong function shape");
                        }
                    }
                }
                Debug.Assert(FloatingPoint.AreEqual((b-a)/(x2 - a), GR, 1e-10));  
                Debug.Assert(FloatingPoint.AreEqual((b-a)/(b - x1), GR, 1e-10));  
            }
        }

        bool IsShapeCorrect(double fStart, double fEnd, double f)
        {
            if (f > fStart)
            {
                return f < fEnd;
            }
            else if (f > fEnd)
            {
                return f < fStart;
            }
            return true;
        }

        readonly double GR = (1.0 + Math.Sqrt(5.0)) * 0.5;
    }
}
