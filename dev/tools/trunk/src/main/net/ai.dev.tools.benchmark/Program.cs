/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;

namespace ai.dev.tools.benchmark
{
    class Program
    {
        static CommandLineParams _cmdLine = new CommandLineParams();

        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return 1;
            }

            if (_cmdLine.DebuggerLaunch)
            {
                Debugger.Launch();
            }

            if(_cmdLine.RangeEnd < 0)
            {
                _cmdLine.RangeEnd = _cmdLine.RangeBegin;
            }

            if(_cmdLine.TestName == "*")
            {
                AllTests();
            }
            else if (_cmdLine.TestName == "exh-mem")
            {
                ExhaustiveMemoryTest();
            }
            else if (_cmdLine.TestName == "thread-perf")
            {
                ThreadPerformanceTest();
            }

            return 0;
        }

        private static void ThreadPerformanceTest()
        {
            ThreadBenchmark b = new ThreadBenchmark { IsVerbose = true };
            int best;
            double repPerSec = b.MultithreadPerformanceBest(
                _cmdLine.RangeBegin, _cmdLine.RangeEnd, 
                _cmdLine.RepetitionsCount, out best);
            Console.WriteLine();
        }

        private static void ExhaustiveMemoryTest()
        {
            MemoryBenchmark b = new MemoryBenchmark
            {
                IsVerbose = true
            };
            int bestElCount;
            b.AllocateArrayExhaustiveBest<double>(
                _cmdLine.RangeBegin, _cmdLine.RangeEnd, 
                out bestElCount);
            Console.WriteLine();
        }

        private static void AllTests()
        {
            ExhaustiveMemoryTest();
            ThreadPerformanceTest();
        }
    }
}
