/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ai.lib.algorithms.random
{
    /// <summary>
    /// Given a sequence of integers, generate a random subsequence of given length.
    /// Either uses a default RNG (System.Random) or allows the user 
    /// to pass one in the constructor.
    /// </summary>
    public class SequenceRng
    {
        #region Public API

        #region Constructors

        public SequenceRng()
        {
            _rng = RngHelper.CreateDefaultRng();
        }

        public SequenceRng(Random rng)
        {
            _rng = rng;
        }

        public SequenceRng(int seed)
        {
            _rng = RngHelper.CreateDefaultRng(seed);
        }

        public SequenceRng(Random rng, int [] sequence)
        {
            _rng = rng;
            SetSequence(sequence);
        }

        public SequenceRng(int[] sequence)
        {
            _rng = RngHelper.CreateDefaultRng();
            SetSequence(sequence);
        }


        public SequenceRng(int seed, int[] sequence)
        {
            _rng = RngHelper.CreateDefaultRng(seed);
            SetSequence(sequence);
        }

        #endregion

        /// <summary>
        /// Set the array with numbers used for the sequences. The array is copied 
        /// to the internal memory and can be modified by user.
        /// </summary>
        public void SetSequence(int[] sequence)
        {
            _sequence = new int[sequence.Length];
            Array.Copy(sequence, _sequence, sequence.Length);
        }

        /// <summary>
        /// Equivalent to SetSequence(new int[]{0, 1, ...., count}).
        /// </summary>
        public void SetSequence(int count)
        {
            _sequence = new int[count];
            for (int i = 0; i < count; ++i)
                _sequence[i] = i;
        }

        /// <summary>
        /// Set the array with numbers used for the sequences. The array is NOT copied 
        /// to the internal memory. This can be used for performance reasons and to shuffle
        /// the user array directly.
        /// </summary>
        public void SetSequenceNoCopy(int[] sequence)
        {
            _sequence = sequence;
        }

        public int[] Sequence
        {
            get { return _sequence; }
        }

        /// <summary>
        /// Shuffles count elements in the given sequnce starting from start. 
        /// The elements below start are untouched.
        /// </summary>
        public static void Shuffle(Random rng, int [] sequence, int start, int count)
        {
            int end = start + count;
            for (int i = start; i < end; ++i)
            {
                int rndIdx = rng.Next(i, sequence.Length);
                ShortSequence.Swap(ref sequence[rndIdx], ref sequence[i]);
            }
        }

        /// <summary>
        /// Shuffles count elements in the sequnce starting from start. 
        /// The elements below start are untouched.
        /// </summary>
        public void Shuffle(int start, int count)
        {
            Shuffle(_rng, _sequence, start, count);
        }

        /// <summary>
        /// Shuffles count elements in the sequnce starting from 0.
        /// </summary>
        public void Shuffle(int count)
        {
            Shuffle(0, count);
        }

        /// <summary>
        /// Shuffles the whole sequence. 
        /// </summary>
        public void Shuffle()
        {
            Shuffle(_sequence.Length);
        }

        #endregion

        #region Protected members

        protected int[] _sequence;
        protected Random _rng;

        #endregion
    }
}