/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using NUnit.Framework;
using System.Reflection;
using System.Text.RegularExpressions;
using ai.lib.utils;

namespace ai.pkr.metagame.nunit
{
    /// <summary>
    /// Unit tests for GameDefinition. 
    /// </summary>
    [TestFixture]
    public class GameDefinition_Test
    {
        #region Tests

        [Test]
        public void Test_XmlSerialization()
        {
            GameDefinition gd1 = new GameDefinition();

            gd1.MinPlayers = 2;
            gd1.MaxPlayers = 2;
            gd1.BetsCountLimits = new int[] {4,4,4,4};
            gd1.BetStructure = new double[] {1,1,2,2};

            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                gd1.XmlSerialize(tw);
            }

            Console.WriteLine(sb.ToString());

            GameDefinition gd2;

            using (TextReader textReader = new StringReader(sb.ToString()))
            {
                XmlSerializerExt.Deserialize(out gd2, textReader);
            }

            Assert.IsNotNull(gd2);
        }


        [Test]
        public void Test_DeserializeHandWrittenWithSchema()
        {
            string subdirName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            string testResourcesPath = Path.Combine(Props.Global.Get("bds.TestDir"), subdirName);

            GameDefinition gd;

            XmlSerializerExt.Deserialize(out gd, Path.Combine(testResourcesPath, "gamedef-test.xml"));
            
            Assert.IsNotNull(gd);
            Assert.AreEqual("HE.FL.Max2", gd.Name);
            Assert.AreEqual(new int[]{4,4,4,4}, gd.BetsCountLimits);
            Assert.AreEqual(new double[] { 1, 1, 2, 2 }, gd.BetStructure);
            Assert.AreEqual(new double[] { .5, 1 }, gd.BlindStructure);
            Assert.AreEqual(new int[] { 2, 0, 0, 0 }, gd.PrivateCardsCount);
            Assert.AreEqual(new int[] { 0, 0, 0, 0 }, gd.PublicCardsCount);
            Assert.AreEqual(new int[] { 0, 3, 1, 1 }, gd.SharedCardsCount);
            Assert.AreEqual(new int[] { 2, 0, 0, 0 }, gd.FirstActor);
            Assert.AreEqual(new int[] { 0, 1, 1, 1 }, gd.FirstActorHeadsUp);
            Assert.AreEqual(LimitKind.FixedLimit, gd.LimitKind);
            Assert.AreEqual(2, gd.MinPlayers);
            Assert.AreEqual(2, gd.MaxPlayers);
            Assert.AreEqual(4, gd.RoundsCount);
            Assert.AreEqual(1, gd.GameRulesCreationParams.Count);
            Assert.AreEqual("Test configuration" , gd.GameRulesCreationParams.Get("p1"));

            Assert.AreEqual("TestDeckDescriptor", gd.DeckDescr.Name); 

            Assert.IsNotNull(gd.GameRules);
            Assert.IsTrue(gd.GameRules is GameRulesHelper);
            GameRulesHelper testGameRules = (GameRulesHelper)gd.GameRules;
            Assert.AreEqual(1, testGameRules.Config.Count);
            Assert.AreEqual("Test configuration", testGameRules.Config.Get("p1"));
        }

        [Test]
        public void Test_CopyConstructor()
        {
            string testResourcesPath = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            GameDefinition gd1;
            XmlSerializerExt.Deserialize(out gd1, Path.Combine(testResourcesPath, "gamedef-test.xml"));

            GameDefinition gd2 = new GameDefinition(gd1);

            Assert.AreEqual(gd1.Name, gd2.Name);
            Assert.AreEqual(gd1.RoundsCount, gd2.RoundsCount);
            Assert.AreEqual(gd1.MinPlayers, gd2.MinPlayers);
            Assert.AreEqual(gd1.MaxPlayers, gd2.MaxPlayers);
            Assert.AreEqual(gd1.BetStructure, gd2.BetStructure);
            Assert.AreEqual(gd1.BlindStructure, gd2.BlindStructure);
            Assert.AreEqual(gd1.PrivateCardsCount, gd2.PrivateCardsCount);
            Assert.AreEqual(gd1.PublicCardsCount, gd2.PublicCardsCount);
            Assert.AreEqual(gd1.SharedCardsCount, gd2.SharedCardsCount);
            Assert.AreEqual(gd1.BetsCountLimits, gd2.BetsCountLimits);
            Assert.AreEqual(gd1.FirstActor, gd2.FirstActor);
            Assert.AreEqual(gd1.FirstActorHeadsUp, gd2.FirstActorHeadsUp);
            Assert.AreEqual(gd1.LimitKind, gd2.LimitKind);
            Assert.AreEqual(gd1.DeckDescrFile, gd2.DeckDescrFile);
            Assert.AreEqual(gd1.DeckDescr, gd2.DeckDescr);
            Assert.AreEqual(gd1.GameRulesAssemblyFile, gd2.GameRulesAssemblyFile);
            Assert.AreEqual(gd1.GameRulesType, gd2.GameRulesType);
            Assert.AreEqual(gd1.GameRulesCreationParams, gd2.GameRulesCreationParams);
            Assert.AreEqual(gd1.GameRules, gd2.GameRules);

        }

        [Test]
        public void Test_GetBlindsBetsCount()
        {
            GameDefinition gd1 = new GameDefinition();

            // Emppy blinds
            gd1.BlindStructure = new double[0];
            Assert.AreEqual(0, gd1.GetBlindsBetsCount());

            // Non-zero unequal blinds
            gd1.BlindStructure = new double[]{1, 2};
            Assert.AreEqual(1, gd1.GetBlindsBetsCount());

            // Non-zero equal blinds (ante)
            gd1.BlindStructure = new double[]{1, 1};
            Assert.AreEqual(1, gd1.GetBlindsBetsCount());

            // Zero blinds
            gd1.BlindStructure = new double[]{0, 0};
            Assert.AreEqual(0, gd1.GetBlindsBetsCount());
        }

        [Test]
        public void Test_GetHandSizes()
        {
            string testResourcesPath = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            GameDefinition gd =
                XmlSerializerExt.Deserialize<GameDefinition>(Path.Combine(testResourcesPath, "gamedef-test.xml"));
            int[] handSizes = gd.GetHandSizes();
            Assert.AreEqual(new int [] {2, 5, 6, 7}, handSizes);
        }

        #endregion
    }
}