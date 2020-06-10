/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using ai.lib.utils;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ai.pkr.fictpl.fictpl
{
    static class Program
    {
        static CommandLineParams _cmdLine = new CommandLineParams();

        static List<double> _epsilons = new List<double>();
        static bool _epsilonsAdjusted = false;
        static double _reachedEpsilon = -1;
        static bool _restart = false;
        static DateTime _lastSnapshotTime;

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

            Console.WriteLine("Chance abstractions are {0}.", _cmdLine.EqualCa ? "equal" : "unequal");


            if (!ParseEpsilons())
            {
                return 1;
            }

            for (; ; )
            {
                FictitiousPlay solver = new FictitiousPlay
                                            {
                                                ChanceTreeFile = _cmdLine.ChanceTree,
                                                EqualCa = _cmdLine.EqualCa,
                                                ActionTreeFile = _cmdLine.ActionTree,
                                                OutputPath = _cmdLine.Output,
                                                SnapshotsCount = _cmdLine.SnapshotCount,
                                                Epsilon = _epsilons.LastOrDefault(),
                                                EpsilonLogThreshold =
                                                    double.Parse(_cmdLine.EpsilonLogThreshold,
                                                                 CultureInfo.InvariantCulture),
                                                OnIterationDone = OnIterationDone,
                                                IterationVerbosity = _cmdLine.IterationVerbosity,
                                                ThreadsCount = _cmdLine.ThreadCount,
                                                IsVerbose = true
                                            };

                _lastSnapshotTime = DateTime.Now;
                _restart = false;
                solver.Solve();

                if (_reachedEpsilon > 0 || solver.CurrentEpsilon < solver.Epsilon)
                {
                    // Either an intermediate or the final epsilon is reached.
                    // Copy the snapshot to make sure it will not get lost.
                    string targetPath = Path.Combine(solver.OutputPath, string.Format("eps-{0:0.0000}", solver.CurrentEpsilon));
                    string sourcePath = solver.CurrentSnapshotInfo.BaseDir;
                    Console.WriteLine("Copy epsilon snapshot {0} to {1}", sourcePath, targetPath);
                    DirectoryExt.Copy(sourcePath, targetPath);
                    _reachedEpsilon = -1;
                }

                if (!_restart)
                {
                    break;
                }
            }

            return 0;
        }

        static bool OnIterationDone(FictitiousPlay solver)
        {
            if ((DateTime.Now - _lastSnapshotTime).TotalMinutes >= _cmdLine.SnapshotTime)
            {
                // Do snapshot by timer
                Console.WriteLine("Make a snapshot by timer");
                _restart = true;
                return false;
            }

            // Do some iterations to ensure stable epsilon.
            if (solver.CurrentIterationCount > 20)
            {
                if (!_epsilonsAdjusted)
                {
                    RemoveReachedEpsilons(solver.CurrentEpsilon);
                    _epsilonsAdjusted = true;
                }
                // The last epsilon is already set in the solver, so check
                // only if there are many.
                if (_epsilons.Count > 1 && solver.CurrentEpsilon <= _epsilons[0])
                {
                    Console.WriteLine("Epsilon {0} reached, make a snapshot", _epsilons[0]);
                    _reachedEpsilon = _epsilons[0];
                    _epsilons.RemoveAt(0);
                    _restart = true;
                    return false;
                }
            }
            if (IsExitKeyPressed())
            {
                return false;
            }
            return true;
        }

        private static bool IsExitKeyPressed()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 'q': return true;
                }
            }
            return false;
        }

        private static void RemoveReachedEpsilons(double curEpsilon)
        {
            while (_epsilons.Count > 0)
            {
                if (_epsilons[0] < curEpsilon)
                {
                    break;
                }
                _epsilons.RemoveAt(0);
            }
        }

        /// <summary>
        /// Comparer to sort in desceding order. 
        /// </summary>
        class EpsilonComparer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                if (x < y)
                    return 1;
                else if (x > y)
                    return -1;
                return 0;
            }
        }

        private static bool ParseEpsilons()
        {
            string[] epsStrings = _cmdLine.Epsilons.Get().Split(new char[] { ',' });
            foreach (string epsString in epsStrings)
            {
                double e = double.Parse(epsString, CultureInfo.InvariantCulture);
                _epsilons.Add(e);
            }
            if (_epsilons.Count == 0)
            {
                Console.WriteLine("ERROR: No epsilons specified");
                return false;
            }
            _epsilons.Sort(new EpsilonComparer());
            return true;
        }
    }
}
