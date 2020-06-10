/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metastrategy.pkrcmpct
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

            if (_cmdLine.ChanceTrees.Length != 2)
            {
                Console.WriteLine("Can compare 2 chance trees, but was specified {0}", _cmdLine.ChanceTrees.Length);
                return 1;
            }
            ChanceTree[] chanceTrees = new ChanceTree[2];
            for (int i = 0; i < 2; ++i)
            {
                chanceTrees[i] = ChanceTree.Read<ChanceTree>(_cmdLine.ChanceTrees[i]);
            }
            CompareChanceTrees cmp = new CompareChanceTrees {
                AllowDifferentStructure = _cmdLine.AllowDifferentStructure, 
                IsVerbose = true 
            };
            cmp.Compare(chanceTrees[0], chanceTrees[1]);
            
            return 0;
        }
    }
}
