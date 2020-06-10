/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;
using System.Reflection;

namespace ai.pkr.holdem.strategy.ca.nunit
{
    /// <summary>
    /// Unit tests for CaMcGen. 
    /// </summary>
    [TestFixture]
    public class CaMcGen_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public void Test_Interactive()
        {
            string bucketCounts = "8 7 6 5";
            HsRangeCa ca = new HsRangeCa(GetParams(bucketCounts));

            CaMcGen gen = new CaMcGen 
            { 
                Clusterizer = ca,
                IsVerbose = true,
                // IsVerboseSamples = true,
                RngSeed = 0,
                SamplesCount = new int[]{0, 100, 10, 10}
            };

            gen.Generate();
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        private Props GetParams(string bucketCounts)
        {
            Props p = new string[]
                          {
                            "TypeName", typeof(HsRangeCa).AssemblyQualifiedName,
                            "AssemblyFileName", "",
                            "BucketCounts", bucketCounts,
                            "Pockets7", "AA KK QQ JJ TT 99",
                            "Pockets6", "88 AKs 77 AQs AJs AKo ATs AQo AJo KQs 66 A9s ATo KJs A8s KTs",
                            "Pockets5", "KQo A7s A9o KJo 55 QJs K9s A5s A6s A8o KTo QTs A4s A7o K8s A3s QJo K9o A5o A6o Q9s K7s JTs A2s QTo 44 A4o K6s",
                            "Pockets4", "K8o Q8s A3o K5s J9s Q9o JTo K7o A2o K4s Q7s K6o K3s T9s J8s 33 Q6s Q8o K5o J9o K2s Q5s",
                            "Pockets3", "K4o T8s J7s Q4s Q7o T9o J8o K3o Q6o Q3s 98s T7s J6s K2o 22 Q2s Q5o J5s T8o J7o Q4o 97s J4s T6s",
                            "Pockets2", "J3s Q3o 98o 87s T7o J6o 96s J2s Q2o T5s J5o T4s 97o 86s J4o T6o 95s T3s 76s J3o 87o T2s 85s 96o J2o T5o 94s 75s T4o",
                            "Pockets1", "93s 86o 65s 84s 95o T3o 92s 76o 74s T2o 54s 85o 64s 83s 94o 75o 82s 73s 93o 65o 53s 63s 84o 92o 43s 74o",
                          };
            return p;
        }

        #endregion
    }
}
