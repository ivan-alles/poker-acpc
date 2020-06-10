/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.holdem.strategy.core;
using ai.pkr.stdpoker;

namespace ai.pkr.holdem.learn.nunit
{
    /// <summary>
    /// Unit tests for MultiplayerPocketProbability. 
    /// </summary>
    [TestFixture]
    public class MultiplayerPocketProbability_Test
    {
        #region Tests

        /// <summary>
        /// Use manually calculated data for verification.
        /// </summary>
        [Test]
        public void Test_1()
        {
            double[] cardProbabs = new double[] {0.7, 0.3};
            double[,] preferenceTable = new double[2,2] {{0.5, 0.1}, {0.9, 0.5}};
            double[] result = MultiplayerPocketProbability.Compute(2, cardProbabs, preferenceTable);
            VerifyResult(cardProbabs.Length, result);
            Assert.AreEqual(result[0], 0.532, 1e-10);
            Assert.AreEqual(result[1], 0.468, 1e-10);

            cardProbabs = new double[] { 0.3, 0.7 };
            preferenceTable = new double[2, 2] { { 0.5, 0.9 }, { 0.1, 0.5 } };
            result = MultiplayerPocketProbability.Compute(2, cardProbabs, preferenceTable);
            VerifyResult(cardProbabs.Length, result);
            Assert.AreEqual(result[1], 0.532, 1e-10);
            Assert.AreEqual(result[0], 0.468, 1e-10);
        }

        [Test]
        [Explicit]
        public void Test_HePockets()
        {
            double[] cardProbabs = new double[169];
            for (int i = 0; i < 169; ++i)
            {
                cardProbabs[i] = HePocket.KindToRange((HePocketKind) i).Length/1326.0;
            }
            double[,] ptEq = MultiplayerPocketProbability.ComputePreferenceMatrixPe(PocketHelper.GetAllPockets());
            double[,] ptMax = MultiplayerPocketProbability.ComputePreferenceMatrixPeMax(PocketHelper.GetAllPockets());
            double[][] resEq = new double[10][];
            double[][] resMax = new double[10][];
            for (int pc = 1; pc < 10; ++pc)
            {
                resEq[pc] = MultiplayerPocketProbability.Compute(pc, cardProbabs, ptEq);
                VerifyResult(169, resEq[pc]);
                resMax[pc] = MultiplayerPocketProbability.Compute(pc, cardProbabs, ptMax);
                VerifyResult(169, resMax[pc]);
            }
            Console.WriteLine();
            Console.Write("{0,3} ", "Poc");
            for (int pc = 1; pc < 10; ++pc)
            {
                Console.Write("{0,6} {1,6} ", pc.ToString() + " eq", pc.ToString() + " max");
            }
            Console.WriteLine();
            for (int i = 0; i < 169; ++i)
            {
                HePocketKind p = (HePocketKind) i;
                Console.Write("{0,3} ", HePocket.KindToString(p));
                for (int pc = 1; pc < 10; ++pc)
                {
                    Console.Write("{0,6:0.00} {1,6:0.00} ", resEq[pc][i] * 100, resMax[pc][i]*100);
                }
                Console.WriteLine();
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        void VerifyResult(int expSize, double[] result)
        {
            Assert.AreEqual(expSize, result.Length);
            double sum = 0;
            for (int i = 0; i < result.Length; ++i)
            {
                sum += result[i];
            }
            Assert.AreEqual(1, sum, 1e-10);

        }

        #endregion
    }
}
