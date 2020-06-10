/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.core;
using ai.pkr.holdem.strategy.hs;
using ai.lib.algorithms;

namespace ai.pkr.luck
{
    public class HeHsDeviation
    {
        public HeHsDeviation()
        {
        }

        public void ProcessHand(int [] hand)
        {
            ProcessHand(hand, hand.Length);
        }

        public void ProcessHand(int [] hand, int length)
        {
            double exp = 0.5;
            for (int round = 0; round < 4; ++round)
            {
                int roundHandLength = HeHelper.RoundToHandSize[round];
                if (roundHandLength > length)
                {
                    break;
                }
                double actHs = HandStrength.CalculateFast(hand, roundHandLength);
                double deviation = actHs - exp;
                _accDeviation[round] += deviation;
                _handCount[round]++;
                exp = actHs;
            }
        }

        /// <summary>
        /// Accumulated HS deviation for each round.
        /// </summary>
        public double[] AccDeviation
        {
            get { return _accDeviation;  }
        }

        /// <summary>
        /// Avergae HS deviation for each round.
        /// </summary>
        public double[] AvDeviation
        {
            get 
            { 
                double [] av = new double[4];
                for (int i = 0; i < av.Length; ++i)
                {
                    if (_handCount[i] != 0)
                    {
                        av[i] = _accDeviation[i]/_handCount[i];
                    }
                }
                return av;
            }
        }

        /// <summary>
        /// Hands counts for each round.
        /// </summary>
        public int[] HandCount
        {
            get { return _handCount; }
        }

        public void Reset()
        {
            _accDeviation.Fill(0);
            _handCount.Fill(0);
        }

        double[] _accDeviation = new double[4];
        int  [] _handCount = new int[4];
    }
}
