/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.numbers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ai.pkr.metastrategy
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ChanceTreeNode: IChanceTreeNode
    {
        #region IChanceTreeNode Members

        public double Probab
        {
            get;
            set;
        }

        public void GetPotShare(ushort activePlayers, double[] potShare)
        {
            int bitCount = CountBits.Count(activePlayers);
            // Now implement for heads-up only.
            if(bitCount < 1 || bitCount > 3)
            {
                throw new ApplicationException(String.Format("Wrong active player mask {0}, players count must be in range [1..2]", activePlayers));
            }
            if (bitCount == 1)
            {
                for (int p = 0; p < potShare.Length; ++p)
                {
                    // Todo: for rake we can sum all _potShare values.
                    // For non-leaves we will need to store the value to an index 0, for example.
                    // Think how this will influence creating abstract chance tree by game def.
                    if ((activePlayers & (1 << p)) == 0)
                    {
                        potShare[p] = 0;
                    }
                    else
                    {
                        potShare[p] = 1;
                    }
                }
            }
            else
            {
                potShare[0] = _potShare0;
                potShare[1] = 1.0 - _potShare0;
            }
        }

        #endregion

        #region IDealerAction Members

        public int Card
        {
            get { return _card; }
            set { _card = (byte)value; }
        }

        #endregion

        #region IStrategicAction Members

        public int Position
        {
            get { return _position; }
            set { _position = (sbyte)value; }
        }

        ///<summary>
        /// If parameters is a string[] with card names, shows card names instead of indexes.
        /// </summary>
        public string ToStrategicString(object parameters)
        {
            return StrategicString.ToStrategicString(Position, Card, parameters);
        }

        #endregion

        public override string ToString()
        {
            return ToStrategicString(null);
        }

        public void SetPotShare(ushort activePlayers, double[] potShare)
        {
            int bitCount = CountBits.Count(activePlayers);
            // Now implement for heads-up only.
            if (bitCount < 1 || bitCount > 3)
            {
                throw new ApplicationException(String.Format("Wrong active player mask {0}, players count must be in range [1..2]", activePlayers));
            }
            if (bitCount == 1)
            {
                return; 
            }
            _potShare0 = potShare[0];
            Debug.Assert(FloatingPoint.AreEqual(1.0, _potShare0 + potShare[1], 1e-12));
        }

        /// <summary>
        /// Pot share for pos 0.
        /// </summary>
        double _potShare0;

        byte _card;
        sbyte _position;

    }
}
