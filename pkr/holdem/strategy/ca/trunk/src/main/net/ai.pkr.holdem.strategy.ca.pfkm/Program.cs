/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.ca.hecamcgen;
using ai.lib.utils.commandline;
using System.Diagnostics;
using ai.pkr.holdem.strategy.core;
using ai.lib.algorithms;
using System.IO;
using System.Text.RegularExpressions;
using ai.lib.kmeans;
using System.Globalization;

namespace ai.pkr.holdem.strategy.ca.pfkm
{
    static class Program
    {
        static CommandLine _cmdLine = new CommandLine();


        /// <summary>
        /// To sort centers ascending by coord. 0, then by coord. 1 etc.
        /// This is usually what we want - the lowest value goes to bucket 0, the highest - to bucket N.
        /// </summary>
        class CenterComparer : IComparer<double[]>
        {

            #region IComparer<double[]> Members

            public int Compare(double[] x, double[] y)
            {
                if(x.Length != y.Length)
                {
                    throw new ArgumentException("Array length mismatch");
                }
                for (int i = 0; i < x.Length; ++i)
                {
                    if (x[i] < y[i])
                    {
                        return -1;
                    }
                    if (x[i] > y[i])
                    {
                        return 1;
                    }
                }
                return 0;
            }

            #endregion
        }

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

            PocketData [] pockets = new PocketData[HePocket.Count].Fill(i => new PocketData(_cmdLine.Dim));

            PrintParameters();

            ParseInputFile(pockets);

            Console.WriteLine("Input data:");
            PrintPockets(pockets, false);
            Console.WriteLine();

            double[][] centers = SolveKMeans(pockets);
            Console.WriteLine("Centers:");
            PrintCenters(centers);


            Array.Sort(centers, new CenterComparer());
            Console.WriteLine("Sorted centers:");
            PrintCenters(centers);

            AssignCenters(pockets, centers);

            Console.WriteLine("Center assignments:");
            PrintPockets(pockets, true);
            Console.WriteLine();

            Console.WriteLine("Buckets:");
            PrintBuckets(pockets);

            return 0;
        }

        static void PrintParameters()
        {
            Console.WriteLine("d: {0}, k: {1}, stages: {2}, use pocket counts: {3}", _cmdLine.Dim, _cmdLine.K, _cmdLine.Stages, _cmdLine.UsePocketCounts);
        }

        private static void PrintBuckets(PocketData[] pockets)
        {
            for (int b = _cmdLine.K-1; b >= 0; --b)
            {
                Console.Write("{0,3}: ", b);
                for (int i = 0; i < pockets.Length; ++i)
                {
                    if (pockets[i].Center == b)
                    {
                        Console.Write("{0} ", HePocket.KindToString((HePocketKind) i));
                    }
                }
                Console.WriteLine();
            }
        }


        private static void PrintPockets(PocketData[] pockets, bool printCenters)
        {
            for (int i = 0; i < pockets.Length; ++i)
            {
                Console.Write("{0,3} ", HePocket.KindToString((HePocketKind)i));
                pockets[i].Print(Console.Out, printCenters);
                Console.WriteLine();
            }
        }

        private static void AssignCenters(PocketData[] pockets, double[][] centers)
        {
            for(int p = 0; p < pockets.Length; ++p)
            {
                double bestDist = double.MaxValue;
                int bestCenter = -1;
                for(int c = 0; c < centers.Length; ++c)
                {
                    double dist = CalcSquaredDistance(pockets[p].Value, centers[c]);
                    if(dist < bestDist)
                    {
                        bestDist = dist;
                        bestCenter = c;
                    }
                }
                pockets[p].Center = bestCenter;
            }
        }

        static double CalcSquaredDistance(double[] p1, double [] p2)
        {
            double dist = 0;

            for (int d = 0; d < p1.Length; ++d)
            {
                double coord_dist = p1[d] - p2[d];
                dist += coord_dist * coord_dist;
            }
            return dist;
        }

        private static unsafe double[][] SolveKMeans(PocketData[] pockets)
        {
            Kml.Init(null);

            Kml.Parameters kmParams = new Kml.Parameters();

            kmParams.SetDefaultTerm();
            kmParams.dim = _cmdLine.Dim;
            kmParams.term_st_a = _cmdLine.Stages;
            kmParams.term_st_b = kmParams.term_st_c = kmParams.term_st_d = 0;
            kmParams.seed = 1;
            kmParams.k = _cmdLine.K;
            kmParams.n = _cmdLine.UsePocketCounts ? 1326: 169;
            kmParams.Allocate();

            Console.WriteLine("Data passed to kml:");
            int p = 0;
            for (int pocket = 0; pocket < pockets.Length; ++pocket)
            {
                int count = _cmdLine.UsePocketCounts ? HePocket.KindToRange((HePocketKind)pocket).Length : 1;
                for (int i = 0; i < count; ++i)
                {
                    for (int d = 0; d < _cmdLine.Dim; ++d)
                    {
                        double value = pockets[pocket].Value[d];
                        *kmParams.GetPoint(p, d) = value;
                        Console.Write(value.ToString(CultureInfo.InvariantCulture) + " ");
                    }
                    Console.WriteLine();
                    ++p;
                }
            }
            Console.WriteLine();
            Debug.Assert(!_cmdLine.UsePocketCounts || p == 1326);

            Kml.KML_Hybrid(&kmParams);

            double[][] centers = new double[_cmdLine.K][].Fill(i => new double[_cmdLine.Dim]);

            for (int c = 0; c < kmParams.k; ++c)
            {
                for (int d = 0; d < kmParams.dim; ++d)
                {
                    centers[c][d] = *kmParams.GetCenter(c, d);
                }
            }

            kmParams.Free();

            return centers;
        }

        static void PrintCenters(double[][] centers)
        {
            for (int c = 0; c < centers.Length; ++c)
            {
                Console.Write("{0, 3}: ", c);

                for (int d = 0; d < centers[c].Length; ++d)
                {
                    Console.Write("{0, -14} ", centers[c][d].ToString(CultureInfo.InvariantCulture));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void ParseInputFile(PocketData[] pockets)
        {
            using(TextReader r = new StreamReader(File.Open(_cmdLine.InputFile, FileMode.Open)))
            {
                int valuesCount = 0;
                for (; ; )
                {
                    string line = r.ReadLine();
                    if(line == null)
                    {
                        break;
                    }
                    if(Regex.IsMatch(line, "^\\s*#") || Regex.IsMatch(line, "^\\s*$"))
                    {
                        // Skip comments and whitespace
                        continue;
                    }
                    int offset = _cmdLine.SkipPocketNames ? 1 : 0;

                    string[] textValues = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (textValues.Length - offset != _cmdLine.Dim)
                    {
                        throw new ApplicationException(string.Format("Wrong number of values in line '{0}', expected: {1}, was: {2}", line, _cmdLine.Dim, textValues.Length));
                    }
                    for (int d = 0; d < _cmdLine.Dim; ++d)
                    {
                        pockets[valuesCount].Value[d] = double.Parse(textValues[d + offset], CultureInfo.InvariantCulture);
                    }
                    valuesCount++;
                }
                if(valuesCount != HePocket.Count)
                {
                    throw new ApplicationException(string.Format("Wrong number of values, expected: {0}, was: {1}", HePocket.Count, valuesCount));
                }
            }
        }
    }
}
