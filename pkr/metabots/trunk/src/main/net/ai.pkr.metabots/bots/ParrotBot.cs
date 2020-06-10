/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ai.lib.utils;
using ai.pkr.metagame;
using System.IO;

namespace ai.pkr.metabots.bots
{
    /// <summary>
    /// A bot that replays actions from a game log.
    /// Given a name of a player to imitate, it will repeate all his actions in every game,
    /// even if the action is illegal.
    /// This is very useful to test a bot against an opponent with random behaviour.
    /// Once recorded, the behavior of the opponent can be replayed.
    /// Pay attention that the Parrot sits at the same position as his player.
    /// 
    /// Creation parameters:
    /// 
    ///  Player:     (required) player to imitate.
    ///  ReplayFrom: (required) path to game log(s)
    /// 
    /// Session parameters: same as creation parameters.
    /// 
    /// </summary>
    public class ParrotBot: BotBase
    {
        public override void OnCreate(string name, Props creationParameters)
        {
            base.OnCreate(name, creationParameters);
            _parameters = creationParameters;
        }

        public override void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters)
        {
            base.OnSessionBegin(sessionName, gameDef, sessionParameters);
            _parameters.UpdateFrom(sessionParameters);
            Parametrize();
        }

        public override PokerAction OnActionRequired(string gameString)
        {
            return _actions[_nextAction++];
        }

        void LoadLogs(string replayFrom)
        {
            _actions.Clear();
            _nextAction = 0;

            GameLogParser logParser = new GameLogParser();
            logParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logParser_OnGameRecord);
            logParser.OnError += new GameLogParser.OnErrorHandler(logParser_OnError);
            logParser.ParsePath(replayFrom);
        }


        void logParser_OnError(GameLogParser source, string error)
        {
            throw new ApplicationException(source.GetDefaultErrorText(error));
        }

        void logParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            int pos = -1;

            for(int p = 0; p < gameRecord.Players.Count; ++p)
            {
                if (gameRecord.Players[p].Name == _playerToImitate)
                {
                    pos = p;
                    break;
                }
            }

            foreach (PokerAction action in gameRecord.Actions)
            {
                if (action.Position == pos && action.IsPlayerAction())
                {
                    _actions.Add(action);
                }
            }
        }

        void Parametrize()
        {
            _playerToImitate = _parameters.Get("Player");
            string replayFrom = _parameters.Get("ReplayFrom");
            LoadLogs(replayFrom);
        }

        private Props _parameters;
        private string _playerToImitate;
        public int _nextAction = 0;
        public List<PokerAction> _actions = new List<PokerAction>();
    }
}
