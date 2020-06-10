/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metastrategy;

namespace ai.pkr.ctmcgen.conv
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

            CtMcGen.Tree input = new CtMcGen.Tree();
            input.Read(_cmdLine.Input);
            long leavesCount = input.CalculateLeavesCount();
            Console.WriteLine("Input file: leaves: {0:#,#}, samples: {1:#,#}, av. samples: {2:#,#}",
                              leavesCount, input.SamplesCount, input.SamplesCount / (ulong)leavesCount);

            ChanceTree ct = input.ConvertToChanceTree();
            ct.Write(_cmdLine.Output);

            return 0;
        }
    }
}
