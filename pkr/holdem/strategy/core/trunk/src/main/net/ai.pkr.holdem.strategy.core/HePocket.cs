/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using System.Diagnostics;
using System.Xml.Serialization;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.holdem.strategy.core
{
    /// <summary>
    /// Information and operations with HE pockets.
    /// </summary>
    public static class HePocket
    {

        #region Public API

        /// <summary>
        /// Returns the number of pocket kinds (169).
        /// </summary>
        public static int Count
        {
            get { return (int)HePocketKind.__Count; }
        }

        /// <summary>
        /// Returns a single suit-normalized cards set, e.g. AKs -> AcKc, AKo -> AcKd.
        /// </summary>
        public static CardSet KindToCardSet(HePocketKind pocketKind)
        {
            return _kindToSuitNormalizedCardset[(int)pocketKind];
        }

        /// <summary>
        /// Converts card set to its poket kind enumeration. The card set need not to be suit-normalized.
        /// </summary>
        /// <remarks>This function is slower that converting the pocket to a suit-normalized form using
        /// NormSuit.</remarks>
        /// Developer notes:
        /// To speed-up this fuction, I also tried to use a sorted array, but it is even slower than the dictionary.
        /// At the moment I have no idea how to beat the performance of NormSuit! (it is about 2-3 times faster).
        public static HePocketKind CardSetToKind(CardSet pocket)
        {
            return _cardSetToKind[pocket];
        }

        /// <summary>
        /// Convert a hand of 2 cards (not necessary suit-normalized) to a pocket kind.
        /// </summary>
        public static HePocketKind HandToKind(int[] hand)
        {
            CardSet cs = StdDeck.Descriptor.GetCardSet(hand, 0, 2);
            return CardSetToKind(cs);
        }

        /// <summary>
        /// Converts a kind to a suit-normalized hand.
        /// </summary>
        public static int[] KindToHand(HePocketKind kind)
        {
            return _kindToHand[(int)kind];
        }

        public static CardSet [] KindToRange(HePocketKind kind)
        {
            return _kindToRange[(int)kind];
        }

        /// <summary>
        /// Converts a kind to a regular string representation, for example KQs.
        /// </summary>
        public static string KindToString(HePocketKind kind)
        {
            return _kindToString[(int)kind];
        }

        /// <summary>
        /// Converts a string representation (for instance AKo) to a pocket kind.
        /// </summary>
        public static HePocketKind StringToKind(string kindString)
        {
            return _stringToKind[kindString];
        }

        #endregion

        #region Implementation

        static HePocket()
        {
            int[] rangePos = new int[(int)HePocketKind.__Count];
            for (int p = 0; p < (int)HePocketKind.__Count; ++p)
            {
                HePocketKind kind = (HePocketKind)p;
                string name = kind.ToString();
                string kindString = name.Substring(1);

                _kindToString[p] = kindString;
                _stringToKind.Add(kindString, kind);

                string c1 = name.Substring(1, 1);
                string c2 = name.Substring(2, 1);
                string type = name.Length == 4 ? name.Substring(3, 1) : "";
                CardSet cs = new CardSet();
                int rangeSize = 6;
                if (type == "s")
                {
                    // Suited
                    cs = StdDeck.Descriptor.GetCardSet(c1 + "c " + c2 + "c");
                    rangeSize = 4;
                }
                else if (type == "o")
                {
                    // Offsuit
                    cs = StdDeck.Descriptor.GetCardSet(c1 + "c " + c2 + "d");
                    rangeSize = 12;
                }
                else 
                {
                    // Pair 
                    cs = StdDeck.Descriptor.GetCardSet(c1 + "c " + c2 + "d");
                    rangeSize = 6;
                }

                _kindToSuitNormalizedCardset[p] = cs;
                _kindToHand[p] = StdDeck.Descriptor.GetIndexesAscending(cs).ToArray();
                _cardSetToKind[cs] = kind;
                _kindToRange[(int)kind] = new CardSet[rangeSize];
            }
            CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty, AddPocket, rangePos);
        }

        // Create ranges for each pocket kind.
        // Also puts non-se pockets to _cardSetToKind
        private static void AddPocket(ref CardSet pocket, int [] rangePos)
        {
            NormSuit ns = new NormSuit();
            CardSet snPocket = ns.Convert(pocket);
            // The suit-normalized pocket is already added, we can use its kind.
            int kindIndex = (int)_cardSetToKind[snPocket];
            if (!_cardSetToKind.ContainsKey(pocket))
            {
                _cardSetToKind.Add(pocket, (HePocketKind)kindIndex);
            }
            _kindToRange[kindIndex][rangePos[kindIndex]] = pocket;
            rangePos[kindIndex]++;
        }

        /// <summary>
        /// Mapping kind -> single suit-normalized cardset.
        /// </summary>
        private static readonly CardSet[] _kindToSuitNormalizedCardset = new CardSet[(int)HePocketKind.__Count];

        private static readonly string[] _kindToString = new string[(int)HePocketKind.__Count];

        private static readonly int[][] _kindToHand = new int[(int)HePocketKind.__Count][];

        private static readonly Dictionary<string, HePocketKind> _stringToKind = new Dictionary<string, HePocketKind>();
        
        /// <summary>
        /// Maps any of 1326 pockets to its kind.
        /// </summary>
        private static readonly Dictionary<CardSet, HePocketKind> _cardSetToKind = new Dictionary<CardSet, HePocketKind>();

        /// <summary>
        /// Maps kind to a list of pockets.
        /// </summary>
        private static readonly CardSet[][] _kindToRange = new CardSet[(int)HePocketKind.__Count][];

        #endregion

    }
}