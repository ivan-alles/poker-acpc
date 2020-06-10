/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metastrategy.algorithms;
using System.IO;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metastrategy.eqlp
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

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(_cmdLine.GameDef.Get(Props.Global));
            
            string chanceTreeFile = _cmdLine.ChanceTree.Get(Props.Global);
            ChanceTree ct;
            if (string.IsNullOrEmpty(chanceTreeFile))
            {
                ct = CreateChanceTreeByGameDef.Create(gd);
            }
            else
            {
                ct = ChanceTree.Read<ChanceTree>(chanceTreeFile);
            }

            ActionTree at = CreateActionTreeByGameDef.Create(gd);

            double [] values;
            StrategyTree [] st = EqLp.Solve(at, ct, out values);
            if (st == null)
            {
                Console.WriteLine("Cannot find solution");
                return 1;
            }
            string output = _cmdLine.Output.Get(Props.Global);
            string basePath = string.IsNullOrEmpty(output) ? gd.Name : output;
            string baseDir = Path.GetDirectoryName(basePath);
            string baseName = Path.GetFileNameWithoutExtension(basePath);

            for (int p = 0; p < st.Length; ++p)
            {
                string fileName = Path.Combine(baseDir, string.Format("{0}-{1}.dat", baseName, p));
                Console.WriteLine("Eq value pos {0}: {1}, file: {2}", p, values[p], fileName);
                st[p].Write(fileName);
            }



            return 0;
        }
    }
}
