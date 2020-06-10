/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metastrategy.pkranct
{
    static class Program
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

            ChanceTree ct = ChanceTree.Read<ChanceTree>(_cmdLine.ChanceTree);

            if (_cmdLine.Verify)
            {
                Console.Write("Verifying tree ...");
                VerifyChanceTree.VerifyS(ct);
                Console.WriteLine(" OK");
            }

            AnalyzeChanceTree.AnalyzeS(ct);

            return 0;
        }
    }
}
