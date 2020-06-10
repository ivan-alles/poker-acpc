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
    /// A bot that always raises if possible, otherwise calls.
    /// </summary>
    public class RaiserBot : BotBase
    {
        public override PokerAction OnActionRequired(string gameString)
        {
            base.OnActionRequired(gameString);
            if (CurGameState.BetCount < GameDefinition.BetsCountLimits[CurGameState.Round])
                return PokerAction.r(0,0);
            return PokerAction.c(0);
        }
    }
}
