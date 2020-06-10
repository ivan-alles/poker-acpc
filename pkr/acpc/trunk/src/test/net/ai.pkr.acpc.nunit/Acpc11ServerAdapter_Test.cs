/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.metabots;

namespace ai.pkr.acpc.nunit
{
    /// <summary>
    /// Unit tests for Acpc11ServerAdapter. 
    /// </summary>
    [TestFixture]
    public class Acpc11ServerAdapter_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public void Test_Interactive()
        {
            Player player = new Player();

            Acpc11ServerMessageConverter c = new Acpc11ServerMessageConverter();

            c.Player = player;
            c.PlayerName = "Pl";

            AcpcServerAdapter adapter = new AcpcServerAdapter();
            adapter.MessageConverter = c;

            adapter.IsVerbose = true;

            adapter.Connect("192.168.178.21", 18791, 100);

            adapter.Run();
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class Player : IPlayer
        {

            public Player()
            {
                Response = Ak.c;
            }

            public Ak Response
            {
                set;
                get;
            }

            public string GameString
            {
                set;
                get;
            }

            #region IPlayer Members

            public PokerAction OnActionRequired(string gameString)
            {
                GameString = gameString;
                return new PokerAction(Response, 0, 0, "");
            }

            public void OnCreate(string name, ai.lib.utils.Props creationParameters)
            {
            }

            public void OnGameBegin(string gameString)
            {
            }

            public void OnGameEnd(string gameString)
            {
            }

            public void OnGameUpdate(string gameString)
            {
            }

            public PlayerInfo OnServerConnect()
            {
                return null;
            }

            public void OnServerDisconnect(string reason)
            {
            }

            public void OnSessionBegin(string sessionName, ai.pkr.metagame.GameDefinition gameDef, ai.lib.utils.Props sessionParameters)
            {
            }

            public void OnSessionEnd()
            {
            }

            public void OnSessionEvent(ai.lib.utils.Props parameters)
            {
            }

            #endregion
        }

        #endregion
    }
}
