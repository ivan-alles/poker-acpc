/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ai.pkr.metastrategy
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ActionTreeNode: IActionTreeNode
    {
        #region IPlayerAction Members

        public double Amount
        {
            set;
            get;
        }

        #endregion

        #region IStrategicAction Members

        public int Position
        {
            get { return _position; }
            set { _position = (sbyte)value; }
        }

        public string ToStrategicString(object parameters)
        {
            return StrategicString.ToStrategicString(Position, Amount, parameters);
        }

        #endregion

        #region IActionTreeNode Members

        public UInt16 ActivePlayers
        {
            set;
            get;
        }

        #endregion


        #region IActionTreeNode Members


        public int Round
        {
            get { return _round; }
            set { _round = (sbyte)value; }
        }

        #endregion

        public override string ToString()
        {
            return ToStrategicString(null);
        }

        sbyte _position;
        sbyte _round;
    }
}
