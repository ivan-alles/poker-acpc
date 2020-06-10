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
    /// A helper class to implement built-in bots. 
    /// <remarks>Must not be used outside of this assembly, therefore it has an internal
    /// constructor.</remarks>
    /// </summary>
    public abstract class BotBase: IPlayer
    {
        /// <summary>
        /// Internal constructor, prevents usage outside this assembly.
        /// </summary>
        internal BotBase()
        {
        }

        protected string Name
        {
            set;
            get;
        }
        protected GameDefinition GameDefinition
        {
            set;
            get;
        }
        protected int Position
        {
            set;
            get;
        }

        protected GameState CurGameState
        {
            set;
            get;
        }

        #region IPlayer Members

        public virtual void OnCreate(string name, Props creationParameters)
        {
            Name = name;
        }

        public PlayerInfo OnServerConnect()
        {
            return new PlayerInfo(Name);
        }

        public void OnServerDisconnect(string reason)
        {
        }

        public virtual void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters)
        {
            GameDefinition = gameDef;
        }

        public void OnSessionEvent(Props parameters)
        {
        }

        public virtual void OnSessionEnd()
        {
        }

        public virtual void OnGameBegin(string gameString)
        {
            CurGameState = new GameState(gameString, GameDefinition);
            Position = CurGameState.FindPositionByName(Name);
        }

        public virtual void OnGameUpdate(string gameString)
        {
            CurGameState = new GameState(gameString, GameDefinition);
        }

        public virtual PokerAction OnActionRequired(string gameString)
        {
            CurGameState = new GameState(gameString, GameDefinition);
            return null;
        }

        public virtual void OnGameEnd(string gameString)
        {
            CurGameState = new GameState(gameString, GameDefinition);
        }

        #endregion

        public override string ToString()
        {
            return Name + " (" + base.ToString() + ")";
        }

    }
}
