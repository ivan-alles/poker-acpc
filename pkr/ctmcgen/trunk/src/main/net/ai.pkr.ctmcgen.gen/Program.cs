/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy;
using System.IO;
using System.Threading;

namespace ai.pkr.ctmcgen.gen
{
    class Program
    {
        static CommandLineParams _cmdLine = new CommandLineParams();
        static bool _quitAsap = false;
        static bool _finishRun = false;

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

            if (gd.MinPlayers != _cmdLine.ChanceAbstractionFiles.Length)
            {
                Console.WriteLine("Number of chance abstractions ({0}) does not match to the minimal number of players in game definition ({1})",
                    _cmdLine.ChanceAbstractionFiles.Length, gd.MinPlayers);
                return 1;
            }

            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[_cmdLine.ChanceAbstractionFiles.Length];
            bool areAbstractionsEqual = true;
            string fileName0 = _cmdLine.ChanceAbstractionFiles[0].Get().ToUpperInvariant();
            for (int p = 0; p < chanceAbstractions.Length; ++p)
            {
                string fileName = _cmdLine.ChanceAbstractionFiles[p];
                if (fileName.ToUpperInvariant() != fileName0)
                {
                    areAbstractionsEqual = false;
                }
                chanceAbstractions[p] =  ChanceAbstractionHelper.CreateFromPropsFile(fileName);
                Console.WriteLine("CA pos {0}: {1}", p, chanceAbstractions[p].Name);
            }

            Console.WriteLine("Abstractions are {0}.", areAbstractionsEqual ? "equal" : "unequal");
            Console.Write("Samples: {0:#,#}. ", _cmdLine.SamplesCount);

            if (_cmdLine.RunsCount > 0)
            {
                Console.Write("Runs: {0:#,#}", _cmdLine.RunsCount);
            }
            else
            {
                Console.WriteLine("Runs: unlimited");
                Console.Write("Press 'q' to quit asap, 'f' to finish run.");
            }
            Console.WriteLine();

            for (int run = 0; ; )
            {
                int rngSeed = (int) DateTime.Now.Ticks;

                Console.Write("Run:{0}, seed:{1}. ", run, rngSeed);

                DateTime startTime = DateTime.Now;

                CtMcGen.Tree tree = CtMcGen.Generate(gd, chanceAbstractions, areAbstractionsEqual, _cmdLine.SamplesCount, rngSeed, Feedback);

                double genTime = (DateTime.Now - startTime).TotalSeconds;

                string fileName = SaveFile(tree, gd, chanceAbstractions);

                Int64 samplesDone = (Int64)tree.SamplesCount;

                Console.WriteLine("Samples: {0:#,#}, time: {1:0.0} s, {2:#,#} sm/s, file: {3}",
                    samplesDone,
                    genTime,
                    samplesDone / genTime,
                    fileName);

                ++run;
                if (_cmdLine.RunsCount > 0)
                {
                    if (run >= _cmdLine.RunsCount)
                    {
                        break;
                    }
                }
                else
                {
                    ProcessKeys();
                    if (_quitAsap || _finishRun)
                    {
                        Console.WriteLine("Exiting on user request");
                        break;
                    }
                }
            }
            return 0;
        }

        static bool Feedback(Int64 samplesCount)
        {
            ProcessKeys();
            return !_quitAsap;
        }

        private static string SaveFile(CtMcGen.Tree tree, GameDefinition gd, IChanceAbstraction[] chanceAbstractions)
        {
            string dir = _cmdLine.Output.Get(Props.Global);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string fileName = Path.Combine(dir, GetUniqueFileName(gd, chanceAbstractions));
            for (int attempt = 0; ; ++attempt)
            {
                try
                {
                    tree.Write(fileName);
                    // Write was successful - exit loop
                    break; 
                }
                catch(IOException e)
                {
                    if (attempt >= 2)
                    {
                        throw e;
                    }
                    // Maybe it was an exception because another process created the file with the same name.
                    // Wait a little (this will change the file name) and try again.
                    Thread.Sleep(500);
                }
            }
            return fileName;
        }

        private static string GetUniqueFileName(GameDefinition gd, IChanceAbstraction[] chanceAbstractions)
        {
            StringBuilder name = new StringBuilder("ctmc-");
            try
            {
                String machineName = Environment.MachineName;
                String date = DateTime.Now.ToString("yyMMdd-HHmmffff");
                String gdName = gd.Name;
                if (gdName != "")
                {
                    name.Append("-"+gdName);
                }
                if (_cmdLine.AddCaNames)
                {
                    for (int p = 0; p < chanceAbstractions.Length; ++p)
                    {
                        String caName = chanceAbstractions[p].Name;
                        if (!string.IsNullOrEmpty(caName))
                        {
                            name.Append("--" + caName);
                        }
                    }
                    name.Append('-');
                }

                if (machineName != "")
                    name.Append("-" + machineName);
                if (date != "")
                    name.Append("-" + date);
                foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
                    name.Replace(invalidChar, '_');
                name.Replace(' ', '_');
            }
            catch
            {
            }
            name.Append(".dat");
            return name.ToString();
        }

        private static void ProcessKeys()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 'q': _quitAsap = true;
                        break;
                    case 'f': _finishRun = true;
                        break;
                }
            }
        }
    }
}
