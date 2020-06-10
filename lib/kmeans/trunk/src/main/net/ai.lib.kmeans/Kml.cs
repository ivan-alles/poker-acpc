/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ai.lib.utils;
using System.Reflection;
using System.IO;

namespace ai.lib.kmeans
{
    /// <summary>
    /// A wrapper for KML library.
    /// </summary>
    public unsafe class Kml
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Parameters
        {
            #region Input 

            // number of centers
            public int k;

            // dimension
            public int dim;


            // Point coordinates for all dimensions. For example, for N points and dim = 2:
            // (p0,0 p0,1) (p1,0 p1,1) ... (pN-1,0 pN-1,1)
            public double* points;

            // Number of points
            public int n;

            /// <summary>
            /// Max stages to run (e.g. 100 0 0 0). 
            /// </summary>
            public double term_st_a, term_st_b, term_st_c, term_st_d;

            
            public double term_minConsecRDL;

            public double term_minAccumRDL;

            public int term_maxRunStage;

            public double term_initProbAccept;

            public int term_tempRunLength;

            public double term_tempReducFact;

            /// <summary>
            /// RNG seed, must be a positive number.
            /// </summary>
            public int seed;

            #endregion

            #region  Output

            // Centers coordinates for all dimensions. Layout as in Parameters.
            public double* centers;

            #endregion

            #region Methods

            public void SetDefaultTerm()
            {
                // Copied from kmlsample.
                term_st_a = 100;
                term_st_b = term_st_c = term_st_d = 0;
                term_minConsecRDL = 0.1;
                term_minAccumRDL = 0.1;
                term_maxRunStage = 3;
                term_initProbAccept = 0.50;
                term_tempRunLength = 10;
                term_tempReducFact = 0.95;
            }

            public double* GetPoint(int i, int d)
            {
                return points + i * dim + d;
            }

            public double* GetCenter(int i, int d)
            {
                return centers + i * dim + d;
            }

            public double CalculateSqaredDist(int i, int c)
            {
                double dist = 0;
                for (int d = 0; d < dim; ++d)
                {
                    double coord_dist = *GetPoint(i, d) - *GetCenter(c, d);
                    dist += coord_dist * coord_dist;
                }
                return dist;
            }

            public int[] CalculateCenterAssignments()
            {
                int[] ca = new int[n];

                for (int i = 0; i < n; ++i)
                {
                    double minDist = double.MaxValue;
                    int center = -1;
                    for (int c = 0; c < k; ++c)
                    {
                        double dist = CalculateSqaredDist(i, c);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            center = c;
                        }
                    }
                    ca[i] = center;
                }
                return ca;
            }

            public void PrintCenters(TextWriter tw)
            {
                Console.WriteLine("Centers:");
                for (int c = 0; c < k; ++c)
                {
                    for (int d = 0; d < dim; ++d)
                    {
                        tw.Write("{0:0.0000} ", *GetCenter(c, d));
                    }
                    tw.WriteLine();
                }
            }


            /// <summary>
            /// Allocate memory for arrays. Must be called when n, dim and k are set.
            /// </summary>
            public void Allocate()
            {
                points = (double*)UnmanagedMemory.AllocHGlobalEx(n * dim * 8);
                centers = (double*)UnmanagedMemory.AllocHGlobalEx(k * dim * 8);
            }

            public void Free()
            {
                UnmanagedMemory.FreeHGlobal(points);
                points = null;
                UnmanagedMemory.FreeHGlobal(centers);
                centers = null;
            }

            #endregion
        };


        [DllImport("ai.lib.kmeans.kml.dll")]
        public static extern int KML_Hybrid(Parameters * p);

        /// <summary>
        /// Prepares the dll to load.
        /// </summary>
        /// <param name="dir">Directory where to search for dll (without plattform subdir), 
        /// default: ${bds.BinDir}</param>
        public static void Init(string dir)
        {
            string platform = System.IntPtr.Size == 8 ? "win64" : "win32";
            if (string.IsNullOrEmpty(dir))
            {
                dir = Props.Global.Get("bds.BinDir");
            }
            string dllDir = Path.Combine(dir,  platform);

            string dllName = "ai.lib.kmeans.kml.dll";

            string dllPath = Path.Combine(dllDir, dllName);

            if (!System.IO.File.Exists(dllPath))
            {
                throw new ApplicationException(string.Format("Cannot load {0}", dllPath));
            }
            string envPath = Environment.GetEnvironmentVariable("PATH");
            string envPathL = envPath.ToLower() + ";";
            if (envPathL.IndexOf(dllDir.ToLower() + ";") < 0)
            {
                Environment.SetEnvironmentVariable("PATH", dllDir + ";" + envPath, EnvironmentVariableTarget.Process);
            }
        } 


    }
}

