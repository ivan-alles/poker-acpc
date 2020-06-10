/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ai.pkr.stdpoker.tablegen
{
    class Program
    {
        static void Main(string[] args)
        {
            LutStraightGenerator st = new LutStraightGenerator();
            st.Generate("LutStraight.cs");
            LutTopCardGenerator tc = new LutTopCardGenerator();
            tc.Generate("LutTopCard.cs");
            LutTopFiveCardsGenerator t5 = new LutTopFiveCardsGenerator();
            t5.Generate("LutTopFiveCards.cs");
        }
    }
}
