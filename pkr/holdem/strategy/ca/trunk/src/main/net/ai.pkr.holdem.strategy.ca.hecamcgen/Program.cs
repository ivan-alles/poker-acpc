/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metastrategy;
using System.IO;
using ai.lib.utils;

namespace ai.pkr.holdem.strategy.ca.hecamcgen
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
            Props caProps = XmlSerializerExt.Deserialize<Props>(_cmdLine.ChanceAbstractionFile);
            caProps.Set("IsCreatingClusterTree", "true");

            IChanceAbstraction ca = ChanceAbstractionHelper.CreateFromProps(caProps);
            Console.WriteLine("CA: {0}", ca.Name);
            
            List<int> samplesCount = new List<int>();
            foreach(string sc in  _cmdLine.SamplesCount.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                samplesCount.Add(int.Parse(sc));
            }

            int rngSeed = _cmdLine.RngSeed == 0 ? (int)DateTime.Now.Ticks : _cmdLine.RngSeed;
            Console.WriteLine("RNG seed: {0}", rngSeed);
            
            CaMcGen gen = new CaMcGen
            {
                Clusterizer = (IClusterizer)ca,
                IsVerbose = true,
                // IsVerboseSamples = true,
                RngSeed = rngSeed,
                SamplesCount = samplesCount.ToArray()
            };

            ClusterTree rt = new ClusterTree();

            rt.Root = gen.Generate();
            string dir = Path.GetDirectoryName(_cmdLine.ChanceAbstractionFile);
            string file = Path.GetFileNameWithoutExtension(_cmdLine.ChanceAbstractionFile) + ".dat";
            string fileName = Path.Combine(dir, file);
            Console.WriteLine("Writing range tree to {0}", fileName);
            rt.Write(fileName);

            return 0;
        }
    }
}
