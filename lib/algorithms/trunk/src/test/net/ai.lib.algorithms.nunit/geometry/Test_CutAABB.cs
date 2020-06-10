/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.geometry;
using System.IO;
using ai.lib.utils;
using System.Reflection;
using System.Globalization;
using System.Threading;

namespace ai.lib.algorithms.geometry.nunit
{
    /// <summary>
    /// Unit tests for CutAABB. 
    /// </summary>
    [TestFixture]
    public class Test_CutAABB
    {
        #region Tests

        [Test]
        public void Test_3d()
        {
            DoTest("1.1", new double[] {0, 0, 0}, new double[] {1, 1, 1}, new double[] {1, 0, 0, 0},
                   true, new double[] {-0.5, 0, 0}, new double[] {0.5, 1, 1});
            DoTest("1.2", new double[] { 0, 0, 0 }, new double[] { 1, 1, 1 }, new double[] { 1, 0, 0, 1 },
               true, new double[] { -1, 0, 0 }, new double[] { 0, 1, 1 });
            DoTest("1.3", new double[] { 0, 0, 0 }, new double[] { 1, 1, 1 }, new double[] { 1, 0, 0, -1 },
               true, new double[] { 0, 0, 0 }, new double[] { 1, 1, 1 });
            DoTest("1.4", new double[] { 0, 0, 0 }, new double[] { 1, 1, 1 }, new double[] { 1, 0, 0, -2 },
               true, new double[] { 0, 0, 0 }, new double[] { 1, 1, 1 });
            DoTest("1.5", new double[] { 0, 0, 0 }, new double[] { 1, 1, 1 }, new double[] { 1, 0, 0, 1.5 },
                   false, null, null);

            
            DoTest("2.1", new double[] { 1, 2, 3 }, new double[] { 1.5, 2, 0.5 }, new double[] { 1, 0, 0, -1 },
               true, new double[] { 0.25, 2, 3 }, new double[] { 0.75, 2, 0.5 });
            DoTest("2.2", new double[] { 1, 2, 3 }, new double[] { 1.5, 2, 0.5 }, new double[] { 1, 0, 0, 0.5 },
               true, new double[] { -0.5, 2, 3 }, new double[] { 0, 2, 0.5 });
            DoTest("2.3", new double[] { 1, 2, 3 }, new double[] { 1.5, 2, 0.5 }, new double[] { 1, 0, 0, -3 },
               true, new double[] { 1, 2, 3 }, new double[] { 1.5, 2, 0.5 });
            DoTest("2.4", new double[] { 1, 2, 3 }, new double[] { 1.5, 2, 0.5 }, new double[] { 1, 0, 0, 1 },
               false, null, null);


            DoTest("3.1", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { 1, 0.5, 0.7, -2 },
                true, new double[] { 0.4, 1.8, 2 }, new double[] { 0.9, 1.8, 1 });
            DoTest("3.2", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { 1, 0.5, 0.7, -5 },
                true, new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 });
            DoTest("3.3", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { 1, 0.5, 0.7, -7 },
                true, new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 });
            DoTest("3.4", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { 1, 0.5, 0.7, 0 },
                false, null, null);

            DoTest("4.1", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { -1, 0.5, 0.7, 1 },
                true, new double[] { 2.1, 0.8, 1.5714285714285712 }, new double[] { 0.4, 0.8, 0.5714285714285714 });
            DoTest("4.2", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { -1, 0.5, 0.7, -3 },
                true, new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 });
            DoTest("4.3", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { -1, 0.5, 0.7, -5 },
                true, new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 });
            DoTest("4.4", new double[] { 1, 2, 2 }, new double[] { 1.5, 2, 1 }, new double[] { -1, 0.5, 0.7, 2 },
                false, null, null);

            // Minimal cut that results the same AABB as original.
            DoTest("5.1", new double[] { 1.5, 1, 0.5 }, new double[] { 1.5, 1, 0.5 }, new double[] { 1, 1.5, 3, -3 },
                true, new double[] { 1.5, 1, 0.5 }, new double[] { 1.5, 1, 0.5 });

        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        private double EPSILON = 0.0000000001;

        void DoTest(string testName, double [] c0, double [] s0, double [] p, bool expAreIntersecting, double[] expC1, double[] expS1)
        {
            Assert.AreEqual(c0.Length, s0.Length);

            bool areIntersecting;
            double[] c1 = new double[3];
            double[] s1 = new double[3];

            areIntersecting = CutAABB.Cut(c0, s0, p, c1, s1);

#if DEBUG
            if (c0.Length == 3)
            {
                Plot3d(c0, s0, p, c1, s1, areIntersecting, testName);
            }
#endif

            Assert.AreEqual(expAreIntersecting, areIntersecting);
            if (expAreIntersecting)
            {
                Assert.AreEqual(c0.Length, c1.Length);
                Assert.AreEqual(s0.Length, s1.Length);
                for (int i = 0; i < expC1.Length; ++i)
                {
                    Assert.AreEqual(expC1[i], c1[i], EPSILON);
                    Assert.AreEqual(expS1[i], s1[i], EPSILON);
                }
            }
            
        }

        void Plot3d(double [] c0, double [] s0, double [] p, double [] c1, double [] s1, bool areIntersecting, string testName)
        {
            double[] p1 = new double[3];
            double p_2 = p[2] == 0 ? 0.01 : p[2];
            p1[0] = -p[0] / p_2;
            p1[1] = -p[1] / p_2;
            p1[2] = -p[3] / p_2;

            string path = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "geometry/CutAABB");

            path = Path.Combine(path, testName+".plt");

            CultureInfo cultureInfoBackup = Thread.CurrentThread.CurrentCulture;
            // To format doubles, etc..
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            int samples = p[2] == 0 ? 131 : 31;

            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine("# Header begin (same for all files) ----------------------------");
                tw.WriteLine("");
                tw.WriteLine("");
                tw.WriteLine("set samples {0}, {0}", samples, samples);
                tw.WriteLine("set isosamples {0}, {0}", samples, samples);
                tw.WriteLine("");
                tw.WriteLine("set xlabel \"x0\" ");
                tw.WriteLine("set xlabel  offset character -3, -2, 0 font \"\" textcolor lt -1 norotate");
                tw.WriteLine("set xrange [ -2.0 : 5.0 ] noreverse nowriteback");
                tw.WriteLine("");
                tw.WriteLine("set ylabel \"x1\" ");
                tw.WriteLine("set ylabel  offset character 3, -2, 0 font \"\" textcolor lt -1 rotate by 90");
                tw.WriteLine("set yrange [ -2.0000 : 5.0000 ] noreverse nowriteback");
                tw.WriteLine("");
                tw.WriteLine("");
                tw.WriteLine("set zlabel \"x2\" ");
                tw.WriteLine("set zlabel  offset character -5, 0, 0 font \"\" textcolor lt -1 norotate");
                tw.WriteLine("set zrange [ -2.0000 : 5.0000 ] noreverse nowriteback");
                tw.WriteLine("");
                tw.WriteLine("");
                tw.WriteLine("set xyplane at 0");
                tw.WriteLine("");
                tw.WriteLine("set dummy x0, x1");
                tw.WriteLine("");
                tw.WriteLine("# To draw a cross in a center");
                tw.WriteLine("cdelta = 0.01");
                tw.WriteLine("");
                tw.WriteLine("# ----------------- AABB P0 --------------------");
                tw.WriteLine("");
                tw.WriteLine("s0_0 = {0}", s0[0]);
                tw.WriteLine("s0_1 = {0}", s0[1]);
                tw.WriteLine("s0_2 = {0}", s0[2]);
                tw.WriteLine("");
                tw.WriteLine("c0_0 = {0}", c0[0]);
                tw.WriteLine("c0_1 = {0}", c0[1]);
                tw.WriteLine("c0_2 = {0}", c0[2]);
                tw.WriteLine("");
                tw.WriteLine("set label \"c0_0\" at c0_0 + 2*cdelta, c0_1 + 2*cdelta, c0_2 + 2*cdelta");
                tw.WriteLine("");
                tw.WriteLine("# Center ");
                tw.WriteLine("set style line 1  linewidth 2");
                tw.WriteLine("set style arrow 1 nohead linestyle 1 linecolor rgb \"red\"");
                tw.WriteLine("set arrow from c0_0-cdelta, c0_1, c0_2 to c0_0+cdelta, c0_1, c0_2 arrowstyle 1");
                tw.WriteLine("set arrow from c0_0, c0_1-cdelta, c0_2 to c0_0, c0_1+cdelta, c0_2 arrowstyle 1");
                tw.WriteLine("set arrow from c0_0, c0_1, c0_2-cdelta to c0_0, c0_1, c0_2+cdelta arrowstyle 1");
                tw.WriteLine("");
                tw.WriteLine("");
                tw.WriteLine("# AABB");
                tw.WriteLine("set style line 2  linetype -1 linewidth 4");
                tw.WriteLine("set style arrow 2 nohead linestyle 2 linecolor rgb \"red\"");
                tw.WriteLine("");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1-s0_1, c0_2-s0_2 to c0_0+s0_0, c0_1-s0_1, c0_2-s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1-s0_1, c0_2-s0_2 to c0_0-s0_0, c0_1-s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1-s0_1, c0_2+s0_2 to c0_0+s0_0, c0_1-s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0+s0_0, c0_1-s0_1, c0_2-s0_2 to c0_0+s0_0, c0_1-s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1+s0_1, c0_2-s0_2 to c0_0+s0_0, c0_1+s0_1, c0_2-s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1+s0_1, c0_2-s0_2 to c0_0-s0_0, c0_1+s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1+s0_1, c0_2+s0_2 to c0_0+s0_0, c0_1+s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0+s0_0, c0_1+s0_1, c0_2-s0_2 to c0_0+s0_0, c0_1+s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1-s0_1, c0_2-s0_2 to c0_0-s0_0, c0_1+s0_1, c0_2-s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0+s0_0, c0_1-s0_1, c0_2-s0_2 to c0_0+s0_0, c0_1+s0_1, c0_2-s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0-s0_0, c0_1-s0_1, c0_2+s0_2 to c0_0-s0_0, c0_1+s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine(
                    "set arrow from c0_0+s0_0, c0_1-s0_1, c0_2+s0_2 to c0_0+s0_0, c0_1+s0_1, c0_2+s0_2 arrowstyle 2");
                tw.WriteLine("");

                if(areIntersecting)
                {
                    tw.WriteLine("# ----------------- AABB P1 --------------------");
                    tw.WriteLine("");
                    tw.WriteLine("s1_0 = {0}", s1[0]);
                    tw.WriteLine("s1_1 = {0}", s1[1]);
                    tw.WriteLine("s1_2 = {0}", s1[2]);
                    tw.WriteLine("");
                    tw.WriteLine("c1_0 = {0}", c1[0]);
                    tw.WriteLine("c1_1 = {0}", c1[1]);
                    tw.WriteLine("c1_2 = {0}", c1[2]);
                    tw.WriteLine("");
                    tw.WriteLine("set label \"c1_0\" at c1_0 + 2*cdelta, c1_1 + 2*cdelta, c1_2 + 2*cdelta");
                    tw.WriteLine("");
                    tw.WriteLine("# Center ");
                    tw.WriteLine("set style line 3  linetype -1 linewidth 1");
                    tw.WriteLine("set style arrow 3 nohead linestyle 3 linecolor rgb \"blue\"");
                    tw.WriteLine("set arrow from c1_0-cdelta, c1_1, c1_2 to c1_0+cdelta, c1_1, c1_2 arrowstyle 3");
                    tw.WriteLine("set arrow from c1_0, c1_1-cdelta, c1_2 to c1_0, c1_1+cdelta, c1_2 arrowstyle 3");
                    tw.WriteLine("set arrow from c1_0, c1_1, c1_2-cdelta to c1_0, c1_1, c1_2+cdelta arrowstyle 3");
                    tw.WriteLine("");
                    tw.WriteLine("");
                    tw.WriteLine("# AABB");
                    tw.WriteLine("set style line 4  linetype -1 linewidth 2");
                    tw.WriteLine("set style arrow 4 nohead linestyle 4 linecolor rgb \"blue\"");
                    tw.WriteLine("");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1-s1_1, c1_2-s1_2 to c1_0+s1_0, c1_1-s1_1, c1_2-s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1-s1_1, c1_2-s1_2 to c1_0-s1_0, c1_1-s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1-s1_1, c1_2+s1_2 to c1_0+s1_0, c1_1-s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0+s1_0, c1_1-s1_1, c1_2-s1_2 to c1_0+s1_0, c1_1-s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1+s1_1, c1_2-s1_2 to c1_0+s1_0, c1_1+s1_1, c1_2-s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1+s1_1, c1_2-s1_2 to c1_0-s1_0, c1_1+s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1+s1_1, c1_2+s1_2 to c1_0+s1_0, c1_1+s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0+s1_0, c1_1+s1_1, c1_2-s1_2 to c1_0+s1_0, c1_1+s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1-s1_1, c1_2-s1_2 to c1_0-s1_0, c1_1+s1_1, c1_2-s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0+s1_0, c1_1-s1_1, c1_2-s1_2 to c1_0+s1_0, c1_1+s1_1, c1_2-s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0-s1_0, c1_1-s1_1, c1_2+s1_2 to c1_0-s1_0, c1_1+s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine(
                        "set arrow from c1_0+s1_0, c1_1-s1_1, c1_2+s1_2 to c1_0+s1_0, c1_1+s1_1, c1_2+s1_2 arrowstyle 4");
                    tw.WriteLine("");
                }

                tw.WriteLine("# ----------------- Vector p(x) < 0 ----------------- ");
                tw.WriteLine("set style line 5  linetype -1 linewidth 2");
                tw.WriteLine("set style arrow 5 linestyle 5 linecolor rgb \"green\"");
                tw.WriteLine("");
                tw.WriteLine(
                    "set arrow from c0_0, c0_1, c0_2 to c0_0-{0}, c0_1-{1}, c0_2-{2} arrowstyle 5",
                    p[0],p[1],p[2]);
                tw.WriteLine("");

                tw.WriteLine("# ----------------- Plane --------------------");
                tw.WriteLine("");
                tw.WriteLine("cut(x0, x1) = {0}*x0 + {1}*x1 + {2}", p1[0], p1[1], p1[2]);
                tw.WriteLine("");
                tw.WriteLine("splot cut(x0, x1) with lines 2");
                tw.WriteLine("");
            }
            Thread.CurrentThread.CurrentCulture = cultureInfoBackup;
        }

        #endregion
    }
}
