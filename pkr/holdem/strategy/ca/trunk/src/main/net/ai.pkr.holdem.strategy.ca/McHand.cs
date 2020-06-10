/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.algorithms;
using ai.pkr.metagame;

namespace ai.pkr.holdem.strategy.ca
{
    public class McHand
    {
        public readonly int [] Cards = new int[StdDeck.Descriptor.Size];
        public int Length;

        public McHand()
        {
        }

        public McHand(McHand other)
        {
            Length = other.Length;
            Cards = other.Cards.ShallowCopy();
        }

        public override string ToString()
        {
            return StdDeck.Descriptor.GetCardNames(Cards, 0, Length);
        }
    }
}