/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.pkr.holdem.strategy;
using ai.lib.utils;
using ai.lib.utils.commandline;
using System.Diagnostics;

namespace ai.pkr.holdem.strategy.hs.lut_gen
{
    class Program
    {

        static CommandLineParams _cmdLine = new CommandLineParams();

        /// <summary>
        /// Calculate LUTs for class HandStrength.
        /// It takes a long time (~ 10 hours) to run.
        /// </summary>
        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                Console.WriteLine("You may need to overwrite the root directory -d:ai.Root=<root-dir> ");
                return 1;
            }

            if (_cmdLine.DebuggerLaunch)
            {
                Debugger.Launch();
            }

            string dataDir = _cmdLine.OutputDir.Get(Props.Global);

            Console.WriteLine("Create LUTs in directory {0}", dataDir);
            Directory.CreateDirectory(dataDir);

            DateTime startTime = DateTime.Now;
            Console.WriteLine("Start time {0}, will take some hours to finish.", startTime);
            HandStrength.PrecalcuateTables(dataDir, -1);
            TimeSpan time = DateTime.Now - startTime;
            Console.WriteLine("Calculated in {0} s", time.TotalSeconds);

            return 0;
        }
    }
}
