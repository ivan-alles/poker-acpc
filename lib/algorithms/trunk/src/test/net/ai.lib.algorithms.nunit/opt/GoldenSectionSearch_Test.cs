/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.algorithms.opt.nunit
{
    /// <summary>
    /// Unit tests for GoldenSectionSearch. 
    /// </summary>
    [TestFixture]
    public class GoldenSectionSearch_Test
    {
        #region Tests

        [Test]
        public void Test_Sin()
        {
            GoldenSectionSearch gss = new GoldenSectionSearch { F = x => -Math.Sin(x) };
            double minX = gss.Solve(0, Math.PI, 1e-5, true);
            Assert.AreEqual(Math.PI / 2, minX, 1e-5);

            minX = gss.Solve(-0.1, Math.PI, 1e-5, true);
            Assert.AreEqual(Math.PI / 2, minX, 1e-5);
        }

        [Test]
        public void Test_Abs()
        {
            GoldenSectionSearch gss = new GoldenSectionSearch { F = x => Math.Abs(x) };
            double minX = gss.Solve(-10, 1, 1e-5, false);
            Assert.AreEqual(0, minX, 1e-5);
        }

        [Test]
        public void Test_WrongShape()
        {
            bool errorOcurred = false;
            try
            {
                GoldenSectionSearch gss = new GoldenSectionSearch { F = x => Math.Sin(x) };
                gss.Solve(0, Math.PI, 1e-5, true);
            }
            catch (ApplicationException e)
            {
                errorOcurred = true;
            }
            Assert.IsTrue(errorOcurred);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
