/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metabots;
using ai.pkr.metagame;

namespace ai.pkr.acpc.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class Acpc11ServerMessageConverter_Test
    {
        #region Tests

        [Test]
        public void Test_Basics()
        {
            Acpc11ServerMessageConverter c = new Acpc11ServerMessageConverter();
            Assert.AreEqual("\r\n", c.LineTerminator);

            // Other that in protocol.pdf, but this is so in the sources.
            Assert.AreEqual("VERSION:2.0.0", c.HandshakeMessage);
        }

        [Test]
        public void Test_OnServerMessage()
        {
            Player player = new Player();

            Acpc11ServerMessageConverter c = new Acpc11ServerMessageConverter();

            c.Player = player;
            c.PlayerName = "Pl";

            // Test strings are taken from protocol.pdf - description of ACPC protocol.

            // Game 0 in position 1

            player.OnGameBeginCount = 0;

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:0::TdAs|"));
            Assert.AreEqual(1, player.OnGameBeginCount);
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0};;", player.OnGameBeginGameString);

            player.Response = Ak.r;
            Assert.AreEqual("MATCHSTATE:0:0:r:TdAs|:r", c.OnServerMessage("MATCHSTATE:0:0:r:TdAs|"));
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{Td As} 0r1;", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:0:rr:TdAs|"));

            player.Response = Ak.r;
            Assert.AreEqual("MATCHSTATE:0:0:rrc/:TdAs|/2c8c3h:r", c.OnServerMessage("MATCHSTATE:0:0:rrc/:TdAs|/2c8c3h"));
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{Td As} 0r1 1r1 0c d{2c 8c 3h};", player.GameString);


            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:0:rrc/r:TdAs|/2c8c3h"));

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:0:rrc/rc/:TdAs|/2c8c3h/9c:c", c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/:TdAs|/2c8c3h/9c"));
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{Td As} 0r1 1r1 0c d{2c 8c 3h} 1r1 0c d{9c};", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/c:TdAs|/2c8c3h/9c"));

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:0:rrc/rc/cr:TdAs|/2c8c3h/9c:c", c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/cr:TdAs|/2c8c3h/9c"));
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{Td As} 0r1 1r1 0c d{2c 8c 3h} 1r1 0c d{9c} 1c 0r2;", player.GameString);

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:0:rrc/rc/crc/:TdAs|/2c8c3h/9c/Kh:c", c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/crc/:TdAs|/2c8c3h/9c/Kh"));
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{Td As} 0r1 1r1 0c d{2c 8c 3h} 1r1 0c d{9c} 1c 0r2 1c d{Kh};", player.GameString);


            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/crc/c:TdAs|/2c8c3h/9c/Kh"));

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:0:rrc/rc/crc/cr:TdAs|/2c8c3h/9c/Kh:c", c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/crc/cr:TdAs|/2c8c3h/9c/Kh"));
            Assert.AreEqual("0; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{Td As} 0r1 1r1 0c d{2c 8c 3h} 1r1 0c d{9c} 1c 0r2 1c d{Kh} 1c 0r2;", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:0:rrc/rc/crc/crc:TdAs|8hTc/2c8c3h/9c/Kh"));

            Assert.AreEqual(1, player.OnGameBeginCount);
            Assert.AreEqual(1, c.GameCount);

            // Game 1 in position 0.
            player.OnGameBeginCount = 0;

            player.Response = Ak.r;
            Assert.AreEqual("MATCHSTATE:1:1::|Qd7c:r", c.OnServerMessage("MATCHSTATE:1:1::|Qd7c"));
            Assert.AreEqual("1; Pl{0 0.5 0} ?Opp{0 1 0}; 0d{Qd 7c} 1d{? ?};", player.GameString);
            Assert.AreEqual(1, player.OnGameBeginCount);
            Assert.AreEqual("1; Pl{0 0.5 0} ?Opp{0 1 0};;", player.OnGameBeginGameString);


            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:1:1:r:|Qd7c"));


            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:1:1:rr:|Qd7c:c", c.OnServerMessage("MATCHSTATE:1:1:rr:|Qd7c"));
            Assert.AreEqual("1; Pl{0 0.5 0} ?Opp{0 1 0}; 0d{Qd 7c} 1d{? ?} 0r1 1r1;", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:1:1:rrc/:|Qd7c/2h8h5c"));

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:1:1:rrc/r:|Qd7c/2h8h5c:c", c.OnServerMessage("MATCHSTATE:1:1:rrc/r:|Qd7c/2h8h5c"));
            Assert.AreEqual("1; Pl{0 0.5 0} ?Opp{0 1 0}; 0d{Qd 7c} 1d{? ?} 0r1 1r1 0c d{2h 8h 5c} 1r1;", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:1:1:rrc/rc/:|Qd7c/2h8h5c/Th"));


            player.Response = Ak.f;
            Assert.AreEqual("MATCHSTATE:1:1:rrc/rc/r:|Qd7c/2h8h5c/Th:f", c.OnServerMessage("MATCHSTATE:1:1:rrc/rc/r:|Qd7c/2h8h5c/Th"));
            Assert.AreEqual("1; Pl{0 0.5 0} ?Opp{0 1 0}; 0d{Qd 7c} 1d{? ?} 0r1 1r1 0c d{2h 8h 5c} 1r1 0c d{Th} 1r2;", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:1:1:rrc/rc/rf:|Qd7c/2h8h5c/Th"));

            Assert.AreEqual(1, player.OnGameBeginCount);
            Assert.AreEqual(2, c.GameCount);

            // Game 2 in position 1
            player.OnGameBeginCount = 0;

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:2::9d7s|"));
            Assert.AreEqual(1, player.OnGameBeginCount);
            Assert.AreEqual("2; ?Opp{0 0.5 0} Pl{0 1 0};;", player.OnGameBeginGameString);

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:2:r:9d7s|:c", c.OnServerMessage("MATCHSTATE:0:2:r:9d7s|"));
            Assert.AreEqual("2; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{9d 7s} 0r1;", player.GameString);


            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:2:rc/:9d7s|/5d2cJc:c", c.OnServerMessage("MATCHSTATE:0:2:rc/:9d7s|/5d2cJc"));
            Assert.AreEqual("2; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{9d 7s} 0r1 1c d{5d 2c Jc};", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:2:rc/c:9d7s|/5d2cJc"));

            player.Response = Ak.c;
            Assert.AreEqual("MATCHSTATE:0:2:rc/cc/:9d7s|/5d2cJc/3d:c", c.OnServerMessage("MATCHSTATE:0:2:rc/cc/:9d7s|/5d2cJc/3d"));
            Assert.AreEqual("2; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{9d 7s} 0r1 1c d{5d 2c Jc} 1c 0c d{3d};", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:2:rc/cc/c:9d7s|/5d2cJc/3d"));

            player.Response = Ak.f;
            Assert.AreEqual("MATCHSTATE:0:2:rc/cc/cr:9d7s|/5d2cJc/3d:f", c.OnServerMessage("MATCHSTATE:0:2:rc/cc/cr:9d7s|/5d2cJc/3d"));
            Assert.AreEqual("2; ?Opp{0 0.5 0} Pl{0 1 0}; 0d{? ?} 1d{9d 7s} 0r1 1c d{5d 2c Jc} 1c 0c d{3d} 1c 0r2;", player.GameString);

            Assert.AreEqual(null, c.OnServerMessage("MATCHSTATE:0:2:rc/cc/crf:9d7s|/5d2cJc/3d"));

            Assert.AreEqual(1, player.OnGameBeginCount);
            Assert.AreEqual(3, c.GameCount);

        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class Player: IPlayer
        {

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

            public int OnGameBeginCount
            {
                set;
                get;
            }

            public string OnGameBeginGameString
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
                OnGameBeginCount++;
                OnGameBeginGameString = gameString;
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
