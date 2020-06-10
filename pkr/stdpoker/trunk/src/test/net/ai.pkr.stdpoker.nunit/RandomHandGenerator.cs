/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Diagnostics;
using ai.lib.algorithms.random;
using ai.pkr.metagame;
using System;

namespace ai.pkr.stdpoker.nunit
{
    /// <summary>
    /// Generates random hands.
    /// </summary>
    class RandomHandGenerator
    {
        /// <summary>
        /// Contains a hand in various formats so that any evaluator can use it.
        /// </summary>
        public struct Hand
        {
            int Size;
            public int c1, c2, c3, c4, c5, c6, c7;
            public int[] Cards;
            public CardSet CardSet;

            public void AddCard(int c)
            {
                Cards[Size] = c;
                switch (Size)
                {
                    case 0: c1 = c; break;
                    case 1: c2 = c; break;
                    case 2: c3 = c; break;
                    case 3: c4 = c; break;
                    case 4: c5 = c; break;
                    case 5: c6 = c; break;
                    case 6: c7 = c; break;
                    default:
                        return;
                }
                Size++;
            }

            public void SetMask(int cardCount)
            {
                CardSet.Clear();
                for (int i = 0; i < cardCount; ++i)
                {
                    Debug.Assert(!CardSet.IsIntersectingWith(StdDeck.Descriptor.CardSets[Cards[i]]));
                    CardSet.UnionWith(StdDeck.Descriptor.CardSets[Cards[i]]);
                }
            }
        }


        public RandomHandGenerator.Hand[] hands;

        private const int HAND_SIZE = 7;

        private SequenceRng _cardRng; 

        public RandomHandGenerator(int seed)
        {
            _cardRng = new SequenceRng(seed, StdDeck.Descriptor.FullDeckIndexes);
        }

        public RandomHandGenerator()
            : this((int)(DateTime.Now.Ticks))
        {
        }

        public void Generate(int count)
        {
            hands = new Hand[count];
            for (int h = 0; h < count; ++h)
            {
                //hands[h] = new RandomHand();
                hands[h].Cards = new int[HAND_SIZE];
                _cardRng.Shuffle(HAND_SIZE);
                for (int i = 0; i < HAND_SIZE; i++)
                {
                    hands[h].AddCard(_cardRng.Sequence[i]);
                }
            }
        }

        public void SetMask(int cardCount)
        {
            for (int h = 0; h < hands.Length; ++h)
            {
                hands[h].SetMask(cardCount);
            }
        }
    }
}