/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace ai.pkr.metagame
{
    [Serializable]
    public class PlayerState
    {
        #region Constructors

        public PlayerState()
        {
            Name = "";
            Hand = "";
        }

        public PlayerState(PlayerState other)
        {
            Name = other.Name;
            Stack = other.Stack;
            Hand = other.Hand;
            InPot = other.InPot;
            Bet = other.Bet;
            Result = other.Result;
            IsFolded = other.IsFolded;
            IsAllIn = other.IsAllIn;
            _canActInCurrentRound = other._canActInCurrentRound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Player name, should be globally unique and constant over time to 
        /// allow bot to build a database.
        /// </summary>
        public string Name
        {
            set;
            get;
        }

        public double Stack { set; get; }

        /// <summary>
        /// Space-separated cards of the player, in order of deals.
        /// </summary>
        public string Hand
        {
            set;
            get;
        }

        /// <summary>
        /// Total amount of money put to the pot, including the current round.
        /// </summary>
        public double InPot { set; get; }

        /// <summary>
        /// Current bet (in the current game round).
        /// </summary>
        public double Bet
        {
            set;
            get;
        }

        /// <summary>
        /// Result of the game (winngings/losses).
        /// Is valid when the game is over.
        /// 
        /// PotShare = Result + InPot.
        /// StackAfterGame = StackBeforeGame + Result.
        /// </summary>
        public double Result
        {
            set; get;
        }

        public bool IsFolded { set; get; }
        public bool IsAllIn { set; get; }

        /// <summary>
        /// Shows if the player still can act in the current round if no raise occurs.
        /// <para>Is set to true:</para>
        /// <para> - after a deal (in a new round). This is not quite correct because the dealer must deal all cards first,
        /// but is not so important and easy to implement.</para>
        /// <para> - after a raise of an opponent</para> 
        /// <para>Is set to false:</para> 
        /// <para> - at the beginning of the game, before any deals.</para>
        /// <para> - when this player raises or calls.</para>
        /// <para> - when the player folds or goes all-in, after that this property remains false until the end of the game.</para>
        /// </summary>
        public bool CanActInCurrentRound
        {
            set
            {
                _canActInCurrentRound = value;
            }
            get
            {
                return CanActInGame && _canActInCurrentRound;
            } 
        }

        /// <summary>
        /// Shows if the player can act in the game (not all-in and not folded).
        /// After game over keeps its value, although it has no meaning.
        /// </summary>
        public bool CanActInGame
        {
            get { return !IsFolded && !IsAllIn; }
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            PlayerState o = obj as PlayerState;
            return Equals(o);
        }

        public bool Equals(PlayerState o)
        {
            if (o == null)
                return false;

            return Name == o.Name &&
                   Stack == o.Stack &&
                   Hand == o.Hand &&
                   InPot == o.InPot &&
                   Bet == o.Bet &&
                   Result == o.Result &&
                   IsFolded == o.IsFolded &&
                   IsAllIn == o.IsAllIn &&
                   CanActInCurrentRound == o.CanActInCurrentRound;
        }

        public override int GetHashCode()
        {
            return (int)Stack ^ (int)InPot ^ (int)Bet ^ (IsFolded ? 0x10000000 : 0) ^ (IsAllIn ? 0x20000000 : 0);
        }

        public override string ToString()
        {
            return string.Format("{0} c:{1} s:{2} ip:{3} b:{4}", 
                IsFolded ? "folded" : IsAllIn ? "all-in" : CanActInCurrentRound ? "wait  " : "acted ",
                Hand, Stack, InPot, Bet);
        }

        #endregion

        #region Implementation

        internal double ReachAmountInPot(double amountToReach)
        {
            double amountToPut = amountToReach - Bet;
            PutAmountInPot(amountToPut);
            return amountToPut;
        }

        internal void PutAmountInPot(double amountToPut)
        {
            Bet += amountToPut;
            InPot += amountToPut;
            Stack -= amountToPut;
        }

        private bool _canActInCurrentRound;

        #endregion
    }
}