using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        public const double Blind = 1;
        public const double Raise1 = 2;
        public const double Raise2 = 4;
        /*
           Pure strategies:
           P0: 0 - Jc, Kc, 1 - Jr, Kc, 2 - Jc, Kr, 3 - Jr, Kr.
           P1: 0 - Qf, 1 - Qc.
         */

        /// <summary>
        /// Payouff matrix for player 0.
        /// </summary>
        public double[,] Payoff0 = new double[4, 2]
                                        {
                                            {- 3 + 3, - 3 + 3},
                                            {+ 1 + 3, - 5 + 3},
                                            {- 3 + 1, - 3 + 5},
                                            {+ 1 + 1, - 5 + 5}
                                        };

        public double[,] Payoff1;


        /// <summary>
        /// Regret matrix for player 0.
        /// </summary>
        public double[,] Regret0, Regret1;

        private double[,] Str0 = new double[1, 4] { {1, 0, 0, 0} };
        private double[,] Str1 = new double[1, 2] { {1, 0}};

        public double Epsilon = 0.001;

        double [,] Mul(double [,]  a, double [,] b)
        {
            if(a.Rank != 2 || b.Rank != 2)
            {
                throw new Exception("Wrong dimension");
            }
            if(a.GetLength(1) != b.GetLength(0))
            {
                throw new Exception("Matrix size does not match");
            }
            double[,] r = new double[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < r.GetLength(0); ++i)
            {
                for (int j = 0; j < r.GetLength(1); ++j)
                {
                    for (int k = 0; k < a.GetLength(1); ++k)
                    {
                        r[i, j] += a[i, k]*b[k, j];
                    }
                }
            }
            return r;
        }

        double [,] Transpose(double [,]  a)
        {
            double[,] r = new double[a.GetLength(1),a.GetLength(0)];
            for (int i = 0; i < a.GetLength(0); ++i)
            {
                for (int j = 0; j < a.GetLength(1); ++j)
                {
                    r[j, i] = a[i, j];
                }
            }
            return r;
        }

        double[,] Negate(double[,] a)
        {
            double[,] r = new double[a.GetLength(0), a.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); ++i)
            {
                for (int j = 0; j < a.GetLength(1); ++j)
                {
                    r[i, j] = -a[i, j];
                }
            }
            return r;
        }

        double[,] CalcRegret(double[,] a)
        {
            double[,] r = new double[a.GetLength(0), a.GetLength(1)];
            for (int j = 0; j < a.GetLength(1); ++j)
            {
                double max = -10000;
                for (int i = 0; i < a.GetLength(0); ++i)
                {
                    max = Math.Max(max, a[i, j]);
                }
                for (int i = 0; i < a.GetLength(0); ++i)
                {
                    r[i, j] = a[i, j] - max;
                }
            }
            return r;
        }

        void PrintMatrix(double[,] a)
        {
            for (int i = 0; i < a.GetLength(0); ++i)
            {
                Console.WriteLine();
                for (int j = 0; j < a.GetLength(1); ++j)
                {
                    Console.Write("{0,7:0.000}", a[i, j]);
                }
            }
            Console.WriteLine();
        }

        bool IsZero(double [] v)
        {
            for(int i = 0; i < v.Length; ++i)
            {
                if (v[i] != 0)
                    return false;
            }
            return true;
        }

        public void StepFictPlay()
        {
            Hero = (Hero + 1) % 2;
            ItCount[Hero]++;

            double[,] A;
            double[,] h, o;

            if (Hero == 0)
            {
                A = Payoff0;
                h = Str0;
                o = Str1;
                
            }
            else
            {
                A = Payoff1;
                h = Str1;
                o = Str0;
            }

            double[,] v = Mul(A, Transpose(o));
            double[,] br = new double[h.GetLength(1), 1];
            Br(v, br);
            UpdateStrategy(br, h, ItCount[Hero]);
        }

        public void StepFictPlayRegret()
        {
            Hero = (Hero + 1) % 2;
            ItCount[Hero]++;

            double[,] A;
            double[,] h, o;

            if (Hero == 0)
            {
                A = Regret0;
                h = Str0;
                o = Str1;

            }
            else
            {
                A = Regret1;
                h = Str1;
                o = Str0;
            }

            double[,] v = Mul(A, Transpose(o));
            double[,] br = new double[h.GetLength(1), 1];
            Br(v, br);
            UpdateStrategy(br, h, ItCount[Hero]);
        }

        public void StepSimplex(int hero)
        {
            Hero = hero;
            ItCount[Hero]++;

            double[,] A;
            double[,] h, o;

            if (Hero == 0)
            {
                A = Payoff1;
                h = Str0;
                o = Str1;

            }
            else
            {
                A = Payoff0;
                h = Str1;
                o = Str0;
            }

            double[,] vo = Mul(A, Transpose(h));
            // vo contains values for each strategy for opponent
            // opponent will maximize them, so find the max
            // and reduce it
            int maxI = -1;
            double max = double.NegativeInfinity;
            for(int i = 0; i < vo.GetLength(0); ++i)
            {
                if(vo[i, 0] > max)
                {
                    max = vo[i, 0];
                    maxI = i;
                }
            }
            // Then row A(maxI) contains the gradient.
            // Use 0-th element as the dependent variable.
            double[] grad = new double[A.GetLength(1)];
            for(int i = 0; i < A.GetLength(1); ++i)
            {
                grad[i] = A[maxI, i] + A[maxI, 0];
            }
            grad[0] = 0;

            // Find a non-zero element of the gradient allowing to 
            // minimize the value for the opponent.
            int changeI = -1;
            double t = 0;
            for(int i = 1; i < A.GetLength(1); ++i)
            {
                if (grad[i] == 0)
                {
                    continue;
                }
                double v = h[0, i];
                t = 0;
                if(grad[i] < 0)
                {
                    // Go with positive t to minimize opp value
                    t = 1 - v;
                }
                else if(grad[i] > 0)
                {
                    // Go with negative t to minimize opp value
                    t = -v;
                }
                if (t == 0)
                {
                    continue;
                }
                for(int j = 0; j < A.GetLength(0); ++j)
                {
                    if(j == maxI)
                    {
                        continue;
                    }
                    double k = A[j, i] + A[j, 0];
                    double dSpeed = (k - grad[i]);
                    double dDist = (vo[maxI, 0] - vo[j, 0]);
                    double t1 = t;
                    if ((grad[i] > 0 && k < 0) || (grad[i] < 0 && k > 0))
                    {
                        // Opposite speeds
                        // Calculate the time when the two values meet.
                        // If they are already equal, this is a no-go to (t will be set to 0).
                        t1 = dDist / dSpeed;
                        Debug.Assert((t < 0 && t1 <= 0) || (t > 0 && t1 >= 0));
                        if(t > 0)
                        {
                            t = Math.Min(t, t1);
                        }
                        else
                        {
                            t = Math.Max(t, t1);
                        }
                    }
                    if(t != 0)
                    {
                        break;
                    }
                }
                if (t != 0)
                {
                    changeI = i;
                    break;
                }
            }
            if (changeI == -1)
            {
                throw new Exception("Nowhere to go");
            }
            h[0, changeI] += t;
            h[0, 0] -= t;
        }

        public int[] ItCount = new int[2];
        int Hero;

        public void Init()
        {
            ItCount = new int[]{1,1};
            Hero = 1;
            Payoff1 = Transpose(Negate(Payoff0));
            Regret0 = CalcRegret(Payoff0);
            Regret1 = CalcRegret(Payoff1);
        }

        public void Print()
        {
            double gv0 = Mul(Str0, Mul(Payoff0, Transpose(Str1)))[0, 0];
            double gv1 = Mul(Str1, Mul(Payoff1, Transpose(Str0)))[0, 0];
            Console.Write(
                "{0,4} {1,4:0.000} {2,4:0.000} {3,4:0.000} {4,4:0.000} {5,4:0.000} {6,4} {7,4:0.000} {8,4:0.000} {9,4:0.000}",
                ItCount[0], Str0[0, 0], Str0[0, 1], Str0[0, 2], Str0[0, 3], gv0,
                ItCount[1], Str1[0, 0], Str1[0, 1], gv1
                );
            Console.WriteLine();
        }

        public void PrintMatrices()
        {
            Console.Write("Payoff 0");
            PrintMatrix(Payoff0);

            Console.Write("Payoff 1");
            PrintMatrix(Payoff1);

            Console.Write("Regret 0");
            PrintMatrix(Regret0);

            Console.Write("Regret 1");
            PrintMatrix(Regret1);
        }

        void UpdateStrategy(double [,] br, double [,] str, int itCount)
        {
            for (int i = 0; i < br.GetLength(0); i++)
            {
                str[0, i] *= (itCount - 1);
                str[0, i] += br[i, 0];
                str[0, i] /= itCount;
            }
        }

        void Br(double [, ] v, double[,] br)
        {
            double maxV = -100000;
            int maxI = -1;
            for (int i = 0; i < br.GetLength(0); i ++)
            {
                br[i, 0] = 0;
                if (v[i, 0] > maxV)
                {
                    maxV = v[i, 0];
                    maxI = i;
                }
            }
            br[maxI, 0] = 1;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            Solver solver = new Solver();
            solver.Init();
            solver.PrintMatrices();

            for (int step = 0; step < 200; ++step)
            {
                solver.Print();
                solver.StepSimplex(0);
            }
            solver.Print();

        }
    }
}
