using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* Cards: player 0 receives J or K with 50% probability, player 1 always has Q. K beats Q, Q beats J.
 * 
 * Game tree (blind and raise sizes are variable):
 * 
 *                                        0p1 
 *                                         |
 *                                        1p1
 *                                         /\
 *                                    0dJ /  \  0dK
 *                                       /    \
 *                           ------------      -----------
 *                          /\                           /\                  
 *                      0p2/  \ 0p4                  0p2/  \ 0p4             
 *                        /    \                       /    \                
 *                       /      \                     /      \
 *                      /        \                   /        \
 *                     /         /\                 /         /\
 *                     |        /  \                |        /  \ 
 *                  1p2|    1p0/    \ 1p4        1p2|    1p0/    \ 1p4
 */


namespace fictitious_play
{

    class Solver
    {
        public void Step()
        {
            Hero = (Hero + 1) % 2;
            ItCount[Hero]++;

            P0Card = _rng.Next(2);
            SdFold[P0Card]++;
            SdCall[P0Card] += P0Card == 0 ? 1 : -1;

            // Pot shares
            double psR1 =  Blind + Raise1;
            double psR2F = Blind;
            double psR2C = Blind + Raise2;

            if (Hero == 0)
            {
                if (!UpdateStrategyForSampledCardOnly || P0Card == 0)
                {
                    P0GvJ[0] = - SdCall[0] * psR1;
                    P0GvJ[1] = SdFold[0] * psR2F * P1Str[0] - SdCall[0] * psR2C * P1Str[1];
                    Br(P0GvJ, P0BrJ);
                    UpdateStrategy(P0BrJ, P0GvJ, P0StrJ, ItCount[Hero]);
                }

                if (!UpdateStrategyForSampledCardOnly || P0Card == 1)
                {
                    P0GvK[0] = -SdCall[1] * psR1;
                    P0GvK[1] = SdFold[1] * psR2F * P1Str[0] - SdCall[1] * psR2C * P1Str[1];
                    Br(P0GvK, P0BrK);
                    UpdateStrategy(P0BrK, P0GvK, P0StrK, ItCount[Hero]);
                }

                GameValues[Hero] = P0GvJ[0] * P0StrJ[0] + P0GvJ[1] * P0StrJ[1] + 
                                   P0GvK[0] * P0StrK[0] + P0GvK[1] * P0StrK[1];
            }
            else
            {
                P1GvR1 = psR1 * (SdCall[0] * P0StrJ[0] + SdCall[1] * P0StrK[0]);
                P1GvR2[0] = -psR2F * (SdFold[0]*P0StrJ[1] + SdFold[1]*P0StrK[1]);
                P1GvR2[1] = psR2C * (SdCall[0]*P0StrJ[1] + SdCall[1]*P0StrK[1]);

                Br(P1GvR2, P1BrR2);
                UpdateStrategy(P1BrR2, P1GvR2, P1Str, ItCount[Hero]);

                GameValues[Hero] = P1GvR1 + P1GvR2[0] * P1Str[0] + P1GvR2[1] * P1Str[1];
            }
        }

        public const double Blind = 1;
        public const double Raise1 = 2;
        public const double Raise2 = 4;

        public Random _rng = new Random(1);

        /// <summary>
        /// Last MC-card of p0 (0 - J, 1 - K).
        /// </summary>
        int P0Card = 9;
        public int[] ItCount = new int[2];
        public double[] GameValues = new double[2];
        public int[] P0BrJ = new int[2];
        public int[] P0BrK = new int[2];
        public double[] P0GvJ = new double[2];
        public double[] P0GvK = new double[2];
        public double[] P0StrJ = new double[2];
        public double[] P0StrK = new double[2];
        public double P1GvR1;
        public double[] P1GvR2 = new double[2];
        public int[] P1BrR2 = new int[2];
        public double[] P1Str = new double[2];
        // Showdown result Q vs J and K for fold (just a counter)
        public double[] SdFold = new double[2];
        // Showdown result Q vs J and K for fold (point of view of player 1)
        public double[] SdCall = new double[2];
        int Hero;

        bool UpdateStrategyForSampledCardOnly = false;

        public void Init()
        {
            // Start from p0, p1 always call strategy
            P0StrJ = new double[]{1,0};
            P0StrK = new double[]{1,0};
            P1Str = new double[]{0,1};
            ItCount = new int[]{1,1};
            Hero = 1;
        }

        public void PrintHeader()
        {
            Console.WriteLine("It  0 C   GvJ0   GvJ1 BrJ0 BrJ1  StrJ0  StrJ1   GvK0   GvK1 BrK0 BrK1  StrK0  StrK1     Gv      1   GvR1  GvR20  GvR21 BrR20 BrR21 StrR20 StrR21     Gv");
        }

        public void Print()
        {
            Console.Write("{0,4}", ItCount[0] + ItCount[1]);
            Console.Write("{0} {1} {2,6:0.000} {3,6:0.000} {4,4} {5,4} {6,6:0.000} {7,6:0.000} {8,6:0.000} {9,6:0.000} {10,4} {11,4} {12,6:0.000} {13,6:0.000} {14,6:0.000}",
                Hero == 0 ? "*" : " ", P0Card, P0GvJ[0], P0GvJ[1], P0BrJ[0], P0BrJ[1], P0StrJ[0], P0StrJ[1], P0GvK[0], P0GvK[1], P0BrK[0], P0BrK[1], P0StrK[0], P0StrK[1], GameValues[0]);
            Console.Write("      {0} {1,6:0.000} {2,6:0.000} {3,6:0.000}  {4,4}  {5,4} {6,6:0.000} {7,6:0.000} {8,6:0.000}", 
                Hero == 1 ? "*" : " ", P1GvR1, P1GvR2[0], P1GvR2[1], P1BrR2[0], P1BrR2[1], P1Str[0], P1Str[1], GameValues[1]);
            Console.Write(" {0}", GameValues[0] + GameValues[1]);
            Console.WriteLine();
        }

        void UpdateStrategy(int [] br, double [] gv, double [] str, int itCount)
        {
            for (int i = 0; i < br.Length; ++i)
            {
                str[i] *= (itCount - 1);
                str[i] += br[i];
                str[i] /= itCount;
            }
        }

        void Br(double [] v, int[] br)
        {
            if (v[0] > v[1])
            {
                br[0] = 1;
                br[1] = 0;
            }
            else
            {
                br[0] = 0;
                br[1] = 1;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Solver solver = new Solver();
            solver.Init();
            for (int step = 0; step < 200000; ++step)
            {
                if (step % 20 == 0)
                {
                    solver.PrintHeader();
                }
                solver.Print();
                solver.Step();
            }
            solver.Print();
            Console.WriteLine("MC Counters {0} {1}", solver.SdFold[0], solver.SdFold[1]);

        }
    }
}
