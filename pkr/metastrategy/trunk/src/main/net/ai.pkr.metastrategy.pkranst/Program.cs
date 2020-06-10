/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy.pkranct;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metastrategy.pkranst
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

            StrategyTree st = StrategyTree.Read<StrategyTree>(_cmdLine.StrategyTree);

            if (_cmdLine.Verify)
            {
                string error = "";
                bool isOk = true;
                if (_cmdLine.IsAbsolute)
                {
                    Console.Write("Verifying absolute strategy ...");
                    isOk = VerifyAbsStrategy.Verify(st, _cmdLine.HeroPosition, 1e-7, out error);
                }
                else
                {
                    Console.Write("Verifying conditional strategy ...");
                    isOk = VerifyCondStrategy.Verify(st, _cmdLine.HeroPosition, 1e-7, out error);
                }
                if(isOk)
                {
                    Console.WriteLine(" OK");
                }
                else
                {
                    Console.WriteLine(" Verification failed: {0}", error);
                    return 1;
                }
            }

            AnalyzeStrategyTree.AnalyzeS(st, _cmdLine.IsAbsolute, _cmdLine.HeroPosition);

            return 0;
        }
    }
}
