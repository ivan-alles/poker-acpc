/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;

namespace ai.pkr.holdem.strategy.ca
{
    public interface IClusterizer: IChanceAbstraction
    {
        bool IsVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// Is called when generation is started.
        /// </summary>
        /// <returns>Root cluster node with preflop children.</returns>
        IClusterNode OnGenerateBegin();
        
        /// <summary>
        /// Clusterises hands by some metric, created children in the parent node and returns 
        /// bucketized hands.
        /// </summary>
        /// <param name="hands">Hands. If it is null or empty, must created a single child and return an array of 0 length.</param>
        Bucket[] BucketizeHands(int round, McHand[] hands, IClusterNode parentNode);

        void OnGenerateEnd(IClusterNode root);

    }
}
