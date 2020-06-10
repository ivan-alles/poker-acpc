/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.lib.utils;
using System.IO;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metastrategy.pkrbr
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
            if (_cmdLine.DiagUnmanagedMemory)
            {
                UnmanagedMemory.IsDiagOn = true;
                Console.WriteLine("Unmanaged memory diagnostics is on");
            }

            ActionTree at = ActionTree.Read<ActionTree>(_cmdLine.ActionTree);
            Console.WriteLine("Action tree: {0}", at.Version.ToString());

            ChanceTree ct = ChanceTree.Read<ChanceTree>(_cmdLine.ChanceTree);
            Console.WriteLine("Chance tree: {0}", ct.Version.ToString());

            StrategyTree oppSt = StrategyTree.Read<StrategyTree>(_cmdLine.OppStrategy);
            Console.WriteLine("Opp strategy: {0}", oppSt.Version.ToString());


            // Create and configure Br solver
            Br br = new Br
            {
                HeroPosition = _cmdLine.HeroPosition,
                ChanceTree = ct,
                ActionTree = at,
            };
            br.Strategies = new StrategyTree[ct.PlayersCount];
            for (int opp = 0; opp < ct.PlayersCount; ++opp)
            {
                if (opp == _cmdLine.HeroPosition) continue;
                br.Strategies[opp] = oppSt;
            }

            DateTime startTime = DateTime.Now;

            // Solve Br
            br.Solve();

            double time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("Done in {0:0.0} s, BR value for hero pos {1}: {2}", time, br.HeroPosition, br.Value);

            string outFile = Path.Combine(Path.GetDirectoryName(_cmdLine.OppStrategy), Path.GetFileNameWithoutExtension(_cmdLine.OppStrategy));
            outFile += "-br.dat";
            Console.WriteLine("Writing br to {0}", outFile);
            br.Strategies[_cmdLine.HeroPosition].Write(outFile);

            return 0;
        }
    }
}
