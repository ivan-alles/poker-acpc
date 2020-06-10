/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;

namespace ai.pkr.holdem.strategy.core.nunit
{
    /// <summary>
    /// Unit tests for HeHelper. 
    /// </summary>
    [TestFixture]
    public class HeHelper_Test
    {
        #region Tests

        [Test]
        public void Test_HandSizeToRound()
        {
            for (int s = 0; s < HeHelper.HandSizeToRound.Length; ++s)
            {
                switch (s)
                {
                    case 2:
                        Assert.AreEqual(0, HeHelper.HandSizeToRound[s]);
                        break;
                    case 5:
                        Assert.AreEqual(1, HeHelper.HandSizeToRound[s]);
                        break;
                    case 6:
                        Assert.AreEqual(2, HeHelper.HandSizeToRound[s]);
                        break;
                    case 7:
                        Assert.AreEqual(3, HeHelper.HandSizeToRound[s]);
                        break;
                    default:
                        Assert.AreEqual(-1, HeHelper.HandSizeToRound[s]);
                        break;
                }
            }
        }

        [Test]
        public void Test_RoundToHandSize()
        {
            for (int r = 0; r < HeHelper.RoundToHandSize.Length; ++r)
            {
                switch (r)
                {
                    case 0:
                        Assert.AreEqual(2, HeHelper.RoundToHandSize[r]);
                        break;
                    case 1:
                        Assert.AreEqual(5, HeHelper.RoundToHandSize[r]);
                        break;
                    case 2:
                        Assert.AreEqual(6, HeHelper.RoundToHandSize[r]);
                        break;
                    case 3:
                        Assert.AreEqual(7, HeHelper.RoundToHandSize[r]);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }
        }

        [Test]
        public void Test_CanLose()
        {
            DeckDescriptor d = StdDeck.Descriptor;
            Assert.IsFalse(HeHelper.CanLose(d.GetIndexes("Ac Ad Ah As 2c 2d 3c")));
            Assert.IsFalse(HeHelper.CanLose(d.GetIndexes("7c 2d Ah Kh Qh Jh Th")));
            Assert.IsTrue(HeHelper.CanLose(d.GetIndexes("7c 2d Kh Qh Jh Th 9h")));
            Assert.IsTrue(HeHelper.CanLose(d.GetIndexes("7c 2d Th 9h 8h 7h 6h")));
            Assert.IsFalse(HeHelper.CanLose(d.GetIndexes("Jh Qh Th 9h 8h 7h 6h")));
            Assert.IsFalse(HeHelper.CanLose(d.GetIndexes("7h 3s Ac Ks Qd Js Tc")));
            Assert.IsTrue(HeHelper.CanLose(d.GetIndexes("7h 3s Ks Qd Js Tc 9h")));
            Assert.IsTrue(HeHelper.CanLose(d.GetIndexes("Ac Ad Ah Kc Kd 7c 3h 2s")));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
