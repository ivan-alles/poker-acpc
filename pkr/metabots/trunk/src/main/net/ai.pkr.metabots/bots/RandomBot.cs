/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using ai.pkr.metagame;

namespace ai.pkr.metabots.bots
{
    /// <summary>
    /// A bot that chooses the action randomly.
    /// 
    /// Creation parameters:
    /// 
    /// RngSeed: int.  If not specfied, no changes is made in the RNG.
    ///    Otherwise, the RNG is reinitialized:
    ///    if the seed is 0, a random seed is used, otherwise the specified seed is used.
    /// 
    /// Session parameters: same creation parameters.
    /// </summary>
    public class RandomBot : BotBase
    {

        public override void OnCreate(string name, Props creationParameters)
        {
            base.OnCreate(name, creationParameters);
        }

        public override void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters)
        {
            base.OnSessionBegin(sessionName, gameDef, sessionParameters);
            Parametrize(sessionParameters);
        }

        public override PokerAction OnActionRequired(string gameString)
        {
            base.OnActionRequired(gameString);
            List<Ak> possibleActions = CurGameState.GetAllowedActions(GameDefinition);
            int i = _rng.Next(possibleActions.Count);
            PokerAction action = new PokerAction(possibleActions[i], 0, 0, "");
            return action;
        }

        void Parametrize(Props parameters)
        {
            int seed = int.Parse(parameters.GetDefault("RngSeed", "0"));
            if (seed == 0)
                _rng = new Random();
            else
                _rng = new Random(seed);
        }

        private Random _rng = new Random();
    }
}
