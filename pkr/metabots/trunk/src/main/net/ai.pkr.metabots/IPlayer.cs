/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using ai.pkr.metagame;

namespace ai.pkr.metabots
{
    /// <summary>
    /// A player (bot). Can connect to a room and participate in games.
    /// Games are united in session, during a session the definition of the game remains constant.
    /// All the session, game and other events are passed to the bot in On* methods.
    /// <remarks>
    /// A bot can connect only to one room at a time and participate only in one
    /// game at a time. Multi-room multi-game bots are possible only when they behave 
    /// like multiple IPlayer instances from the server perspective.
    /// </remarks>
    /// </summary>
    /// <seealso cref="SessionSuiteRunner"/>
    /// <seealso cref="SessionSuiteCfg"/>
    public interface IPlayer
    {
        #region Local bots events


        /// <summary>
        /// Is called when the a local bot is created. 
        /// The player must accept the name given and return in in OnServerConnect() call.
        /// 
        /// In addition to user-defined parameters, the server will add the following:
        /// 
        /// pkr.SessionSuiteXmlFileName: the name of the session suite XML file (if available).
        ///     It can be used to resolve relative file references.
        /// 
        /// <remarks>Is not used for remote players.</remarks>
        /// </summary>
        void OnCreate(string name, Props creationParameters);

        #endregion

        #region Server events
        PlayerInfo OnServerConnect();
        void OnServerDisconnect(string reason);
        #endregion

        #region Session events
        /// <summary>
        /// Is called at the beginning of a session.
        /// </summary>
        void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters);
        /// <summary>
        /// Notifies about session events, depending on session type and situation.
        /// <para>
        /// Predefined parameters are:</para>
        /// <para>
        /// pkr.ForgetAll: tells the bot to forget all games histories, player models, etc. Is used, for instance,
        ///                in sessions with seat permutations.</para>
        /// </summary>
        void OnSessionEvent(Props parameters);
        /// <summary>
        /// Is called at the end beginning of a session.
        /// </summary>
        void OnSessionEnd();
        #endregion

        #region Game events
        /// <summary>
        /// As the game is started, all players are notified. Initial game string is passed.
        /// </summary>
        void OnGameBegin(string gameString);

        /// <summary>
        /// Game update is send to each player if GameState was changed, 
        /// e.g. a player posted blind or cards are dealt. 
        /// Allows bots to use idle time to analyze situation.
        /// There is no guarantee that all the game updates will be delivered 
        /// to each player as single events, e.g. posting of small and big blind 
        /// can be combined in one event.
        /// In the worst case, a player receives an update 
        /// when it is his turn to act (OnActionRequired).
        /// </summary>
        void OnGameUpdate(string gameString);

        /// <summary>
        /// Is called when it is turn to act for this player. 
        /// Current game string is passed as parameter. 
        /// Player returns its action. Allowed action kinds are r, c or f.
        /// Position of the action is ignored. 
        /// If amount is wrong or unspecified, action is corrected by the server:
        /// - if amount is less than the minimal allowed, minimal allowed amount is used.
        /// - if amount is more than the maximal allowed, maximal allowed amount is used.
        /// </summary>
        PokerAction OnActionRequired(string gameString);

        /// <summary>
        /// Last game string containing game results is reported.
        /// Delivered to all players of the game, even if folded.
        /// </summary>
        void OnGameEnd(string gameString);
        #endregion
    }
}
