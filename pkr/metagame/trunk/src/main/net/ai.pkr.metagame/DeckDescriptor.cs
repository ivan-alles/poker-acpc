/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.numbers;
using System.Xml.Serialization;
using ai.lib.utils;
using ai.lib.algorithms;

namespace ai.pkr.metagame
{
    /// <summary>
    /// A description of a deck. It contains an array of card names and an array of cardsets
    /// showing wich bit correspond to that name (each cardset must have exactly one bit set).
    /// <para>
    /// The array of cardsets must be ordered ascending.</para>
    /// <para>
    /// For example, for Kuhn poker:</para>
    /// <para>
    /// Cardnames: {"J",   "Q",  "K"}</para>
    /// <para>
    /// CardSets:  {0x01, 0x02, 0x04}</para>
    /// <remarks>
    /// <para>
    /// If your deck uses suits, please follow the following convention for the deck layout:</para>
    /// <para>
    /// 1. All cards of one sutes are located  in a 16-bit area of a cardset, 
    /// suit 0 in bits 0..15, suite 1 in bits 16..31, etc.</para>
    /// <para>
    /// For example, standard deck will have the following layout:</para>
    /// <para>
    /// UUUSSSSSSSSSSSSS UUUHHHHHHHHHHHHH UUUDDDDDDDDDDDDD UUUCCCCCCCCCCCCC</para>
    /// <para>
    /// Where: U is an unused bit; S, H, D, C are spades, hearts, diamonds, clubs respectively.</para>
    /// <para>
    /// 2. Orders of cards in each suit are the same:</para>
    /// <para>
    /// For example, a correct layout is:</para>
    /// <para>
    /// 2c, 3c, 4c, ..., Ac; 2d, 3d, 4d, ..., Ad; ...</para>
    /// <para>
    /// A wrong layout:</para>
    /// <para>
    /// 2c, 3c, 4c, ..., Ac; Ad, Kd, Qd, ..., 2d; ...</para>
    /// <para></para>
    /// <para>
    /// These assumptions allow to write efficient algoritms for suit processing without having to
    /// know how many suite are there, how many card they contain and where their cards are in the bit mask.
    /// An example of such an algorithm is suite equivalent isomorphism.</para>
    /// <para>Three card formats (string, indexes and cardsets) are supported because there are algorithms
    /// that require these formats. Which format is better depends on the use case. Of these three formats 
    /// strings is the most flexible and human readable. The cardset allow compact representation of the whole deck.
    /// Indexes can be easily and quickly converted into 2 others, the reverse conversion is possibly but is not so fast.
    /// </para>
    /// <para>
    /// Decks with multiple cards with the same name are allowed, because it can be quite useful 
    /// for some experimental games, such as Leduc HE. The property HasDuplicates is true for such decks.
    /// </para>
    /// <para>
    /// Example:</para>
    /// <para>
    /// Card CS1 CS1</para>
    /// <para>
    /// J 0x400 0x4000000</para>
    /// <para>
    /// Q 0x800 0x8000000</para>para>
    /// <para>
    /// K 0x1000 0x10000000</para>
    /// <para>
    /// Methods that convert strings to indexes or cardset will convert a card name to the same index,
    /// but there is now assumption which of many possible will be used.</para>
    ///</remarks>
    /// </summary>
    [XmlRoot("DeckDescriptor", Namespace = "ai.pkr.metagame.DeckDescriptor.xsd")]
    [Serializable]
    public class DeckDescriptor
    {
        #region Constructors

        /// <summary>
        /// Constructor for serialization only.
        /// </summary>
        [Obsolete("Used for XML serialization only")]
        public DeckDescriptor()
        {
        }

        /// <summary>
        /// Creates a new object. 
        /// <remarks>Parameters will be cloned internally.</remarks>
        /// </summary>
        public DeckDescriptor(string name, string [] cardNames, CardSet [] cardSets)
        {
            _cardNames = (string[])cardNames.Clone();
            _cardSets = (CardSet[])cardSets.Clone();
            Name = name;
            Initialize();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the deck.
        /// <remarks> Do not call the setter, it is for XML serialization only! </remarks>
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        [XmlIgnore]
        public int Size
        {
            get
            {
                return CardNames.Length;
            }
        }

        /// <summary>
        /// Names of the cards.
        /// <remarks> Do not call the setter, it is for XML serialization only! </remarks>
        /// </summary>
        [XmlArrayItem("i")]
        public string[] CardNames
        {
            get
            {
                return _cardNames;
            }

            set
            {
                _cardNames = value;
            }
        }

        /// <summary>
        /// Cards sets for each card.
        /// <remarks> Do not call the setter, it is for XML serialization only! </remarks>
        /// </summary>
        [XmlArrayItem("i")]
        public CardSet[] CardSets
        {
            get
            {
                return _cardSets;
            }

            set
            {
                _cardSets = value;
            }
        }

        [XmlIgnore]
        public CardSet FullDeck
        {
            get
            {
                return _fullDeck;
            }
        }

        [XmlIgnore]
        public int[] FullDeckIndexes
        {
            get
            {
                return _fullDeckIndexes;
            }
        }

        /// <summary>
        /// Is true if the deck containss multiple cards with the same name.
        /// </summary>
        [XmlIgnore]
        public bool HasDuplicates
        {
            get;
            private set;
        }

        #endregion

        #region Index methods


        /// <summary>
        /// Gets index of one card.
        /// </summary>
        public int GetIndex(string card)
        {
            return _cardNameToIndex[card];
        }

        /// <summary>
        /// Gets array of indexes of multiple cards, separated by spaces.
        /// Indexes are in the same order as the cards in the string.
        /// </summary>
        public int[] GetIndexes(string cards)
        {
            string[] cardStrings = cards.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] result = new int[cardStrings.Length];
            for (int i = 0; i < cardStrings.Length; ++i)
            {
                result[i] = GetIndex(cardStrings[i]);
            }
            return result;
        }


        /// <summary>
        /// Returns an array of indexes corresponding to cards in the cardset.
        /// Indexes are ordered ascending.
        /// </summary>
        /// <returns></returns>
        public List<int> GetIndexesAscending(CardSet cardSet)
        {
            List<int> result = new List<int>(52);
            for (int c = 0; c < (int)_cardSets.Length; ++c)
            {
                if ((cardSet.bits & _cardSets[c].bits) != 0)
                {
                    result.Add(c);
                    cardSet.bits &= ~_cardSets[c].bits;
                    if (cardSet.bits == 0)
                        return result;
                }
            }
            return result;
        }

        #endregion

        #region CardSet methods

        /// <summary>
        /// Returns a cardset corresponding to the card string, containing one or more cards,
        /// separated by spaces.
        /// </summary>
        public CardSet GetCardSet(string cards)
        {
            // Code is optimized for speed.

            CardSet result = new CardSet();

            int begin = 0, end;
            int cardsLength = cards.Length;

            // Skip whitespace.
            for(;;begin++)
            {
                if (begin >= cardsLength) return result;
                if(cards[begin] != ' ') break;
            }
            end = begin + 1;

            for (; ; end++)
            {
                if (end == cardsLength || cards[end] == ' ')
                {
                    int idx = GetIndex(cards.Substring(begin, end - begin));
                    result.UnionWith(CardSets[idx]);
                    begin = end + 1;
                    // Skip whitespace.
                    for (; ;begin++)
                    {
                        if (begin >= cardsLength) return result;
                        if (cards[begin] != ' ') break;
                    }
                    end = begin;
                }
            }
        }

        public CardSet GetCardSet(string[] cards)
        {
            return GetCardSet(cards, 0, cards.Length);
        }

        public CardSet GetCardSet(string[] cards, int start, int count)
        {
            CardSet result = new CardSet();
            for (int i = 0; i < count; ++i)
            {
                result.UnionWith(CardSets[GetIndex(cards[start + i])]);
            }
            return result;
        }

        public CardSet GetCardSet(int[] indexes)
        {
            return GetCardSet(indexes, 0, indexes.Length);
        }

        public CardSet GetCardSet(int[] indexes, int start, int count)
        {
            CardSet result = new CardSet();
            for (int i = 0; i < count; ++i)
            {
                result.UnionWith(CardSets[indexes[start + i]]);
            }
            return result;
        }

        #endregion

        #region Card names methods

        public string GetCardNames(CardSet cardSet)
        {
            StringBuilder sb = new StringBuilder(64*3);
            int count = 0;
            UInt64 cardBits = cardSet.bits;
            for (int r = 15; r >= 0; --r)
            {
                for (int s = 0; s <= 4; ++s)
                {
                    int bitPos = s*16 + r;
                    UInt64 bit = 1ul << bitPos;

                    if ((cardBits & bit) != 0)
                    {
                        if (count++ > 0)
                            sb.Append(' ');
                        sb.Append(_bitPosToCardName[bitPos]);
                        cardBits &= ~bit;
                        if (cardBits == 0)
                            goto end;
                    }
                }
            }
            end:
            return sb.ToString();
        }

        public string GetCardNames(int[] indexes)
        {
            return String.Join(" ", GetCardNamesArray(indexes, 0, indexes.Length));
        }

        public string GetCardNames(int[] indexes, int start, int count)
        {
            return String.Join(" ", GetCardNamesArray(indexes, start, count));
        }

        public string[] GetCardNamesArray(CardSet cardSet)
        {
            string[] result = new string[cardSet.CountCards()];
            int count = 0;
            UInt64 cardBits = cardSet.bits;

            for (int r = 15; r >= 0; --r)
            {
                for (int s = 0; s <= 4; ++s)
                {
                    int bitPos = s * 16 + r;
                    UInt64 bit = 1ul << bitPos;

                    if ((cardBits & bit) != 0)
                    {
                        result[count++] = _bitPosToCardName[bitPos];
                        cardBits &= ~bit;
                        if (cardBits == 0)
                            goto end;
                    }
                }
            }
        end:
            return result;
        }

        public string[] GetCardNamesArray(int[] indexes)
        {
            return GetCardNamesArray(indexes, 0, indexes.Length);
        }

        public string[] GetCardNamesArray(int[] indexes, int start, int count)
        {
            string [] result = new string[count];
            for(int i = 0; i < count; ++i)
            {
                result[i] = CardNames[indexes[start + i]];
            }
            return result;
        }

        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            DeckDescriptor o = obj as DeckDescriptor;
            return Equals(o);
        }

        public bool Equals(DeckDescriptor o)
        {
            if (o == null)
                return false;
            return Name == o.Name && _cardNames.EqualsTo(o._cardNames)
                   && _cardSets.EqualsTo(o._cardSets);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ _cardNames.GetHashCode() ^ _cardSets.GetHashCode();
        }
        #endregion

        #region XML serialization

        public void ConstructFromXml(ConstructFromXmlParams parameters)
        {
            Initialize();
        }

        #endregion

        #region Implementation

        private string[] _cardNames;
        private CardSet[] _cardSets;
        private string[] _bitPosToCardName = new string[64];
        private CardSet _fullDeck;
        private int[] _fullDeckIndexes;
        private Dictionary<string, int> _cardNameToIndex = new Dictionary<string,int>();

        private void Initialize()
        {
            if (_cardNames.Length != _cardSets.Length)
                throw new ApplicationException("Size of card names array does not match the size of card bits array");

            _fullDeckIndexes = new int[_cardNames.Length];

            HasDuplicates = false;
            HashSet<string> distinctCards = new HashSet<string>();

            for (int i = 0; i < _cardSets.Length; ++i)
            {
                _fullDeckIndexes[i] = i;

                if (distinctCards.Contains(_cardNames[i]))
                {
                    HasDuplicates = true;
                }
                else
                {
                    distinctCards.Add(_cardNames[i]);
                }

                if (_cardSets[i].CountCards() != 1)
                {
                    throw new ApplicationException(String.Format("CardSet for {0} contains {1} cards instead of 1",
                                                                 _cardNames[i], _cardSets[i].CountCards()));
                }
                if (i > 0 && (_cardSets[i].bits <= _cardSets[i - 1].bits))
                {
                    throw new ApplicationException(String.Format("CardSets are not ordered ascending, wrong at index {0}",
                                                                 i));
                }
                for (int j = i + 1; j < _cardSets.Length; ++j)
                {
                    if (_cardSets[i].IsIntersectingWith(_cardSets[j]))
                    {
                        throw new ApplicationException(String.Format("CardSet for {0} intersects with cardset for {1}",
                                                                     _cardNames[i], _cardNames[j]));
                    }
                }
                int bitPos = CountBits.Count(_cardSets[i].bits - 1ul);
                _bitPosToCardName[bitPos] = _cardNames[i];
                _fullDeck.UnionWith(_cardSets[i]);

                // We need this check for decks that contain multiple cards with the same name, like Leduc.
                if (!_cardNameToIndex.ContainsKey(_cardNames[i]))
                {
                    _cardNameToIndex[_cardNames[i]] = i;
                }
            }
        }

        #endregion
    }
}
