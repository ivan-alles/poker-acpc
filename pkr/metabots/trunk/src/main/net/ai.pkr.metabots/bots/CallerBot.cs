/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.metabots.bots
{
    /// <summary>
    /// A bot that always calls.
    /// </summary>
    public class CallerBot : BotBase
    {
        public override PokerAction OnActionRequired(string gameString)
        {
            return PokerAction.c(0);
        }
    }
}
