/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.lib.algorithms;

namespace ai.pkr.metabots
{
    public enum SessionKind
    {
        /// <summary>
        /// Used to run preconfigured bots against each other.
        /// </summary>
        RingGame,
        /// <summary>
        /// Same as RingGame, bots change seats to even out influence of card distribution.
        /// </summary>
        RingGameWithSeatPermutations,
    }

    [Serializable]
    public class SessionCfg
    {
        public SessionCfg()
        {
            ReplayFrom = new PropString();
            GameDefinitionFile = new PropString();
        }

        public SessionCfg(string name, SessionKind kind, string gameDefinitionFile)
        {
            Name = name;
            Kind = kind;
            GameDefinitionFile = new PropString(gameDefinitionFile);
        }

        public string Name
        {
            set;
            get;
        }
        public SessionKind Kind
        {
            set;
            get;
        }


        public PropString GameDefinitionFile
        {
            set;get;
        }


        [XmlIgnore]
        public GameDefinition GameDefinition
        {
            set;
            get;
        }

        /// <summary>
        /// Name of a card log file or directory to replay.
        /// If specified, cards for the game are taken from the log, IGameRules.Deal() is not used.
        /// Number of games played is min(cardsInLogsCount, GamesCount).
        /// </summary>
        public PropString ReplayFrom
        {
            set;
            get;
        }

        /// <summary>
        /// Random number generator seed used to shuffle cards.
        /// 0 means use random time-based value obtained at the beginning of the session.
        /// </summary>
        public int RngSeed
        {
            set;
            get;
        }

        /// <summary>
        /// For sessions with repetitions - a step to increment RngSeed. Is useful to created deterministic
        /// sessions with multiple repetitions. 0 means use creating a random time-based seed after each repetition.
        /// </summary>
        public int RngSeedStep
        {
            set;
            get;
        }

        /// <summary>
        /// Number of games, meaning depends on the session kind. For instance, for seat-permutation
        /// sessions this is a number of games in a single permutation subsession.
        /// </summary>
        public int GamesCount
        {
            set;
            get;
        }

        /// <summary>
        /// Number of repetition of this session, default is 1. 
        /// Is intended mostly for fine-granularity of sessions to allow to interrupt
        /// session suite execution. E.g. 100 SP sessions of 1000 games each are equivalent 
        /// of 1000 SP session of 100 games each for bots with constant strategy, but the latter
        /// can be interrupted quicker.
        /// </summary>
        public int RepeatCount
        {
            set { _repeatCount = value;  }
            get { return _repeatCount; }
        }

        /// <summary>
        /// If set to true, the button does not move, all players' positions are constant.
        /// </summary>
        public bool FixButton
        {
            set;
            get;
        }

        /// <summary>
        /// Players of the game. 
        /// Table order of players will correspond (in general) to the order in this collection.
        /// This can be used in tests where order of players must be predefined.
        /// Exception can be games where the table order changes over time, e.g. tournaments.
        /// 
        /// For ring game sessions the first player will be the small blind in the 1st game 
        /// (index 0 in terms of GameDefinition class).
        /// </summary>
        public PlayerSessionCfg[] Players
        {
            set;get;
        }

        /// <summary>
        /// Returns an estimated games count for one repetition, depending on session type.
        /// Actual game count may differ, for instance if replaying from log.
        /// </summary>
        /// <returns></returns>
        public int GetEstimatedGamesCount()
        {
            switch(Kind)
            {
                case SessionKind.RingGameWithSeatPermutations:
                    return (int)EnumAlgos.Factorial(Players.Length) * GamesCount;
            }
            return GamesCount;
        }

        #region Serialization

        public void ConstructFromXml(ConstructFromXmlParams parameters)
        {
            if (GameDefinitionFile != null)
            {
                string gdFile = GameDefinitionFile.Get(parameters.Local);
                if (gdFile != "")
                {
                    GameDefinition = XmlSerializerExt.Deserialize<GameDefinition>(gdFile);
                }
            }
        }
        #endregion

        #region Data

        private int _repeatCount = 1;
        #endregion
    }
}
