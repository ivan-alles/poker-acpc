/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using ai.pkr.metagame;

namespace ai.pkr.metabots.nunit
{
    /// <summary>
    /// Implements IPlayer for unit-tests. 
    /// Contains method calls counters and other helpful data members.
    /// </summary>
    public class PlayerHelper: IPlayer
    {

        #region IPlayer Members

        virtual public void OnCreate(string name, Props creationParameters)
        {
            Name = name;
            CreationParameters = creationParameters;
            OnCreateCount++;
        }

        virtual public PlayerInfo OnServerConnect()
        {
            OnServerConnectCount++;
            return new PlayerInfo(Name);
        }

        virtual public void OnServerDisconnect(string reason)
        {
            OnServerDisconnectCount++;
        }


        virtual public void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters)
        {
            GameDefinition = gameDef;
            SessionName = sessionName;
            SessionParameters = sessionParameters;
            OnSessionBeginCount++;
        }

        public void OnSessionEvent(Props parameters)
        {
            OnSessionEventCount++;
        }

        virtual public void OnSessionEnd()
        {
            OnSessionEndCount++;
        }

        virtual public void OnGameBegin(string gameString)
        {
            OnGameBeginCount++;
            CurGameState = new GameState(gameString, GameDefinition);
            Position = CurGameState.FindPositionByName(Name);
        }

        virtual public void OnGameUpdate(string gameString)
        {
            OnGameUpdateCount++;
            CurGameState = new GameState(gameString, GameDefinition);
        }

        virtual public PokerAction OnActionRequired(string gameString)
        {
            OnActionRequiredCount++;
            CurGameState = new GameState(gameString, GameDefinition);
            return PokerAction.c(0);
        }

        virtual public void OnGameEnd(string gameString)
        {
            CurGameState = new GameState(gameString, GameDefinition);
            OnGameEndCount++;
        }

        #endregion

        #region Test data

        public List<string> PrivateCards = new List<string>();
        public int Position;
        public string Name;
        public string RoomName;
        public Props CreationParameters;
        public string SessionName;
        public Props SessionParameters;
        public GameState CurGameState;
        public GameDefinition GameDefinition;

        #endregion   

        #region Call counters

        public int OnCreateCount;
        public int OnServerConnectCount;
        public int OnServerDisconnectCount;
        public int OnSessionBeginCount;
        public int OnSessionEventCount;
        public int OnSessionEndCount;
        public int OnGameBeginCount;
        public int OnGameUpdateCount;
        public int OnActionRequiredCount;
        public int OnGameEndCount;
        
        #endregion
    }
}
