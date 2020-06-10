/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ai.pkr.metastrategy
{
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StrategyTreeNode: IStrategyTreeNode
    {
        #region IStrategyTreeNode Members

        public bool IsDealerAction
        {
            set 
            {
                if (value)
                {
                    _id |= 1;
                }
                else
                {
                    _id &= ~(1u);
                }
                Debug.Assert(value == IsDealerAction);
            }
            get 
            {
                return (_id & 1) == 1;
            }
        }

        public bool IsPlayerAction(int position)
        {
            return !IsDealerAction && position == Position;
        }


        /// <summary>
        /// Position [0..7]. For the player is the position of the player. For the root is the number of player in the game.
        /// For dealer nodes is 0.
        /// </summary>
        public int Position
        {
            set
            {
                Debug.Assert(0 <= value && value <= 7);
                _id &= ~(0xEu);
                _id |= ((uint)value << 1);
                Debug.Assert(value == Position);
            }
            get
            {
                return (int)((_id & 0xEu) >> 1);
            }
        }

        public int Card
        {
            set
            {
                _id &= 0xF;
                _id |= ((uint)value << 4);
                Debug.Assert(value == Card);
            }
            get
            {
                Debug.Assert(IsDealerAction);
                return (int)((_id & 0xFFFFFFF0u) >> 4);
            }
        }


        /// <summary>
        /// Amount [0.0..2000.0], up to 5 decimal digits.
        /// </summary>
        public double Amount
        {
            set
            {
                Debug.Assert(0 <= value && value <= 2000);
                int amount = (int)Math.Round(value / AMOUNT_FACTOR, 0);
                _id &= 0xF;
                _id |= ((uint)amount << 4);
                Debug.Assert(value == Amount);
            }
            get
            {
                int amount = (int)((_id & 0xFFFFFFF0u) >> 4);
                return Math.Round(amount * AMOUNT_FACTOR, 5);
            }
        }

        /// <summary>
        /// Probability of the player making this move. Non-players is ignored.
        /// </summary>
        public double Probab
        {
            get { return _probab; }
            set { _probab = value; }
        }

        ///<summary>
        /// If parameters is a string[] with card names, shows card names instead of indexes.
        /// </summary>
        public string ToStrategicString(object parameters)
        {
            if (IsDealerAction)
            {
                return StrategicString.ToStrategicString(Position, Card, parameters);
            }
            else
            {
                return StrategicString.ToStrategicString(Position, Amount, parameters);
            }
        }

        #endregion

        public override string ToString()
        {
            return ToStrategicString(null);
        }

        // Layout (bits):
        // 0: 0 - player, 1 - dealer
        // 1..3: position (0..7)
        // 4-31: 
        //   for player: amount devided by AMOUNT_FACTOR and converted to integer.
        //   for dealer: card index.
        private const double AMOUNT_FACTOR = 0.00001;

        UInt32 _id;
        double _probab;
    }
}
