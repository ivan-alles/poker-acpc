/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.lib.utils;

namespace ai.lib.algorithms.random
{
    /// <summary>
    /// Generates random numbers with probability given by weights.
    /// It is guaranteed that 0-weighted number will never occur.
    /// </summary>
    public class DiscreteProbabilityRng
    {
        #region Public API

        #region Constructors

        public DiscreteProbabilityRng()
        {
            _rng = RngHelper.CreateDefaultRng();
        }

        public DiscreteProbabilityRng(int seed)
        {
            _rng = RngHelper.CreateDefaultRng(seed);
        }

        public DiscreteProbabilityRng(Random rng)
        {
            _rng = rng;
        }

        #endregion
        /// <summary>
        /// Set weights. Can be called anytime to generate new distribution. 
        /// This method does not influence the underlying RNG.
        /// </summary>
        /// <param name="weights"></param>
        public void SetWeights(double [] weights)
        {
            _distr = new double[weights.Length];
            double sum = 0;
            for(int i = 0; i < _distr.Length; ++i)
            {
                if(weights[i] < 0)
                {
                    throw new ArgumentOutOfRangeException("Weights must be non-negative");
                }
                sum += weights[i];
                _distr[i] = sum;
            }
            if (sum == 0)
                throw new ApplicationException("Wrong probability distribution, all probabilities are 0");
            for (int i = 0; i < _distr.Length; ++i)
            {
                _distr[i] /= sum;
            }
        }

        /// <summary>
        /// Get next number.
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            // Generate random is in range [0..1), _dist[last] == 1
            double random = _rng.NextDouble();
            // Now find the first element in _distr that is larger than random.

            int idx = Array.BinarySearch(_distr, random);
            if (idx < 0)
            {
                idx = ~idx;
                // Now idx points to the element that is larger than random 
                // The docu is not quite clear if it the first such an element, so assume it is not.
                for (idx--; idx >= 0; idx--)
                {
                    if (_distr[idx] <= random)
                    {
                        break;
                    }
                }
                idx++;
            }
            else
            {
                // Exact match - it may be not the last such element, so find the next that is larger
                for (idx++; idx < _distr.Length; idx++)
                {
                    if (_distr[idx] > random)
                        break;
                }
            }
            Debug.Assert(idx >= 0 && idx < _distr.Length);
            Debug.Assert(_distr[idx] > 0);
            return idx;
        }

        #endregion

        #region Implementation

        private Random _rng;
        /// <summary>
        /// Contains discrete distribution functions for numbers [0, weigths.Length).
        /// Weigth is an array of real numbers, element at index i is the exclusive top probability
        /// of generating number i.
        /// Example:
        /// weights: 0, 0, 1, 2, 0, 0, 3, 2, 0, 0
        /// dist:    0, 0, 1, 3, 3, 3, 6, 8, 8, 8
        /// </summary>
        private double[] _distr;

        #endregion
    }
}
