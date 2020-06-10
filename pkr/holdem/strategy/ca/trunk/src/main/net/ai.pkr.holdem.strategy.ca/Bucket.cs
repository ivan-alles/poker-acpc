/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Collections.Generic;
using System.Text;

namespace ai.pkr.holdem.strategy.ca
{
    public class Bucket
    {
        public List<McHand> Hands = new List<McHand>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(McHand h in Hands)
            {
                sb.Append(h.ToString());
                sb.Append(", ");
            }
            return sb.ToString();
        }

        public int Length
        {
            get { return Hands.Count; }
        }
    }
}