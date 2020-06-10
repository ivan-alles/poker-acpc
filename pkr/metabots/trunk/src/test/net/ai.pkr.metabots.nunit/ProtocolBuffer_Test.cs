/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using ai.pkr.metagame;
using ProtoBuf;
using ai.pkr.metabots.remote;
using GameDefinition = ai.pkr.metagame.GameDefinition;
using Ak = ai.pkr.metagame.Ak;
using NUnit.Framework;
using System.Reflection;

namespace ai.pkr.metabots.nunit
{
    /// <summary>
    /// Unit tests for all classes that are protocol buffer serializable. 
    /// </summary>
    [TestFixture]
    public class ProtocolBuffer_Test
    {
        #region Tests

        [Test]
        public void Test_PokerAction()
        {
            metagame.PokerAction o1 = metagame.PokerAction.c(0, -15.3);
            metagame.PokerAction o2 = null;
            remote.PokerAction rm1 = new remote.PokerAction();
            rm1.ToRemote(o1);
            using (MemoryStream s = new MemoryStream())
            {
                Serializer.Serialize(s, rm1);

                s.Seek(0, SeekOrigin.Begin);
                remote.PokerAction rm2 = Serializer.Deserialize<remote.PokerAction>(s);
                o2 = rm2.FromRemote();
            }
            Assert.IsNotNull(o2);
            Assert.AreEqual(o1, o2);
        }


        /// <summary>
        /// Tests both GameDefinition and DeckDescriptor.
        /// </summary>
        [Test]
        public void Test_GameDefinition()
        {
            string[] cardNames1 = new string[] {"Q", "K", "A"};
            CardSet[] cardSets1 = new CardSet[]
                                      {
                                          new CardSet {bits = 0x01},
                                          new CardSet {bits = 0x02},
                                          new CardSet {bits = 0x10}
                                      };

            GameDefinition o1 = new GameDefinition();

            o1.Name = "TestGameDef";
            o1.RoundsCount = 4;
            o1.MinPlayers = 2;
            o1.MaxPlayers = 10;
            o1.BetsCountLimits = new int[] { 1, 2, 3, 4 };
            o1.BetStructure = new double[] { 1, 6.2, 7.3, 8.4 };
            o1.BlindStructure = new double[] { 0.5, 2.2, 3.3, 4.4 };
            o1.PrivateCardsCount = new int[]{10, 11, 12, 13};
            o1.PublicCardsCount = new int[]{21, 22, 23, 24};
            o1.SharedCardsCount = new int[]{0,1,2,3};
            o1.FirstActor = new int[] {0, 1, 2, 3};
            o1.FirstActorHeadsUp = new int[] {0, 1, 1, 0};
            o1.LimitKind = metagame.LimitKind.NoLimit;
            o1.DeckDescr = new metagame.DeckDescriptor("TestDeck", cardNames1, cardSets1);

            GameDefinition o2 = null;

            remote.GameDefinition rm1 = new remote.GameDefinition();
            rm1.ToRemote(o1);
            using (MemoryStream s = new MemoryStream())
            {
                Serializer.Serialize(s, rm1);
                s.Seek(0, SeekOrigin.Begin);
                remote.GameDefinition rm2 = Serializer.Deserialize<remote.GameDefinition>(s);
                o2 = rm2.FromRemote();
            }

            Assert.IsNotNull(o2);

            Assert.AreEqual(o1.Name, o2.Name);
            Assert.AreEqual(o1.RoundsCount, o2.RoundsCount);
            Assert.AreEqual(o1.MinPlayers, o2.MinPlayers);
            Assert.AreEqual(o1.MaxPlayers, o2.MaxPlayers);
            Assert.AreEqual(o1.BetsCountLimits, o2.BetsCountLimits);
            Assert.AreEqual(o1.BetStructure, o2.BetStructure);
            Assert.AreEqual(o1.BlindStructure, o2.BlindStructure);
            Assert.AreEqual(o1.PrivateCardsCount, o2.PrivateCardsCount);
            Assert.AreEqual(o1.PublicCardsCount, o2.PublicCardsCount);
            Assert.AreEqual(o1.SharedCardsCount, o2.SharedCardsCount);
            Assert.AreEqual(o1.FirstActor, o2.FirstActor);
            Assert.AreEqual(o1.FirstActorHeadsUp, o2.FirstActorHeadsUp);
            Assert.AreEqual(o1.LimitKind, o2.LimitKind);
            Assert.AreEqual(o1.DeckDescr, o2.DeckDescr);
        }


        [Test]
        public void Test_PlayerInfo()
        {
            PlayerInfo o1 = new PlayerInfo("TestPlayer");
            PlayerInfo o2 = null;

            remote.PlayerInfo rm1 = new remote.PlayerInfo();
            rm1.ToRemote(o1);
            using (MemoryStream s = new MemoryStream())
            {
                Serializer.Serialize(s, rm1);
                s.Seek(0, SeekOrigin.Begin);
                remote.PlayerInfo rm2 = Serializer.Deserialize<remote.PlayerInfo>(s);
                o2 = rm2.FromRemote();
            }
            Assert.IsNotNull(o2);
            Assert.AreEqual(o1.Name, o2.Name);
        }

        #endregion
    }
}
