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
    /// A bot that can be used to explore a fixed strategy.
    /// It never folds and chooses between raise and call with 50% probability.
    /// </summary>
    public class ProbeBot : BotBase
    {

        public override PokerAction OnActionRequired(string gameString)
        {
            base.OnActionRequired(gameString);
            List<Ak> possibleActions = CurGameState.GetAllowedActions(GameDefinition);
            possibleActions.Remove(Ak.f);
            int i = _rng.Next(possibleActions.Count);
            PokerAction action = new PokerAction(possibleActions[i], 0, 0, "");
            return action;
        }

        private Random _rng = new Random();
    }
}
