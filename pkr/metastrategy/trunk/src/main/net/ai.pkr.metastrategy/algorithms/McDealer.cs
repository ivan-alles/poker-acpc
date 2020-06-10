/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.algorithms.random;
using System.Diagnostics;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Monte-Carlo hands dealer.
    /// Generates random hands for one or many players. 
    /// Shared cards are copied to each player. The order and number of cards is as described in the game definition
    /// and in our conventions about deal order.
    /// </summary>
    public class McDealer
    {
        public McDealer(GameDefinition gd, int rngSeed): this(gd, new MersenneTwister(rngSeed))
        {
        }

        public McDealer(GameDefinition gd, Random underlyingRng)
        {
            List<DealKind> dealPattern = new List<DealKind>();
            for (int r = 0; r < gd.RoundsCount; ++r)
            {
                // Deal player's cards fist by our convention
                int playersCard = gd.PrivateCardsCount[r] + gd.PublicCardsCount[r];
                _playerCardCount += playersCard;
                for (int i = 0; i < playersCard; ++i)
                {
                    dealPattern.Add(DealKind.Player);
                }

                // Deal shared cards after private and public cards
                _sharedCardCount += gd.SharedCardsCount[r];
                for (int i = 0; i < gd.SharedCardsCount[r]; ++i)
                {
                    dealPattern.Add(DealKind.Shared);
                }

            }
            _dealPattern = dealPattern.ToArray();
            _dealer = new SequenceRng(underlyingRng, gd.DeckDescr.FullDeckIndexes);
            Debug.Assert(_dealPattern.Length == _playerCardCount + _sharedCardCount);
        }


        public int HandSize
        {
            get { return _sharedCardCount + _playerCardCount; }
        }

        /// <summary>
        /// Deals random cards to each player for all rounds.
        /// </summary>
        /// <param name="hands"></param>
        public void NextDeal(int[][] hands)
        {
            int totalCardCount = _playerCardCount * hands.Length + _sharedCardCount;

            // Shuffle cards for all rounds. 
            // Let's reserve the cards at the beginning for shared cards.
            _dealer.Shuffle(totalCardCount);

#if false // For debugging
                Debug.Write("Random cards: ");
                for(int i = 0; i < totalCardCount; ++i)
                {
                    Debug.Write(String.Format(" {0}", Dealer.Sequence[i]));
                }
                Debug.WriteLine("");
#endif
            for (int p = 0; p < hands.Length; ++p)
            {
                Debug.Assert(HandSize == hands[p].Length);

                int dealtShared = 0;
                int dealtPlayer = 0;

                int playerCardsStart = p*_playerCardCount + _sharedCardCount;

                int[] hand = hands[p];
                
                for (int d = 0; d < _dealPattern.Length; ++d)
                {
                    DealKind dk = _dealPattern[d];
                    switch(dk)
                    {
                        case DealKind.Player:
                            hand[d] = _dealer.Sequence[playerCardsStart + dealtPlayer++];
                            break;
                        case DealKind.Shared:
                            hand[d] = _dealer.Sequence[dealtShared++];
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Number of private or public cards for each player.
        /// </summary>
        int _playerCardCount;

        /// <summary>
        /// Number of shared cards
        /// </summary>
        int _sharedCardCount;


        enum DealKind
        {
            Shared,
            Player
        };

        DealKind[] _dealPattern;


        SequenceRng _dealer;
    }
}
