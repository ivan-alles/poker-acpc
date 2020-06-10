/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using NUnit.Framework;
using ai.lib.utils;

namespace ai.pkr.metabots.nunit
{
    /// <summary>
    /// Test for SocketServerPlayer. JavaClient must be started after start of this test.
    /// </summary>
    [TestFixture]
    public class SocketPlayer_Test
    {
        #region Tests
        [Test]
        [Explicit("Requires client (ai.pkr.jmetabots.SocketClientTestWithServer) to be started after this test.")]
        public void Test_RemoteClient()
        {
            SocketServer socketServer = new SocketServer("127.0.0.1", 9001);
            Console.WriteLine("Waiting for client to connect");
            SocketServerPlayer socketPlayer = socketServer.Listen(60000);
            Assert.IsNotNull(socketPlayer);
            Console.WriteLine("Socket client connected");

            PlayerInfo pi = socketPlayer.OnServerConnect();
            Console.WriteLine("Player connected");
            Assert.AreEqual("SocketClientTest", pi.Name);

            GameDefinition gameDef = new GameDefinition();
            gameDef.Name = "Test game";

            string[] cardNames1 = new string[] { "Q", "K", "A" };
            CardSet[] cardSets1 = new CardSet[]
                                      {
                                          new CardSet {bits = 0x01},
                                          new CardSet {bits = 0x02},
                                          new CardSet {bits = 0x10}
                                      };

            gameDef.DeckDescr = new metagame.DeckDescriptor("TestDeck", cardNames1, cardSets1);


            socketPlayer.OnSessionBegin("Test session", gameDef,  new Dictionary<string, string> {
                {"p1", "v1"},
                {"p2", "v2"}});

            socketPlayer.OnGameBegin("gamestring OnGameBegin");

            socketPlayer.OnGameUpdate("gamestring OnGameUpdate");

            PokerAction action = socketPlayer.OnActionRequired("gamestring OnActionRequired");
            Assert.AreEqual(Ak.r, action.Kind);
            Assert.AreEqual(100, action.Amount);
            Console.WriteLine("Action received");

            socketPlayer.OnGameEnd("gamestring OnGameEnd");

            socketPlayer.OnSessionEnd();

            socketPlayer.OnServerDisconnect("End of test");
            socketPlayer.Disconnect();
        }
        #endregion
    }
}
