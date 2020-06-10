/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.pkr.stdpoker;

namespace ai.pkr.bots.neytiri
{
    public class MonteCarloDealer
    {
        protected int[] _cards;

        protected Random _rng;

        public MonteCarloDealer()
        {
            _rng = new Random();
        }

        public MonteCarloDealer(int seed)
        {
            _rng = new Random(seed);
        }

        public void Initialize(CardSet deadCards)
        {
            CardSet full = StdDeck.Descriptor.FullDeck;
            Debug.Assert(full.CountCards() == 52);
            full.Remove(deadCards);
            _cards = StdDeck.Descriptor.GetIndexesAscending(full).ToArray();
        }

        public int[] Cards
        {
            get { return _cards; }
        }

        /// <summary>
        /// Shuffles N cards. The cards can be read from Cards property.
        /// </summary>
        public void Shuffle(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                int rndIdx = _rng.Next(i, _cards.Length);
                int tmp = _cards[rndIdx];
                _cards[rndIdx] = _cards[i];
                _cards[i] = tmp;
            }
        }
    }
}