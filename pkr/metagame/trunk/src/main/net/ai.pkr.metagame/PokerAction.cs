/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ai.pkr.metagame
{
    /// <summary>
    /// An action of a poker game.
    /// </summary>
    [Serializable]
    public class PokerAction
    {

        #region Constructors

        public PokerAction()
        {
            Cards = "";
        }

        public PokerAction(Ak kind, int position, double amount, string cards)
        {
            Kind = kind;
            Position = position;
            Amount = amount;
            Cards = cards;
        }

        public PokerAction(PokerAction other)
        {
            Kind = other.Kind;
            Position = other.Position;
            Amount = other.Amount;
            Cards = other.Cards;
        }

        #endregion

        #region Public properties and methods

        public Ak Kind
        {
            set;
            get;
        }

        /// <summary>Position of the acting player. Valid for c, f, r, d, p.</summary>
        public int Position
        {
            set
            {
                _position = value;
            }
            get
            {
                return _position;
            }
        }

        /// <summary>Amount of the action, if applicable (c, r), otherwise 0. The amount is measured
        /// from the level of the last bet of the current round.
        /// <para>For raises the amount is mandatory.</para>
        /// <para>For calls is optional and can be:</para>
        /// <para>- 0 - a regular call. A check also falls into this category.</para>
        /// <para>- a negative value for an all-in call.</para>
        /// <para>- Notice that a positive value for calls is not feasible (it would be a raise). But this cannot be always checked,
        /// because the kind and amount can be set separately.</para>
        /// </summary>
        public double Amount
        {
            set
            {
                _amount = value;
            }
            get
            {
                return _amount;
            }
        }

        /// <summary>Cards of the action if applicable, or empty string.
        /// If the cards are applicable (e.g. for d) but unknown, ? is set instead of each card.
        /// </summary>
        public string Cards
        {
            set;
            get;
        }

        public bool IsPlayerAction()
        {
            return Kind >= Ak.f;
        }

        public bool IsPlayerAction(int position)
        {
            return Kind >= Ak.f && position == Position;
        }

        public bool IsDealerAction()
        {
            return Kind  == Ak.d;
        }

        /// <summary>
        /// Returns true if the actor was the player at the given position (but not the dealer).
        /// </summary>
        public bool HasPlayerActed(int pos)
        {
            return Position == pos && IsPlayerAction();
        }

        /// <summary>
        /// Convert to the textual representation of game record.
        /// </summary>
        public string ToGameString()
        {
            StringBuilder sb = new StringBuilder();
            ToGameString(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Convert to the textual representation of game record.
        /// </summary>
        public static string ToGameString(PokerAction [] actions)
        {
            StringBuilder sb = new StringBuilder();
            for (int a = 0; a < actions.Length; a++)
            {
                actions[a].ToGameString(sb);
                if (a < actions.Length - 1)
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses a string of actions in game string format.
        /// </summary>
        public static bool Parse(string actionString, List<PokerAction> actions, out string error)
        {
            char term;
            error = "";
            return GameStringParser.ParseActions(actionString, 0, int.MaxValue, actions, ref error, out term);
        }

        /// <summary>
        /// Verifies data consistency. Is never called implicitely by other methods of this class, because
        /// the object can be constructed step by step and may be invalid in the middle.
        /// </summary>
        public void Verify()
        {
            switch (Kind)
            {
                case Ak.c:
                    if (Amount > 0)
                    {
                        throw new ApplicationException(String.Format("Wrong amount for call (should be <=0): {0}",
                                                                        Amount));
                    }
                    break;
                case Ak.f:
                    if (Amount != 0)
                    {
                        throw new ApplicationException(String.Format("Wrong amount for fold (should be 0): {0}",
                                                                        Amount));
                    }
                    break;
                case Ak.r:
                    if (Amount <= 0)
                    {
                        throw new ApplicationException(String.Format("Wrong amount for raise (should be > 0): {0}",
                                                                        Amount));
                    }
                    break;
                case Ak.d:
                    if (Amount != 0)
                    {
                        throw new ApplicationException(String.Format("Wrong amount for deal (should be 0): {0}",
                                                                        Amount));
                    }
                    break;
            }
        }

        #endregion

        #region Overridables

        public override string ToString()
        {
            return ToGameString();
        }

        public override bool Equals(object obj)
        {
            PokerAction o = obj as PokerAction;
            return Equals(o);
        }

        public bool Equals(PokerAction o)
        {
            if (o == null)
                return false;
            return Kind == o.Kind &&
                   Position == o.Position &&
                   Amount == o.Amount &&
                   Cards == o.Cards;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion


        #region Static methods

        public static PokerAction d(int position, string cards)
        {
            return new PokerAction(Ak.d, position, 0, cards);
        }

        public static PokerAction d(string cards)
        {
            return new PokerAction(Ak.d, -1, 0, cards);
        }


        public static PokerAction f(int position)
        {
            return new PokerAction(Ak.f, position, 0, "");
        }

        public static PokerAction c(int position)
        {
            return new PokerAction(Ak.c, position, 0, "");
        }

        public static PokerAction c(int position, double amount)
        {
            return new PokerAction(Ak.c, position, amount, "");
        }

        public static PokerAction r(int position, double amount)
        {
            return new PokerAction(Ak.r, position, amount, "");
        }

        public static bool IsPlayerAction(Ak kind)
        {
            return kind >= Ak.f;
        }

        public static bool IsDealerAction(Ak kind)
        {
            return kind  == Ak.d;
        }

        /// <summary>
        /// Converts a character to Ak. If a character is invalid, Ak.b is returned.
        /// </summary>
        public static Ak AkFromChar(char ch)
        {
            return _actionCharToKindMap[ch];
        }

        /// <summary>
        /// Converts a character to Ak. If a character is invalid, false is returned.
        /// </summary>
        public static bool AkFromChar(char ch, out Ak kind)
        {
            kind = _actionCharToKindMap[ch];
            return kind != (Ak) (-1);
        }

        #endregion

        #region Private methods

        static PokerAction()
        {
            for(int i = 0; i < _actionCharToKindMap.Length; ++i)
            {
                _actionCharToKindMap[i] = (Ak)(-1);
            }
            _actionCharToKindMap['b'] = Ak.b;
            _actionCharToKindMap['d'] = Ak.d;
            _actionCharToKindMap['r'] = Ak.r;
            _actionCharToKindMap['c'] = Ak.c;
            _actionCharToKindMap['f'] = Ak.f;
        }

        void ToGameString(StringBuilder sb)
        {
            switch (Kind)
            {
                case Ak.d:
                    if (Position < 0)
                    {
                        sb.AppendFormat("d{{{0}}}", Cards);
                    }
                    else
                    {
                        sb.AppendFormat("{0}d{{{1}}}", Position, Cards);
                    }
                    break;
                case Ak.r:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}r{1:0.####}", Position, Amount);
                    break;
                case Ak.c:
                    sb.AppendFormat("{0}c", Position);
                    if(Amount != 0)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0:0.####}", Amount);
                    }
                    break;
                case Ak.f:
                    sb.AppendFormat("{0}f", Position);
                    break;
                case Ak.b:
                    sb.Append("b");
                    break;
                default:
                    Debug.Assert(false);
                    sb.Append("???");
                    break;
            }
        }

        #endregion

        #region Private data

        private static Ak[] _actionCharToKindMap = new Ak[256];

        private int _position;
        private double _amount;
        #endregion

    }
}