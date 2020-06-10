/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.algorithms.nunit
{
    /// <summary>
    /// Unit tests for class EnumAlgos.
    /// </summary>
    [TestFixture]
    public class EnumAlgos_Test
    {
        #region Tests

        [Test]
        public void Test_Factorial()
        {
            Assert.AreEqual(1, EnumAlgos.Factorial(0));
            Assert.AreEqual(1, EnumAlgos.Factorial(1));
            Assert.AreEqual(2, EnumAlgos.Factorial(2));
            Assert.AreEqual(6, EnumAlgos.Factorial(3));
            Assert.AreEqual(24, EnumAlgos.Factorial(4));
            Assert.AreEqual(120, EnumAlgos.Factorial(5));
            Assert.AreEqual(720, EnumAlgos.Factorial(6));
        }

        [Test]
        public void Test_CountPermut()
        {
            Assert.AreEqual(1, EnumAlgos.CountPermut(0, 0));
            Assert.AreEqual(1, EnumAlgos.CountPermut(1, 1));
            Assert.AreEqual(2, EnumAlgos.CountPermut(2, 2));
            Assert.AreEqual(6, EnumAlgos.CountPermut(3, 3));
            Assert.AreEqual(24, EnumAlgos.CountPermut(4, 4));
            Assert.AreEqual(120, EnumAlgos.CountPermut(5, 5));
            Assert.AreEqual(720, EnumAlgos.CountPermut(6, 6));

            Assert.AreEqual(1, EnumAlgos.CountPermut(4, 0));
            Assert.AreEqual(4, EnumAlgos.CountPermut(4, 1));
            Assert.AreEqual(12, EnumAlgos.CountPermut(4, 2));
            Assert.AreEqual(24, EnumAlgos.CountPermut(4, 3));
            Assert.AreEqual(24, EnumAlgos.CountPermut(4, 4));
        }

        [Test]
        public void Test_CountCombin()
        {
            Assert.AreEqual(1, EnumAlgos.CountCombin(0, 0));
            Assert.AreEqual(1, EnumAlgos.CountCombin(1, 0));
            Assert.AreEqual(2, EnumAlgos.CountCombin(2, 1));
            Assert.AreEqual(1326, EnumAlgos.CountCombin(52, 2));
            Assert.AreEqual(133784560, EnumAlgos.CountCombin(52, 7));
        }

        [Test]
        public void Test_GetPermut()
        {
            int expCount = 1;
            for(int n = 1; n < 7; ++n)
            {
                expCount *= n;
                List<List<int>> permuts = EnumAlgos.GetPermut(n);
                VerifyPermut(permuts, n, expCount);
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        
        private void VerifyPermut(List<List<int>> permuts, int n, int expCount)
        {
            Assert.AreEqual(expCount, permuts.Count);

            for (int i = 0; i < n; ++i)
            {
                permuts[0][i] = i;
            }

            for(int i = 0; i < permuts.Count; ++i)
            {
                Assert.AreEqual(n, permuts[i].Count);
                // Verify that all sequences are different.
                for(int j = 0; j < permuts.Count; ++j)
                {
                    if(i != j)
                    {
                        Assert.AreNotEqual(permuts[i], permuts[j]);
                    }
                }
            }
        }

        #endregion
    }
}
