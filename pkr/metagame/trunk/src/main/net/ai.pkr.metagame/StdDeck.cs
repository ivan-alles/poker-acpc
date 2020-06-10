/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.metagame
{
    /// <summary>
    /// The description of a standard 52-card deck and a couple of helper functions.
    /// </summary>
    public static class StdDeck
    {
        private static readonly string[] _cardNames =
            new string[]
        {
             "2c", "3c", "4c", "5c", "6c", "7c", "8c", "9c", "Tc", "Jc", "Qc", "Kc", "Ac",
             "2d", "3d", "4d", "5d", "6d", "7d", "8d", "9d", "Td", "Jd", "Qd", "Kd", "Ad",
             "2h", "3h", "4h", "5h", "6h", "7h", "8h", "9h", "Th", "Jh", "Qh", "Kh", "Ah",
             "2s", "3s", "4s", "5s", "6s", "7s", "8s", "9s", "Ts", "Js", "Qs", "Ks", "As"
        };

        static StdDeck()
        {
            CardSet[] cardsets = new CardSet[_cardNames.Length];
            for(int s = 0; s < SuitCount; ++s)
            {
                for (int r = 0; r < RankCount; ++r)
                {
                    cardsets[s * RankCount + r].bits = 1ul << (s * 16 + r);
                }
            }

            Descriptor = new DeckDescriptor("StdDeck", _cardNames, cardsets);
        }

        public const int RankCount = 13;
        public const int SuitCount = 4;

        public static readonly DeckDescriptor Descriptor;

        #region Rank and suit

        public enum Suit
        {
            Clubs = 0,
            Diamonds = 1,
            Hearts = 2,
            Spades = 3,
            _NumberOfSuits
        }

        public enum Rank
        {
            r_2 = 0, r_3, r_4, r_5, r_6, r_7, r_8, r_9, r_T, r_J, r_Q, r_K, r_A, _NumberOfRanks
        }

        public static int RankFromString(string rank)
        {
            if (rank.Length != 1)
                throw new ArgumentException("Unexpected rank name length");

            return _rankNames.IndexOf(rank[0]);
        }

        public static int RankFromChar(char r)
        {
            return _rankNames.IndexOf(r);
        }

        public static int SuitFromString(string suit)
        {
            if (suit.Length != 1)
                throw new ArgumentException("Unexpected suit name length");

            return _suitNames.IndexOf(suit[0]);
        }

        public static int SuitFromChar(char s)
        {
            return _suitNames.IndexOf(s);
        }

        public static int IndexFromRankAndSuit(int rank, int suit)
        {
            return rank + suit * 13;
        }

        public static int GetRank(int c)
        {
            return c % 13;
        }

        public static int GetSuit(int c)
        {
            return c / 13;
        }

        /// <summary>
        /// Compares two card indexes, first compares the ranks, than the suites.
        /// </summary>
        public static bool LessRS(int c1, int c2)
        {
            int r1 = GetRank(c1);
            int r2 = GetRank(c2);
            if (r1 == r2)
                return GetSuit(c1) < GetSuit(c2);
            return r1 < r2;
        }

        public static bool RankEquals(int c1, int c2)
        {
            return GetRank(c1) == GetRank(c2);
        }

        public static bool SuitEquals(int c1, int c2)
        {
            return GetSuit(c1) == GetSuit(c2);
        }

        #endregion

        #region Implementation
        const string _rankNames = "23456789TJQKA";
        const string _suitNames = "cdhs";
        #endregion
    }
}
