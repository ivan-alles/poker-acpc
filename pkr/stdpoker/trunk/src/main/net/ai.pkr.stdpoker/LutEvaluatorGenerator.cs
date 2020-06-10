/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metagame;
using ai.lib.algorithms;
using System.Diagnostics;
using ai.lib.algorithms.numbers;
using System.IO;
using ai.lib.utils;
using System.Reflection;

namespace ai.pkr.stdpoker
{
    /// <summary>
    /// LUT generator for lut-based evaluators. It creates a set of linked states. 
    /// Each state correspond to one or more hands (containing 0, 1, ... max cards). 
    /// A transition table of 52 entries contains the id of the next state for each possible cards.
    /// For the hands of size max-1 the transition table contains hand values to save space.
    /// This costruct can be saved as a look up table. Multiple formats may be created to solve specific problems.
    /// The most compact and easy to use format supports only evaluation of max-hands  (e.g. only 5 or 7).
    /// </summary>
    /// See also the explanations at the bottom of the file.
    public class LutEvaluatorGenerator
    {
        #region Public API

        public void GenerateStates(int maxHandSize)
        {
            DateTime startTime = DateTime.Now;

            _maxHandSize = maxHandSize;

            _cardsToState = new CardsToState[_maxHandSize][];
            for (int i = 0; i < _maxHandSize; ++i)
            {
                _cardsToState[i] = new CardsToState[EnumAlgos.CountCombin(52, i)];
            }

            for (int i = 0; i < _maxHandSize; ++i)
            {
                _combinCount = 0;
                GenerateCombin(CardSet.Empty, 51, 0, i, OnCombinCreateStates);
                Debug.Assert(_combinCount == _cardsToState[i].Length);
                Console.WriteLine("{0}-hands generated", i);
            }
            Console.WriteLine("{0:#,#} states created", _states.Count);

            if (_maxHandSize == 7)
            {
                // Print some statistics about 7-hands.
                int max6EquivStatesCount = -1;
                int nonUniqueKeyCount = 0;
                foreach (List<State> l in _dict6.Values)
                {
                    max6EquivStatesCount = Math.Max(max6EquivStatesCount, l.Count);
                    if (l.Count > 1)
                    {
                        nonUniqueKeyCount++;
                    }
                }
                Console.WriteLine("Number of non-unique 6-hands keys: {0}", nonUniqueKeyCount);
                Console.WriteLine("Max. number of equivalent 6-hands with same key: {0}", max6EquivStatesCount);
            }

            for (int i = 0; i < _maxHandSize; ++i)
            {
                _combinCount = 0;
                GenerateCombin(CardSet.Empty, 51, 0, i, OnCombinLinkStates);
                Debug.Assert(_combinCount == _cardsToState[i].Length);
                Console.WriteLine("{0}-hands linked", i);
            }

            double runTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Generated in {0:0.000} seconds", runTime);
        }

        /// <summary>
        /// Saves the generated states as a look-up table.
        /// Format:
        /// BdsVersion.
        /// UInt32 format ID.
        /// UInt32 LUT size (number of UInt32 entries).
        /// UInt32 [] LUT.
        /// </summary>
        public void SaveLut(string path, UInt32 formatId)
        {
            string dirName = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            using (BinaryWriter w = new BinaryWriter(File.Open(path, FileMode.Create, FileAccess.Write)))
            {
                BdsVersion version = new BdsVersion(Assembly.GetExecutingAssembly());
                version.Write(w);
                w.Write(formatId);
                w.Write((UInt32)_states.Count * 52);
                for (int s = 0; s < _states.Count; ++s)
                {
                    for (int i = 0; i < 52; ++i)
                    {
                        UInt32 value = (UInt32)_states[s].Table[i];
                        if (_states[s].Key.HandSize < _maxHandSize - 1)
                        {
                            value *= 52;
                        }
                        w.Write(value);
                    }
                }
            }
        }


        public bool Test5Hands()
        {
            _typeCounts = new int[(int)HandValue.Kind._Count + 1];
            int[] ci = new int[5];
            GenerateTestCombin(CardSet.Empty, ci, 0, 0, 0, 5, OnTestCombin);
            return VerifyTypeCounts(_typeCounts, _expTypeCounts5);
        }

        public bool Test6Hands()
        {
            _typeCounts = new int[(int)HandValue.Kind._Count + 1];
            int[] ci = new int[6];
            GenerateTestCombin(CardSet.Empty, ci, 0, 0, 0, 6, OnTestCombin);
            return VerifyTypeCounts(_typeCounts, _expTypeCounts6);
        }

        public bool Test7Hands()
        {
            _typeCounts = new int[(int)HandValue.Kind._Count + 1];
            int[] ci = new int[7];
            GenerateTestCombin(CardSet.Empty, ci, 0, 0, 0, 7, OnTestCombin);
            return VerifyTypeCounts(_typeCounts, _expTypeCounts7);
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Returns a cardset containing only cards of suites that are either a flush-draw 
        /// or a flush. A flush draw is a set of cards of one suit that still can produce a flush,
        /// e.g. any single card for 5-hands, or 7c 8c Ad Kd for 7-hands.
        /// If the flush (5+ cards of one suit) is present, this function preserves 
        /// only 5 highest flush cards, to prevent 6-hands like 
        /// Ac Qc Jc Tc 9c 2c and Ac Qc Jc Tc 9c 3c from being different.
        /// This function is public to make some unit-tests.
        /// </summary>
        public static CardSet ExtractFlush(CardSet hand, int maxHandSize)
        {
            int handSize = hand.CountCards();
            uint half = (uint) hand.bits;

            _suits[0] = (half & 0xFFFF);
            _suits[1] = (half >> 16);
            half = (uint) (hand.bits >> 32);
            _suits[2] = (half & 0xFFFF);
            _suits[3] = (half >> 16);

            for(int i = 0; i < 4; ++i)
            {
                int suitCount = CountBits.Count(_suits[i]);
                if (suitCount + (maxHandSize - handSize) >= 5)
                {
                    // Flush is possible
                    // Preserve the 5 highest cards.
                    uint bit = 0x00000001;
                    while(suitCount > 5)
                    {
                        if((_suits[i] & bit) != 0)
                        {
                            suitCount--;
                            _suits[i] &= ~(bit);
                        }
                        bit <<= 1;
                    }
                }
                else
                {
                    // Flush is impossible, delete the cards.
                    _suits[i] = 0;
                }
            }
            CardSet result = CardSet.Empty;
            for(int i = 0; i < 4; ++i)
            {
                result.bits |= (((UInt64)_suits[i]) << (i*16));
            }
            return result;
        }

        private class KeyT
        {
            public KeyT(int handSize, int maxHandSize, CardSet cards)
            {
                HandSize = (byte)handSize;
                RankCards  = NormRank.Convert(cards);
                FlushCards = ExtractFlush(cards, maxHandSize);
                if(handSize < 5)
                {
                    return;
                }
                HandValue = CardSetEvaluator.Evaluate(ref cards);
            }

            public byte HandSize;
            public CardSet RankCards;
            public CardSet FlushCards;
            public UInt32 HandValue;
        }

        private class KeyComparer : IEqualityComparer<KeyT>
        {
            #region IEqualityComparer<KeyT> Members

            public bool Equals(KeyT x, KeyT y)
            {
                // We can just safely compare all the fiels if sizes equals, because
                // they are either both 0 (not used) or unique.

                return x.HandSize == y.HandSize &&
                       x.HandValue == y.HandValue &&
                       x.RankCards == y.RankCards &&
                       x.FlushCards == y.FlushCards;
            }

            public int GetHashCode(KeyT k)
            {
                int value = ((Int32) k.HandSize << 28);
                value |= (int) (k.HandValue);
                value ^= (int)((k.RankCards.bits >> 32) | (k.RankCards.bits & 0xFFFFFFFFUL));
                value ^= (int)((k.FlushCards.bits >> 32) | (k.FlushCards.bits & 0xFFFFFFFFUL));
                return value;
            }

            #endregion
        }

        class State
        {
            public KeyT Key;
            public Int32[] Table = new int[52];
            public int Id = -1;
        }

        struct CardsToState: IComparable<CardsToState>
        {
            public CardSet Cards;
            public int StateId;

            #region IComparable<CardsToState> Members

            public int CompareTo(CardsToState other)
            {
                // Comparison for descending sort.
                return -Cards.bits.CompareTo(other.Cards.bits);
            }

            #endregion

            public override string ToString()
            {
                return Cards.ToString();
            }
        }

        private List<State> _states = new List<State>();
        private Dictionary<KeyT, State> _dict = new Dictionary<KeyT, State>(new KeyComparer());
        private Dictionary<KeyT, List<State>> _dict6 = new Dictionary<KeyT, List<State>>(new KeyComparer());
        private int _combinCount;
        private int _maxHandSize;
        static readonly uint[] _suits = new uint[4];

        /// <summary>
        /// Contains mapping cards->states for all hands [0.._maxHandSize-1].
        /// Mapping for the maximal hand size is not needed because there is no states
        /// for it, the tables contain hand value instead of state id.
        /// Each array is created in sorted order (descending).
        /// </summary>
        private CardsToState[][] _cardsToState;

        private delegate void OnCombinDelegate(CardSet cards, int count);

        private void AddState(KeyT key, State state)
        {
            state.Key = key;
            state.Id = _states.Count;
            _states.Add(state);
            if (state.Key.HandSize < 6)
            {
                _dict.Add(key, state);
            }
            else
            {
                List<State> list;
                if(!_dict6.TryGetValue(key, out list))
                {
                    list = new List<State>();
                    _dict6.Add(key, list);
                }
                list.Add(state);
            }
            if(_states.Count % 50000 == 0)
            {
                Console.WriteLine("{0:#,#} states created", _states.Count);
            }
        }

        void GenerateCombin(CardSet cards, int start, int count, int requiredCount, OnCombinDelegate onCombin)
        {
            if(count == requiredCount)
            {
                onCombin(cards, count);
                return;
            }
            // Deal so that the generated cardsets are sorted (in decsending order).
            for (int c = start; c >= (requiredCount - count - 1); --c)
            {
                CardSet newCard = StdDeck.Descriptor.CardSets[c];
                GenerateCombin(cards | newCard, c - 1, count + 1, requiredCount, onCombin);
            }
        }

        void OnCombinCreateStates(CardSet cards, int count)
        {
            _cardsToState[count][_combinCount].Cards = cards;
            KeyT key = new KeyT(count, _maxHandSize, cards);
            State state;
            if (key.HandSize < 6)
            {
                if (!_dict.TryGetValue(key, out state))
                {
                    state = new State();
                    AddState(key, state);
                }
            }
            else
            {
                state = CreateOrFind6State(cards, key);
            }
            _cardsToState[count][_combinCount].StateId = state.Id;

            _combinCount++;
        }

        private void FillFinalHands(State state, CardSet cards)
        {
            for (int c = 0; c < 52; ++c)
            {
                if (state.Table[c] != 0)
                {
                    // Already done.
                    continue;
                }
                CardSet nextCard = StdDeck.Descriptor.CardSets[c];
                if (nextCard.IsIntersectingWith(cards))
                {
                    continue;
                }
                CardSet finalHand = cards | nextCard;
                Debug.Assert(finalHand.CountCards() == _maxHandSize);
                UInt32 handRank = CardSetEvaluator.Evaluate(ref finalHand);
                state.Table[c] = (Int32)handRank;
            }
        }

        /// <summary>
        /// Finds an equivalent 6-hand states or creates a new one.
        ///For 6-hands the equality of the keys does not guarantee the equality of 
        /// the states. Therefore we have to do special processing and compare
        /// states with equal key. To do this.compare states with the same keys, 
        /// we will compare the transition tables.
        /// </summary>
        State CreateOrFind6State(CardSet cards, KeyT key)
        {
            List<State> list;
            State state = new State();
            FillFinalHands(state, cards);
            if (!_dict6.TryGetValue(key, out list))
            {
                // The very first state for this key, no doubts we have to add it.
                AddState(key, state);
                return state;
            }

            foreach (State eqCandidate in list)
            {
                bool areEqual = true;
                for (int i = 0; i < 52; ++i)
                {
                    if (eqCandidate.Table[i] != state.Table[i])
                    {
                        if (eqCandidate.Table[i] == 0 || state.Table[i] == 0)
                        {
                            // Ignore difference if one of the elements is not initialized.
                            continue;
                        }
                        areEqual = false;
                        break;
                    }
                }
                if (areEqual)
                {
                    // Great, an equivalent state is found, return it.
                    return eqCandidate;
                }
            }
            // No equivalent is found - add this state.
            AddState(key, state);
            return state;
        }
        
        void OnCombinLinkStates(CardSet srcCards, int count)
        {
            // Find the state for theses cards. No search is necessary here,
            // as the cards are dealt in the same order as in creation, we can use _combinCount.
            int srcMapIdx = _combinCount;
            Debug.Assert(_cardsToState[count][srcMapIdx].Cards == srcCards);
            State srcState = _states[_cardsToState[count][srcMapIdx].StateId];

            // Deal next cards.
            for(int c = 0; c < 52; ++c)
            {
                if(srcState.Table[c] != 0)
                {
                    // Already linked
                    continue;
                }
                CardSet newCard = StdDeck.Descriptor.CardSets[c];
                if(newCard.IsIntersectingWith(srcCards))
                {
                    // Impossible card.
                    continue;
                }
                CardSet dstCards = newCard | srcCards;
                int linkValue;
                if(count + 1 < _maxHandSize)
                {
                    // Link to state
                    int dstMapIdx = Array.BinarySearch(_cardsToState[count+1], new CardsToState { Cards = dstCards });
                    Debug.Assert(_cardsToState[count + 1][dstMapIdx].Cards == dstCards);
                    State dstState = _states[_cardsToState[count+1][dstMapIdx].StateId];
                    linkValue = dstState.Id;
                }
                else
                {
                    // Link to hand value.
                    linkValue = (int) CardSetEvaluator.Evaluate(ref dstCards);
                }
                srcState.Table[c] = linkValue;
            }
            _combinCount++;
        }

        #region Simple self testing

        private delegate void OnTestCombinDelegate(CardSet cards, int [] cardIndexes, int linkValue);

        private void GenerateTestCombin(CardSet cards, int [] cardIndexes, int linkValue, int start, int count, int requiredCount,
                                        OnTestCombinDelegate onCombin)
        {
            if(count == requiredCount)
            {
                onCombin(cards, cardIndexes, linkValue);
                return;
            }
            State r = _states[linkValue];
            // Go in the order opposite to the order of creation to put a little stress 
            for (int c = start; c <= 52 - (requiredCount - count); ++c)
            {
                CardSet newCard = StdDeck.Descriptor.CardSets[c];
                int newLinkValue = r.Table[c];
                cardIndexes[count] = c;
                GenerateTestCombin(cards | newCard, cardIndexes, newLinkValue, c + 1, count + 1, requiredCount, onCombin);
            }
        }

        void OnTestCombin(CardSet cards, int[] cardIndexes, int linkValue)
        {
            UInt32 handValue = (UInt32)linkValue;
            UInt32 expHandValue = CardSetEvaluator.Evaluate(ref cards);
            if (handValue != expHandValue)
            {
                string cardsText =
                    StdDeck.Descriptor.GetCardNames(cardIndexes);
                // Set a breakpoint here to see what went wrong.
            }
            int handType = handValue == 0
                               ? 0
                               : (int)HandValue.GetKind(handValue) + 1;
            _typeCounts[handType]++;

        }

        private bool VerifyTypeCounts(int[] typeCounts, int[] expCounts)
        {
            Console.WriteLine("Type counts: ");
            bool isOk = true;
            for(int i = 0; i < typeCounts.Length; ++i)
            {
                Console.Write("{0,20}: ", i == 0 ? "Bad" : ((HandValue.Kind) (i - 1)).ToString());
                Console.Write("{0,10}", typeCounts[i]);
                if (typeCounts[i] == expCounts[i])
                {
                    Console.Write(" OK");
                }
                else
                {
                    Console.Write(" ERROR, expected {0:0,0}", expCounts[i]);
                    isOk = false;
                }
                Console.WriteLine();
            }
            return isOk;
        }

        private int[] _typeCounts;

        private static readonly Int32[] _expTypeCounts5 =
        {
            0,
            1302540,
            1098240,
            123552,
            54912,
            10200,
            5108,
            3744,
            624,
            40
        };

        private static readonly Int32[] _expTypeCounts6 =
        {
            0,
            6612900,
            9730740,
            2532816,
            732160,
            361620,
            205792,
            165984,
            14664,
            1844,
        };

        private static readonly Int32[] _expTypeCounts7 =
        {
            0,
            23294460,
            58627800,
            31433400,
            6461620,
            6180020,
            4047644,
            3473184,
            224848,
            41584,
        };
        #endregion

        #endregion
    }
}
/* Description of the algorithm.
 * 
 * Simplified diagram of data structures:
 * 
 * 1. LIST OF STATES.         2. DICTIONARY KEY -> STATE    3. MAPPING CARDSET -> STATE ID OR HAND VALUE 
 * Position == state ID.                                    (sorted lists)
 * |state 0|                  |key|state|                   |cards|state id / hv|
 * |state 1|                  |key|state|                   |cards|state id / hv| 
 * ...                        |key|state|                   |cards|state id / hv|
 * |state N|                  |key|state|                   |cards|state id / hv|
 * 
 * Simplified description of algorithm:
 * 
 * 1.  For each card combination (size 0..max): 
 * 1.1 Generate key
 * 1.2 Find existing state for this key in the dictionary 2 or 
 *     create a new state and add it to containers 1 and 2.
 * 1.3 Store state ID to the mapping 3.
 * 2.  For each card combination (size 0..max)
 * 2.1 Find the source state using mapping 3 
 * 2.2 Deal each of (52 - cardset.size) cards, lookup target state id in mapping 3 and create
 *     a link source -> target.
 *
 * In reality there are complications:
 * 1. No states are created for final hands. Instead, cardsets are mapped directly to hand values.
 * 2. Mapping 3 is done separely for each hand size - it is easier to create a sorted array this way.
 * 3. The key is a tricky structure taking into account flush draws, flushes, ranks and hand values.
 *    Look at the key class to see how it works.
 * 4. If final hand size == 7, there is a problem with keys for 6-hands. Some states are not uniquely
 *    identified by keys. Therefore there is a special code and data structures to handle 6-states.
 */
