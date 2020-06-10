/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metabots
{
    /// <summary>
    /// Executes a single game with given rules and players.
    /// No players are allowed to be added or removed during a game. 
    /// Player parameters such as stack must not be changed during a game from outside.
    /// </summary>
    internal class GameRunner
    {
        #region Internal interface

        /// <summary>
        /// Creates a new game. Quickly copies all game parameters to the own members.
        /// The game is started by Run().
        /// </summary>
        internal GameRunner(GameDefinition gameDefinition, List<PlayerData> gamePlayers, 
            GameRecord dealRecord)
        {
            _gameDef = gameDefinition;
            _dealRecord = dealRecord;
            foreach (PlayerData gamePlayer in gamePlayers)
            {
                _players.Add(gamePlayer);
            }
        }

        internal GameRecord Run()
        {
            _state = new GameState(_gameDef, _players.Count);
            _state.Id = _dealRecord.Id;

            if (_players.Count < _gameDef.MinPlayers || _players.Count > _gameDef.MaxPlayers)
                throw new ApplicationException("Number of players in out of range in game " + _gameDef.Name);

            _gameRecord = new GameRecord();
            _gameRecord.Id = _state.Id;

            for (int p = 0; p < _players.Count; ++p)
            {
                // Blind are already set, so the stacks in the _state can be negative.
                _state.Players[p].Name = _players[p].info.Name;
                _state.Players[p].Stack += _players[p].stack;

                // For GameRecord we use stack before the game start.
                _gameRecord.Players.Add(new GameRecord.Player(_players[p].info.Name,
                    _players[p].stack, _state.Players[p].InPot, 0));
            }

            for (int p = 0; p < _players.Count; ++p)
            {
                _players[p].gameRecord = _gameRecord.DeepCopy();
                _players[p].iplayer.OnGameBegin(_gameRecord.ToString());
            }

            PlayGame();

            CalculateGameResult();

            foreach (PlayerData player in _players)
            {
                player.iplayer.OnGameEnd(player.gameRecord.ToString());
            }

            return _gameRecord;
        }

        #endregion

        #region Implementation


        void PlayGame()
        {
            for (; !_state.IsGameOver; )
            {
                // Start new round.
                Debug.Assert(_state.IsDealerActing);
                DealToAll();
                Debug.Assert(!_state.IsDealerActing);
                // Play round
                for (; !_state.IsGameOver && !_state.IsDealerActing; )
                {
                    Debug.Assert(_state.Players[_state.CurrentActor].CanActInCurrentRound);
                    PlayerData actor = _players[_state.CurrentActor];

                    PokerAction playerAction = actor.iplayer.OnActionRequired(actor.gameRecord.ToString());
                    AdjustAction(ref playerAction);
                    UpdateAllByAction(playerAction);
                    UpdatePlayers();
                }
            }
        }

        void DealToAll()
        {
            for (; ; )
            {
                if (!_state.IsDealerActing)
                    break;
                if(_dealIndex >= _dealRecord.Actions.Count)
                {
                    throw new ApplicationException(String.Format(
                        "Deal action index {0} out of range, probably replaying session with different player actions",
                                                                 _dealIndex));
                }
                PokerAction dealAction = _dealRecord.Actions[_dealIndex++];
                if (_state.GetAllowedActions(_gameDef)[0] != dealAction.Kind)
                {
                    throw new ApplicationException(String.Format("Wrong deal action, expected {0}, was {1}",
                                                                 _state.GetAllowedActions(_gameDef)[0], dealAction.Kind));
                }
                UpdateAllByAction(dealAction);
            }
            UpdatePlayers();
        }

        void CalculateGameResult()
        {
            if (_state.IsShowdownRequired)
            {
                UInt32[] ranks = new UInt32[_state.Players.Length];
                int [][] hands = new int[_state.Players.Length][];
                for (int p = 0; p < _state.Players.Length; ++p)
                {
                    if (!_state.Players[p].IsFolded)
                    {
                        hands[p] = _gameDef.DeckDescr.GetIndexes(_state.Players[p].Hand);
                    }
                }
                _gameDef.GameRules.Showdown(_gameDef, hands, ranks);
                long ranksSum = 0;
                for(int p = 0; p < ranks.Length; ++p)
                {
                    if(ranks[p] < 0)
                    {
                        throw new ApplicationException(String.Format("Negative rank {0} for player # {1}", ranks[p], p));
                    }
                    ranksSum += ranks[p];
                }
                if(ranksSum == 0)
                {
                    throw new ApplicationException("At least one rank must be non-negative");
                }
                _state.UpdateByShowdown(ranks, _gameDef);
            }

            // Update the main game record and players' game records.
            // Make private cards of non-folders visible for other players.
            // Update stacks.
            for(int p = 0; p < _players.Count; ++p)
            {
                _gameRecord.Players[p].Result = _state.Players[p].Result;
                _gameRecord.IsGameOver = _state.IsGameOver;
                _players[p].gameRecord.IsGameOver = _state.IsGameOver;
                _players[p].stack = _state.Players[p].Stack;
                for(int p1 = 0; p1 < _players.Count; ++p1)
                {
                    _players[p].gameRecord.Players[p1].Result = _state.Players[p1].Result;
                }
                for (int a = 0; a < _players[p].gameRecord.Actions.Count; ++a)
                {
                    PokerAction action = _players[p].gameRecord.Actions[a];
                    if (action.Kind == Ak.d && 
                        (action.Position == -1 || !_state.Players[action.Position].IsFolded))
                    {
                        action.Cards = _gameRecord.Actions[a].Cards;
                    }
                }
            }
        }

        /// <summary>
        /// Generates a string like "? ?" for invisible private cards.
        /// </summary>
        string GetHiddenPrivateDeal()
        {
            StringBuilder result = new StringBuilder();
            for(int i = 0; i < _gameDef.PrivateCardsCount[_state.Round]; ++i)
            {
                if (i == 0)
                {
                    result.Append("?");
                }
                else
                {
                    result.Append(" ?");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Updates game state, main game record and players' game records.
        /// </summary>
        void UpdateAllByAction(PokerAction action)
        {
            _state.UpdateByAction(action, _gameDef);
            _gameRecord.Actions.Add(action);
            for(int p = 0; p < _players.Count; ++p)
            {
                // Todo: public cards of other players will be hidden with '?'. This is 
                // of low importance now because they are used only in Stud.
                if(action.Kind == Ak.d && action.Position != -1 && action.Position != p)
                {
                    PokerAction hidden = action.DeepCopy();
                    hidden.Cards = GetHiddenPrivateDeal();
                    _players[p].gameRecord.Actions.Add(hidden);
                }
                else
                {
                    _players[p].gameRecord.Actions.Add(action);
                }
            }
        }

        void AdjustAction(ref PokerAction action)
        {
            if (action == null || !action.IsPlayerAction())
            {
                log.WarnFormat("Player returned null action, Action replaced by fold.");
                action = new PokerAction{Kind = Ak.f};
            }
            if (!action.IsPlayerAction())
            {
                log.WarnFormat("Player returned a non-player action: {0}. Action replaced by fold.", action);
                action = new PokerAction { Kind = Ak.f };
            }

            action.Position = _state.CurrentActor;

            if (action.Kind == Ak.r)
            {
                if (_state.BetCount >= _gameDef.BetsCountLimits[_state.Round])
                {
                    // No more raises allowed.
                    action.Kind = Ak.c;
                }
            }

            if (action.Kind != Ak.r)
                action.Amount = 0;
            else
            {
                double minBet = _state.GetMinBet(_gameDef);
                double maxBet = _state.GetMaxBet(_gameDef);
                if (action.Amount < minBet)
                    action.Amount = minBet;
                if (action.Amount > maxBet)
                    action.Amount = maxBet;
            }
        }

        void UpdatePlayers()
        {
            foreach (PlayerData player in _players)
            {
                player.iplayer.OnGameUpdate(player.gameRecord.ToString());
            }
        }

        /// <summary>
        /// List of players of this game, in order of player index 
        /// (same order as in GameState.Players).
        /// </summary>
        List<PlayerData> _players = new List<PlayerData>();
        private GameState _state;
        private GameRecord _gameRecord;
        public GameDefinition _gameDef;
        private GameRecord _dealRecord;
        private int _dealIndex = 0;

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
    } 
}
