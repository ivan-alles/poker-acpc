/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.Reflection;
using System.IO;

namespace ai.lib.kmeans.nunit
{
    /// <summary>
    /// Unit tests for Kml. 
    /// </summary>
    [TestFixture]
    public unsafe class Kml_Test
    {
        #region Tests

        [Test]
        public void Test_Hybrid()
        {
            Kml.Init(Path.GetDirectoryName(CodeBase.Get(Assembly.GetExecutingAssembly())));

            Kml.Parameters p = new Kml.Parameters();

            try
            {
                p.n = 20;
                p.k = 4;
                p.dim = 2;

                p.term_st_a = 50;
                p.term_st_b = p.term_st_c = p.term_st_d = 0;
                p.term_minConsecRDL = 0.2;
                p.term_minAccumRDL = 0.1;
                p.term_maxRunStage = 100;
                p.term_initProbAccept = 0.50;
                p.term_tempRunLength = 10;
                p.term_tempReducFact = 0.75;
                p.seed = 4;

                p.Allocate();

                for (int i = 0; i < p.n; ++i)
                {
                    for (int d = 0; d < p.dim; ++d)
                    {
                        *p.GetPoint(i, d) = _data1[i * p.dim + d];
                    }
                }
                Kml.KML_Hybrid(&p);

                p.PrintCenters(Console.Out);

                VerifyResult(p, _data1, _data1_expCenters, _data1_expCenterAssignments);
            }
            finally
            {
                p.Free();
            }
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        /// <summary>
        /// 2-d data points.
        /// </summary>
        double[] _data1 = new double[]
        {
          -0.29,   0.17,
          0.56,  -0.36,
           0.90,  -0.18,
           0.92,   0.47,
           0.16,   0.04,
           0.30,  -0.08,
           0.10,   0.88,
           0.74,   0.16,
           0.07,  -0.82,
           0.60,  -0.55,
           0.18,  -0.64,
           0.74,  -0.94,
          -0.97,  -0.22,
           0.91,   0.87,
           0.92,  -0.38,
          -0.71,   0.27,
          -0.51,   0.98,
           0.13,   0.92,
          -0.08,   0.61,
           0.05,   0.89
        };

        double[] _data1_expCenters = new double[]
        {
            0.4922, -0.4344,
            -0.062, 0.856,
            0.8567, 0.5, 
            -0.6567, 0.07333 
        };

        int[] _data1_expCenterAssignments = new int[]
        { 
           3,   
           0,   
           0,   
           2,   
           0,   
           0,   
           1,   
           2,   
           0,   
           0,   
           0,   
           0,   
           3,   
           2,   
           0,   
           3,   
           1,   
           1,   
           1,   
           1   
        };

        private void VerifyResult(Kml.Parameters p, double[] points, double[] expCenters, int[] expCenterAssignments)
        {
            Assert.AreEqual(expCenters.Length / p.dim, p.k);
            for (int i = 0; i < p.k; ++i)
            {
                for(int d = 0; d < p.dim; ++d)
                {
                    Assert.AreEqual(expCenters[i * p.dim + d], *p.GetCenter(i, d), 0.001);
                }
            }
            Assert.AreEqual(expCenterAssignments.Length, p.n);
            int[] centerAssigments = p.CalculateCenterAssignments();
            Assert.AreEqual(centerAssigments.Length, p.n);

            for (int i = 0; i < p.n; ++i)
            {
                Assert.AreEqual(expCenterAssignments[i], centerAssigments[i], i.ToString());
            }
        }

        #endregion
    }
}
