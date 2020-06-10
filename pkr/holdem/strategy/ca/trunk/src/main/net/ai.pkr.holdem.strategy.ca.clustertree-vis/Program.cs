/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;
using System.Diagnostics;
using System.IO;
namespace ai.pkr.holdem.strategy.ca.clustertree_vis
{
    static class Program
    {
        static CommandLine _cmdLine = new CommandLine();

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

            foreach (string rtFile in _cmdLine.InputFiles)
            {
                Visualize(rtFile);
            }

            return 0;
        }

        private static void Visualize(string rtFile)
        {
            ClusterTree rt = ClusterTree.Read(rtFile);
            string fileName = rtFile + ".gv";

            using (TextWriter output = new StreamWriter(fileName))
            {
                var vis = new ClusterTree.Vis
                {
                    Output = output,
                    ShowDealPath = _cmdLine.ShowPath
                    
                };
                vis.Walk(rt, rt.Root);
            }
        }
    }
}