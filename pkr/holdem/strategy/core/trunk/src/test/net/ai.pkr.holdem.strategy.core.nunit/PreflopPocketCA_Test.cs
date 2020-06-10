/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;

namespace ai.pkr.holdem.strategy.core.nunit
{
    /// <summary>
    /// Unit tests for PreflopPocketCA. 
    /// </summary>
    [TestFixture]
    public class PreflopPocketCA_Test
    {
        #region Tests

        [Test]
        public void Test_Constructor()
        {
            // No preflop bucketing by pocket
            Props p = new string[]
            {
                "SomeProp",	"SomeVal"
            };
            PreflopPocketCA pfca = new PreflopPocketCA(p, 4);
            Assert.IsNull(pfca.PocketKindToAbstrCard);

            // Do preflop bucketing by pocket
            p = new string[]
            {
                "Pockets3",	"AA KK",
                "Pockets2",	"QQ JJ TT",
                "Pockets1",	"AKs AKo AQs",
            };
            pfca = new PreflopPocketCA(p, 4);
            Assert.IsNotNull(pfca.PocketKindToAbstrCard);
            Assert.AreEqual((int)HePocketKind.__Count, pfca.PocketKindToAbstrCard.Length);
            for (int i = 0; i < (int)HePocketKind.__Count; ++i)
            {
                HePocketKind pk = (HePocketKind)i;
                switch (pk)
                {
                    case HePocketKind._AA:
                    case HePocketKind._KK:
                        Assert.AreEqual(3, pfca.PocketKindToAbstrCard[i]);
                        break;
                    case HePocketKind._QQ:
                    case HePocketKind._JJ:
                    case HePocketKind._TT:
                        Assert.AreEqual(2, pfca.PocketKindToAbstrCard[i]);
                        break;
                    case HePocketKind._AKs:
                    case HePocketKind._AKo:
                    case HePocketKind._AQs:
                        Assert.AreEqual(1, pfca.PocketKindToAbstrCard[i]);
                        break;
                    default:
                        Assert.AreEqual(0, pfca.PocketKindToAbstrCard[i], pk.ToString());
                        break;
                }
            }
        }

        [Test]
        public void Test_Constructor_Exception()
        {
            // Specify some bucket, but miss bucket 2
            Props p = new string[]
            {
                "Pockets3",	"AA KK",
                "Pockets1",	"AKs AKo AQs",
            };
            bool exceptionOccured = false;
            try
            {
                PreflopPocketCA pfca = new PreflopPocketCA(p, 4);
            }
            catch (ArgumentException )
            {
                exceptionOccured = true;
            }
            Assert.IsTrue(exceptionOccured);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
