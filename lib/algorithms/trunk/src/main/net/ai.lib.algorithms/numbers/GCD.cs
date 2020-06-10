/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.numbers
{
    public static class GCD
    {
        public static int Calculate(int num1, int num2)
        {
            while (num1 != num2)
            {
                if (num1 > num2)
                    num1 = num1 - num2;
 
                if (num2 > num1)
                    num2 = num2 - num1;
            }
            return num1;
        }
    }
}
