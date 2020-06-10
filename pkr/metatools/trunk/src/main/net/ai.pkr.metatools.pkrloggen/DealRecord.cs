/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using System.Diagnostics;
using ai.lib.algorithms.random;
using ai.lib.algorithms;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metatools.pkrloggen
{
    class DealRecord
    {
        public DealRecord(GameRecord cfg, DeckDescriptor deckDescr, SequenceRng randomDealer)
        {
            _cfg = cfg;
            _deckDescr = deckDescr;
            _randomDealer = randomDealer;

            // Loop through actions and
            // - find the fixed cards 
            // - count random cards
            // - count enumerated cards
            
            for (int a = 0; a < _cfg.Actions.Count; ++a)
            {
                PokerAction action = _cfg.Actions[a];
                if (!action.IsDealerAction())
                    continue;
                string[] cards = action.Cards.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string card in cards)
                {
                    if (card == "?")
                    {
                        _randomCount++;
                    }
                    else if (card.Length > 0 && card.Substring(0, 1) == "#")
                    {
                        int idx = int.Parse(card.Substring(1));
                        while (_enumCounts.Count <= idx)
                        {
                            _enumCounts.Add(0);
                            _enumCombosCounts.Add(0);
                            _enumCombos.Add(new List<CardSet>());
                        }
                        _enumCounts[idx]++;
                    }
                    else
                    {
                        _fixedCards.UnionWith(_deckDescr.GetCardSet(card));
                    }
                }
            }

            // Count enumerated combinations.
            int deadCards = _fixedCards.CountCards();
            for (int i = 0; i < _enumCounts.Count; ++i)
            {
                _enumCombosCounts[i] = EnumAlgos.CountCombin(_deckDescr.Size - deadCards, _enumCounts[i]);
                deadCards += _enumCounts[i];
            }
        }

        public GameRecord Generate(int repetition)
        {
            string[][] enumNames = new string[_enumCounts.Count][];
            int[] enumIndexes = new int[_enumCounts.Count];
            CardSet deadCards = _fixedCards;

            if(_enumCounts.Count != 0)
            {
                int r = repetition;
                for(int d = _enumCounts.Count-1; d >=0 ; --d)
                {
                    enumIndexes[d] = r%(int)_enumCombosCounts[d];
                    r/=(int)_enumCombosCounts[d];
                }
                int redealFrom = _enumCombos.Count;
                for (int d = _enumCounts.Count - 1; d >= 0; --d)
                {
                    if (enumIndexes[d] != 0)
                    {
                        break;
                    }
                    redealFrom = d;
                }
                for (int d = 0; d < redealFrom; ++d)
                {
                    deadCards.UnionWith(_enumCombos[d][enumIndexes[d]]);
                }
                for (int d = redealFrom; d < _enumCombos.Count; ++d)
                {
                    _enumCombos[d].Clear();
                    CardEnum.Combin(_deckDescr, _enumCounts[d], CardSet.Empty, deadCards, OnCombin, d);
                    Debug.Assert(_enumCombosCounts[d] == _enumCombos[d].Count);
                    deadCards.UnionWith(_enumCombos[d][enumIndexes[d]]);
                }
                for (int d = 0; d < _enumCombos.Count; ++d)
                {
                    string enumCards = _deckDescr.GetCardNames(_enumCombos[d][enumIndexes[d]]);
                    enumNames[d] = enumCards.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            GameRecord result = new GameRecord(_cfg.ToString());
            result.Actions.Clear();

            // Shuffle the rest of the deck.
            CardSet deckRest = _deckDescr.FullDeck;
            deckRest.Remove(deadCards);
            _randomDealer.SetSequence(_deckDescr.GetIndexesAscending(deckRest).ToArray());
            _randomDealer.Shuffle(_randomCount);

            int randomDealt = 0;
            int[] enumDealt = new int[_enumCounts.Count];
            // Deal the cards.
            for (int a = 0; a < _cfg.Actions.Count; ++a)
            {
                PokerAction action = _cfg.Actions[a];
                if (!action.IsDealerAction())
                    continue;
                string[] cards = action.Cards.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                CardSet resultCards = new CardSet();
                foreach (string card in cards)
                {
                    CardSet nextCard = new CardSet();
                    if (card == "?")
                    {
                        nextCard = _deckDescr.GetCardSet(_randomDealer.Sequence, randomDealt++, 1);
                    }
                    else if (card.Length > 0 && card.Substring(0, 1) == "#")
                    {
                        int d = int.Parse(card.Substring(1));
                        string enumName = enumNames[d][enumDealt[d]++];
                        nextCard = _deckDescr.GetCardSet(enumName);
                    }
                    else
                    {
                        nextCard = _deckDescr.GetCardSet(card);
                    }
                    Debug.Assert(!resultCards.IsIntersectingWith(nextCard));
                    resultCards.UnionWith(nextCard);
                }

                PokerAction resultAction = new PokerAction(action.Kind, action.Position, action.Amount,
                    _deckDescr.GetCardNames(resultCards));
                result.Actions.Add(resultAction);
            }
            Debug.Assert(_randomCount == randomDealt);
            return result;
        }

        void OnCombin(ref CardSet combin, int d)
        {
            _enumCombos[d].Add(combin);
        }

        public long[] EnumCombosCounts
        {
            get { return _enumCombosCounts.ToArray(); }
        }

        CardSet _fixedCards;
        GameRecord _cfg;
        int _randomCount = 0;

        private List<int> _enumCounts = new List<int>();
        private List<long> _enumCombosCounts = new List<long>();
        private List<List<CardSet>> _enumCombos = new List<List<CardSet>>(10);
        
        private static readonly char[] _separator = new char[] { ' ' };

        private DeckDescriptor _deckDescr;
        private SequenceRng _randomDealer; 
    }
}
