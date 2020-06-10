/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.numbers
{
    public static class LCM
    {
        public static int Calculate(int num1, int num2)
        {
            return (int)(((long)num1 * (long)num2) / GCD.Calculate(num1, num2));
        }

        public static int Calculate(int [] numbers)
        {
            int lcm = numbers[0];
            for (int i = 1; i < numbers.Length; ++i)
            {
                lcm = Calculate(numbers[i], lcm);
            }
            return lcm;
        }
    }
}
