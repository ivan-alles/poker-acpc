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
    /// A bot that always folds.
    /// </summary>
    public class FolderBot: BotBase
    {
        public override PokerAction OnActionRequired(string gameString)
        {
            return PokerAction.f(0);
        }
    }
}
