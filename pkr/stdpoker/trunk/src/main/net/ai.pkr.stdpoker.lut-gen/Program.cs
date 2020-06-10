/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ai.pkr.stdpoker.lut_gen
{
    class Program
    {
        static void Main(string[] args)
        {
            // We cannot generate all the files in one run because some generators rely on others
            // and they are initialized once (without LUT) in a static constructor.
            if (args[0] == "eval7")
            {
                GenerateLutEvaluator7();
            }
            else if (args[0] == "hvo7")
            {
                Console.WriteLine("Generating LUT for HandValueToOrdinal 7 ...");
                HandValueToOrdinal.Precalculate(7);
            }
            else
            {
                Console.WriteLine("Wrong command line parameter: '{0}'", args[0]);
            }
        }

        static void GenerateLutEvaluator7()
        {
            Console.WriteLine("Generating LUT for LutEvaluator7...");
            string lutPath = LutEvaluator7.LutPath;
            if (File.Exists(lutPath))
            {
                Console.WriteLine("{0} alredy exist, will not overwrite", lutPath);
                return;
            }
            LutEvaluatorGenerator g = new LutEvaluatorGenerator();
            g.GenerateStates(7);
            g.SaveLut(lutPath, LutEvaluator7.LutFileFormatID);
            Console.WriteLine("{0} written", lutPath);
        }
    }
}
