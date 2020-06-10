/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.bots.neytiri.builder
{
    [Serializable]
    class CardDistribution
    {
        public Dictionary<CardSet, int> HandCounters;
        public int TotalCounter;

        public CardDistribution()
        {
            HandCounters = new Dictionary<CardSet,int>();
        }

        public CardDistribution(CardDistribution other)
        {
            HandCounters = new Dictionary<CardSet, int>(other.HandCounters);
            TotalCounter = other.TotalCounter;
        }

        public double GetProbability(CardSet hand)
        {
            int counter = 0;
            if(HandCounters.TryGetValue(hand, out counter))
            {
                return ((double) counter)/TotalCounter;
            }
            return 0;
        }

        public void AddHand(CardSet hand, int counter)
        {
            if (!HandCounters.ContainsKey(hand))
            {
                HandCounters[hand] = counter;
            }
            else
            {
                HandCounters[hand] += counter;
            }
            TotalCounter += counter;
        }

        public void Clear()
        {
            HandCounters.Clear();
            TotalCounter = 0;
        }

        /// <summary>
        /// Merges the other dictionary to this dictionary.
        /// </summary>
        public void Merge(CardDistribution other)
        {
            foreach(KeyValuePair<CardSet, int> kvp in other.HandCounters)
            {
                AddHand(kvp.Key, kvp.Value);
            }
        }
    }
}
