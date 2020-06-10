/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ai.lib.algorithms.numbers;

namespace ai.pkr.metagame
{
    /// <summary>
    /// A set of cards. Each card of a deck can be at most once in a Cardset.
    /// </summary>
    /// <remarks>
    /// The underlying data structure is a 64-bit mask. This allows very efficient 
    /// algorithms such as card evaluators and combination enumerators.
    /// 
    /// </remarks>
    [Serializable]
    public struct CardSet: IXmlSerializable
    {
        #region Public data
        /// <summary>
        /// The bitmask, one bit corresponds to one card. The deck layout (which bit represents which card)
        /// is defined by an instance of DeckDescriptor.
        /// 
        /// You can write efficient algorithms by using direct access to this data field. 
        /// Operators to convert between Cardset and UInt64 are intentionally not provided,
        /// in order to keep track on places where such low-level usage is required.
        /// </summary>
        public UInt64 bits;

        #endregion

        #region Constants
        
        /// <summary>
        /// An empty cardset. A cardset created with the default constructor is also empty.
        /// </summary>
        public static readonly CardSet Empty = new CardSet();

        #endregion

        #region Constructors
        
        /// <summary>
        /// Creates a new cardset by joining 2 cardsets.
        /// </summary>
        public CardSet(CardSet cs1, CardSet cs2)
        {
            bits = cs1.bits | cs2.bits;
        }

        #endregion

        #region Public operations

        public bool IsEmpty()
        {
            return bits == 0;
        }

        public void Clear()
        {
            bits = 0;
        }

        public int CountCards()
        {
            return CountBits.Count(bits);
        }

        /// <summary>
        /// Converts a cardset to string for DEBUGGING purposes. 
        /// <remarks>
        /// Unfortunately it is impossible to make this method correctly working,
        /// as we do not know which DeckDescriptor corresponds to this CardSet.
        /// Therefore we use here a deck descriptor specified by the static property
        /// ToStringDeckDescriptor. While debugging a deck, set this property to 
        /// your deck descriptor.
        /// 
        /// To correctly convert a cardset to string of card names, use DeckDescriptor.GetCardNames().
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ToStringDeckDescriptor != null)
            {
                return ToStringDeckDescriptor.GetCardNames(this);
            }
            return StdDeck.Descriptor.GetCardNames(this);
        }

        /// <summary>
        /// Sets DeckDescriptor to be used in ToString() method.
        /// Default value is null, a standard deck is used in this case.
        /// </summary>
        public static DeckDescriptor ToStringDeckDescriptor
        {
            set;
            get;
        }

        #region Set operations

        /// <summary>
        /// Join this cardset with another cardset.
        /// </summary>
        public void UnionWith(CardSet cs)
        {
            bits |= cs.bits;
        }

        /// <summary>
        /// Joins 2 cardsets.
        /// </summary>
        public static CardSet operator | (CardSet cs1, CardSet cs2)
        {
            return new CardSet(cs1, cs2);
        }

        /// <summary>
        /// Returns true, if this cardset contains another cardset.
        /// <remarks>
        /// 1. Non-empty cardset contains the empty cardset.
        /// 2. Empty cardset contains no other cardsets than self.
        /// 3. Non-empty cardset contains self.
        /// </remarks>
        /// </summary>
        public bool Contains(CardSet cs)
        {
            return (bits & cs.bits) == cs.bits;
        }

        /// <summary>
        /// Returns true, if this cardset is intersecting with another cardset.
        /// <remarks>
        /// 1. Empty cardset is not inersecting with any other cardset (including empty).
        /// 2. Non-empty cardset is intersecting with self.
        /// 3. This function is commutative.
        /// </remarks>
        /// </summary>
        public bool IsIntersectingWith(CardSet cs)
        {
            return (bits & cs.bits) != 0;
        }

        public static CardSet Intersect(CardSet cs1, CardSet cs2)
        {
            CardSet cs = new CardSet();
            cs.bits = cs1.bits & cs2.bits;
            return cs;
        }

        /// <summary>
        /// Same as static Interserct(). 
        /// </summary>
        public static CardSet operator & (CardSet cs1, CardSet cs2)
        {
            CardSet cs = new CardSet();
            cs.bits = cs1.bits & cs2.bits;
            return cs;
        }

        /// <summary>
        /// Removes all cards that belong to the other set from this set (set difference).
        /// </summary>
        public void Remove(CardSet cs)
        {
            bits &= ~(cs.bits);
        }

        /// <summary>
        /// Set difference cs1 - cs2.
        /// </summary>
        public static CardSet Remove(CardSet cs1, CardSet cs2)
        {
            CardSet result;
            result.bits = cs1.bits & ~(cs2.bits);
            return result;
        }

        /// <summary>
        /// Set difference cs1 - cs2.
        /// </summary>
        public static CardSet operator - (CardSet cs1, CardSet cs2)
        {
            return Remove(cs1, cs2);
        }

        #endregion

        #endregion

        #region Equality

        public static bool operator ==(CardSet x, CardSet y)
        {
            return x.bits == y.bits;
        }

        public static bool operator !=(CardSet x, CardSet y)
        {
            return x.bits != y.bits;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CardSet))
                return false;
            CardSet cs = (CardSet)obj;
            return bits == cs.bits;
        }

        public bool Equals(CardSet cs)
        {
            return bits == cs.bits;
        }

        public override int GetHashCode()
        {
            return (int)bits ^ (int)(bits >> 32);
        }

        #endregion

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                reader.MoveToContent();
                string s = reader.ReadString();
                bits = UInt64.Parse(s, System.Globalization.NumberStyles.HexNumber);
                reader.ReadEndElement();
            }
            else
                bits = 0;
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(bits.ToString("X"));
        }

        #endregion
    }
}