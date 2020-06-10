/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.lib.algorithms.numbers;
using ai.pkr.holdem.strategy.core;

namespace ai.pkr.holdem.learn
{
    /// <summary>
    /// Estimates a probability that any of multiple opponents holding a given card. See Compute().
    /// </summary>
    public static class MultiplayerPocketProbability
    {
        /// <summary>
        /// Computes a probability that any of opponents have been dealt a given pocket.
        /// The following assumptions are used:
        /// <para> Assume that all the opponents play as a team knowing each others pockets. 
        /// They will keep only one pocket (most likely the best) and throw away the rest. 
        /// This will be done by comparing the 2 first cards and keeping one of them with the probability
        /// given by the preference matrix, then comparing this card with the next one, etc.</para>
        /// <para>Assume that the probability of each opponent is dealt a given pocket is given by the card probability table 
        /// independent on each other. For example, if one can have AA with a probability of pAA, all 9 opponent can 
        /// simultaneously have AA with pAA^9, although this is impossible in real life.</para>
        /// </summary>
        /// <param name="oppCount"> Number of opponents.</param>
        /// <param name="cardProbabs">Probability of being dealt a given card.</param>
        /// <param name="preferenceMatrix">PT[i, j] gives the probability of preferring card i to card j. 
        /// The following constraints must be fulfilled: PT[i,j] &gt;= 0, PT[i,j] &lt;= 1, PT[i,j] + PT[j,i] == 1.</param>
        /// <returns>An array with card probability distribution.</returns>
        public static double[] Compute(int oppCount, double[] cardProbabs, double[,] preferenceMatrix)
        {
            Verify(oppCount, cardProbabs, preferenceMatrix);
            // For one opponent the resulting distribution equalt to the deal distribution.
            double[] result = cardProbabs.ShallowCopy();

            // Simulate card selection procedure by dealing cards to the rest of the opponents.
            for(int p = 1; p < oppCount; ++p)
            {
                result = Deal(result, cardProbabs, preferenceMatrix);
            }
            return result;
        }

        /// <summary>
        /// Computes a preference matrix with preference probability based on pocket equity 
        /// given by PocketEquity class.
        /// </summary>
        public static double[,] ComputePreferenceMatrixPe(HePocketKind[] pockets)
        {
            int n = pockets.Length;
            double[] cardProbabs = PocketHelper.GetProbabDistr(pockets);
            double[,] ptEq = new double[n, n];
            for (int i = 0; i < n; ++i)
            {
                HePocketKind p1 = pockets[i];
                for (int j = 0; j <= i; ++j)
                {
                    HePocketKind p2 = pockets[j];
                    PocketEquity.Result r = PocketEquity.CalculateFast(p1, p2);
                    ptEq[i, j] = r.Equity;
                    ptEq[j, i] = 1 - ptEq[i, j];
                }
            }
            return ptEq;
        }

        /// <summary>
        /// Computes a preference matrix with preference probability based on maximum of pocket equity 
        /// for given pairs (0 or 1). Pocket equity is given by PocketEquity class.
        /// </summary>
        public static double[,] ComputePreferenceMatrixPeMax(HePocketKind [] pockets)
        {
            int n = pockets.Length;
            double[] cardProbabs = PocketHelper.GetProbabDistr(pockets);
            double[,] ptMax = new double[n, n];
            for (int i = 0; i < n; ++i)
            {
                HePocketKind p1 = pockets[i];
                for (int j = 0; j <= i; ++j)
                {
                    HePocketKind p2 = pockets[j];
                    PocketEquity.Result r = PocketEquity.CalculateFast(p1, p2);
                    if (i == j || r.Equity == 0.5)
                    {
                        ptMax[i, j] = 0.5;
                    }
                    else
                    {
                        ptMax[i, j] = r.Equity > 0.5 ? 1 : 0;
                    }
                    ptMax[j, i] = 1 - ptMax[i, j];
                }
            }
            return ptMax;
        }

        static double[] Deal(double[] currentProbabs, double[] dealProbabs, double[,] pm)
        {
            int n = dealProbabs.Length;
            double[] result = new double[n];
            for (int c1 = 0; c1 < n; ++c1)
            {
                for (int c2 = 0; c2 < n; ++c2)
                {
                    // Probability of all previous opponents selected c1 and 
                    // the next opponent is dealt c2
                    double situationProbab = currentProbabs[c1] * dealProbabs[c2];
                    // Probability that the opponents will prefer c1 to c2
                    double prefC1 = pm[c1, c2];
                    result[c1] += situationProbab * prefC1;
                    result[c2] += situationProbab * (1 - prefC1);
                }
            }
            return result;
        }


        static void Verify(int oppCount, double[] cardProbabs, double[,] pm)
        {
            if(oppCount <= 0)
            {
                throw new ApplicationException("Player count must be > 0");
            }
            int n = cardProbabs.GetLength(0);
            if(n != pm.GetLength(0) || n != pm.GetLength(1))
            {
                throw new ApplicationException("Array size mismatch");
            }
            double sum = 0;
            for (int i = 0; i < n; ++i)
            {
                sum += cardProbabs[i];
            }
            if(!FloatingPoint.AreEqual(1, sum, 1e-5))
            {
                throw new ApplicationException(String.Format("Wrong sum of card probabilities: {0}, expected 1.", sum));
            }
            for(int i = 0; i < n; ++i)
            {
                for(int j = 0; j <= i; ++j)
                {
                    if(pm[i, j] < 0 || pm[i, j] > 1 || 
                        pm[j, i] < 0 || pm[j, i] > 1 || 
                        !FloatingPoint.AreEqual(pm[i, j] + pm[j, i], 1, 1e-5))
                    {
                        throw new ApplicationException(String.Format("Prefernce matrix wrong at ({0}, {1}).", i, j));
                    }
                }
            }
        }
    }
}
