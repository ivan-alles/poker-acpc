/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.metagame.nunit
{
    /// <summary>
    /// Unit tests for Showdown. 
    /// </summary>
    [TestFixture]
    public class Showdown_Test
    {
        #region Tests

        [Test]
        public void Test_CalculateHi_2Players()
        {
            double[] inpot, result;
            UInt32[] ranks;

            // Same in-pot

            inpot = new double[] { 1, 1 };
            ranks = new UInt32[] { 1, 2 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 1 }, result);

            inpot = new double[] { 1, 1 };
            ranks = new UInt32[] { 2, 1 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 1, -1 }, result);

            inpot = new double[] { 1, 1 };
            ranks = new UInt32[] { 1, 1 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 0, 0 }, result);

            // Different in-pot

            inpot = new double[] { 1, 2 };
            ranks = new UInt32[] { 1, 2 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 1 }, result);

            inpot = new double[] { 2, 1 };
            ranks = new UInt32[] { 1, 2 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 1 }, result);

            inpot = new double[] { 1, 2 };
            ranks = new UInt32[] { 1, 1 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 0, 0 }, result);

            inpot = new double[] { 2, 1 };
            ranks = new UInt32[] { 1, 1 };
            result = new double[2];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 0, 0 }, result);
        }

        [Test]
        public void Test_CalculateHi_3Players()
        {
            double[] inpot, result;
            UInt32[] ranks;

            // Same in-pot

            inpot = new double[] { 1, 1, 1 };
            ranks = new UInt32[] { 1, 3, 2 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 2, -1 }, result);

            inpot = new double[] { 1, 1, 1 };
            ranks = new UInt32[] { 1, 3, 1 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 2, -1 }, result);

            inpot = new double[] { 1, 1, 1 };
            ranks = new UInt32[] { 1, 3, 3 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 0.5, 0.5 }, result);

            inpot = new double[] { 1, 1, 1 };
            ranks = new UInt32[] { 3, 3, 3 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 0, 0, 0 }, result);

            // Different in-pot
            inpot = new double[] { 1, 3, 1 };
            ranks = new UInt32[] { 1, 3, 2 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 2, -1 }, result);

            inpot = new double[] { 1, 3, 2 };
            ranks = new UInt32[] { 1, 3, 2 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 3, -2 }, result);

            inpot = new double[] { 1, 3, 3 };
            ranks = new UInt32[] { 1, 3, 2 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 4, -3 }, result);

            inpot = new double[] { 1, 3, 10 };
            ranks = new UInt32[] { 1, 3, 2 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -1, 4, -3 }, result);

            inpot = new double[] { 3, 3, 10 };
            ranks = new UInt32[] { 1, 3, 2 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -3, 6, -3 }, result);

            inpot = new double[] { 1, 3, 10 };
            ranks = new UInt32[] { 3, 3, 3 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 0, 0, 0 }, result);

            inpot = new double[] { 3, 3, 10 };
            ranks = new UInt32[] { 3, 3, 5 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -3, -3, 6 }, result);

            inpot = new double[] { 3, 3, 10 };
            ranks = new UInt32[] { 3, 3, 1 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 1.5, 1.5, -3 }, result);

            inpot = new double[] { 3, 0, 10 };
            ranks = new UInt32[] { 3, 0, 1 };
            result = new double[3];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { 3, 0, -3 }, result);
        }

        [Test]
        public void Test_CalculateHi_ManyPlayers()
        {
            double[] inpot, result;
            UInt32[] ranks;

            inpot = new double[] { 3, 7, 1, 8, 6, 1, 4 };
            ranks = new UInt32[] { 3, 2, 0, 4, 4, 0, 9 };
            result = new double[7];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -3, -7, -1, -2, -3, -1, 17 }, result);

            inpot = new double[] { 3, 7, 1, 8, 6, 0, 1, 4 };
            ranks = new UInt32[] { 3, 2, 0, 4, 4, 0, 0, 9 };
            result = new double[8];
            Showdown.CalcualteHi(inpot, ranks, result, 0);
            Assert.AreEqual(new double[] { -3, -7, -1, -2, -3, 0, -1, 17 }, result);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
