/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for WalkChanceAndActionTrees.
    /// As it is difficult to verify, do a regression test only.
    /// </summary>
    [TestFixture]
    public class WalkChanceAndActionTrees_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition  gd= XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));
            ActionTree at = CreateActionTreeByGameDef.Create(gd);
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ct, 0);

            TestWalker tw = new TestWalker { ActionTree = at, PlayerChanceTree = pct };
            tw.Walk();
            Console.WriteLine("Begin: a:{0} c:{1} total:{2}", tw.BeginCount[0], tw.BeginCount[1], tw.BeginCount[0] + tw.BeginCount[1]);
            Assert.AreEqual(26, tw.BeginCount[0]);
            Assert.AreEqual(3, tw.BeginCount[1]);
            Assert.AreEqual(_kuhn, tw.Text);
        }

        [Test]
        public void Test_Leduc()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));
            ActionTree at = CreateActionTreeByGameDef.Create(gd);
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ct, 0);

            TestWalker tw = new TestWalker { ActionTree = at, PlayerChanceTree = pct };
            tw.Walk();
            Console.WriteLine("Begin: a:{0} c:{1} total:{2}", tw.BeginCount[0], tw.BeginCount[1], tw.BeginCount[0] + tw.BeginCount[1]);
            Assert.AreEqual(674, tw.BeginCount[0]);
            Assert.AreEqual(48, tw.BeginCount[1]);
            Assert.AreEqual(_leduc, tw.Text);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Kuhn verification

        string _kuhn = @"B tree:Action node:1 depth:0
B tree:Action node:2 depth:1
B tree:Chance node:1 depth:2
B tree:Action node:3 depth:3
B tree:Action node:4 depth:4
B tree:Action node:5 depth:4
B tree:Action node:6 depth:5
B tree:Action node:7 depth:5
B tree:Action node:8 depth:3
B tree:Action node:9 depth:4
B tree:Action node:10 depth:4
B tree:Chance node:2 depth:2
B tree:Action node:3 depth:3
B tree:Action node:4 depth:4
B tree:Action node:5 depth:4
B tree:Action node:6 depth:5
B tree:Action node:7 depth:5
B tree:Action node:8 depth:3
B tree:Action node:9 depth:4
B tree:Action node:10 depth:4
B tree:Chance node:3 depth:2
B tree:Action node:3 depth:3
B tree:Action node:4 depth:4
B tree:Action node:5 depth:4
B tree:Action node:6 depth:5
B tree:Action node:7 depth:5
B tree:Action node:8 depth:3
B tree:Action node:9 depth:4
B tree:Action node:10 depth:4
";
        #endregion 

        #region Leduc verification

        string _leduc = @"B tree:Action node:1 depth:0
B tree:Action node:2 depth:1
B tree:Chance node:1 depth:2
B tree:Action node:3 depth:3
B tree:Action node:4 depth:4
B tree:Chance node:2 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Chance node:3 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Chance node:4 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Action node:19 depth:4
B tree:Action node:20 depth:5
B tree:Action node:21 depth:5
B tree:Chance node:2 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Chance node:3 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Chance node:4 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Action node:36 depth:5
B tree:Action node:37 depth:6
B tree:Action node:38 depth:6
B tree:Chance node:2 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Chance node:3 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Chance node:4 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Action node:53 depth:3
B tree:Action node:54 depth:4
B tree:Action node:55 depth:4
B tree:Chance node:2 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Chance node:3 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Chance node:4 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Action node:70 depth:4
B tree:Action node:71 depth:5
B tree:Action node:72 depth:5
B tree:Chance node:2 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:3 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:4 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:5 depth:2
B tree:Action node:3 depth:3
B tree:Action node:4 depth:4
B tree:Chance node:6 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Chance node:7 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Chance node:8 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Action node:19 depth:4
B tree:Action node:20 depth:5
B tree:Action node:21 depth:5
B tree:Chance node:6 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Chance node:7 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Chance node:8 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Action node:36 depth:5
B tree:Action node:37 depth:6
B tree:Action node:38 depth:6
B tree:Chance node:6 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Chance node:7 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Chance node:8 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Action node:53 depth:3
B tree:Action node:54 depth:4
B tree:Action node:55 depth:4
B tree:Chance node:6 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Chance node:7 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Chance node:8 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Action node:70 depth:4
B tree:Action node:71 depth:5
B tree:Action node:72 depth:5
B tree:Chance node:6 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:7 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:8 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:9 depth:2
B tree:Action node:3 depth:3
B tree:Action node:4 depth:4
B tree:Chance node:10 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Chance node:11 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Chance node:12 depth:5
B tree:Action node:5 depth:6
B tree:Action node:6 depth:7
B tree:Action node:7 depth:7
B tree:Action node:8 depth:8
B tree:Action node:9 depth:8
B tree:Action node:10 depth:8
B tree:Action node:11 depth:9
B tree:Action node:12 depth:9
B tree:Action node:13 depth:6
B tree:Action node:14 depth:7
B tree:Action node:15 depth:7
B tree:Action node:16 depth:7
B tree:Action node:17 depth:8
B tree:Action node:18 depth:8
B tree:Action node:19 depth:4
B tree:Action node:20 depth:5
B tree:Action node:21 depth:5
B tree:Chance node:10 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Chance node:11 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Chance node:12 depth:6
B tree:Action node:22 depth:7
B tree:Action node:23 depth:8
B tree:Action node:24 depth:8
B tree:Action node:25 depth:9
B tree:Action node:26 depth:9
B tree:Action node:27 depth:9
B tree:Action node:28 depth:10
B tree:Action node:29 depth:10
B tree:Action node:30 depth:7
B tree:Action node:31 depth:8
B tree:Action node:32 depth:8
B tree:Action node:33 depth:8
B tree:Action node:34 depth:9
B tree:Action node:35 depth:9
B tree:Action node:36 depth:5
B tree:Action node:37 depth:6
B tree:Action node:38 depth:6
B tree:Chance node:10 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Chance node:11 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Chance node:12 depth:7
B tree:Action node:39 depth:8
B tree:Action node:40 depth:9
B tree:Action node:41 depth:9
B tree:Action node:42 depth:10
B tree:Action node:43 depth:10
B tree:Action node:44 depth:10
B tree:Action node:45 depth:11
B tree:Action node:46 depth:11
B tree:Action node:47 depth:8
B tree:Action node:48 depth:9
B tree:Action node:49 depth:9
B tree:Action node:50 depth:9
B tree:Action node:51 depth:10
B tree:Action node:52 depth:10
B tree:Action node:53 depth:3
B tree:Action node:54 depth:4
B tree:Action node:55 depth:4
B tree:Chance node:10 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Chance node:11 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Chance node:12 depth:5
B tree:Action node:56 depth:6
B tree:Action node:57 depth:7
B tree:Action node:58 depth:7
B tree:Action node:59 depth:8
B tree:Action node:60 depth:8
B tree:Action node:61 depth:8
B tree:Action node:62 depth:9
B tree:Action node:63 depth:9
B tree:Action node:64 depth:6
B tree:Action node:65 depth:7
B tree:Action node:66 depth:7
B tree:Action node:67 depth:7
B tree:Action node:68 depth:8
B tree:Action node:69 depth:8
B tree:Action node:70 depth:4
B tree:Action node:71 depth:5
B tree:Action node:72 depth:5
B tree:Chance node:10 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:11 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
B tree:Chance node:12 depth:6
B tree:Action node:73 depth:7
B tree:Action node:74 depth:8
B tree:Action node:75 depth:8
B tree:Action node:76 depth:9
B tree:Action node:77 depth:9
B tree:Action node:78 depth:9
B tree:Action node:79 depth:10
B tree:Action node:80 depth:10
B tree:Action node:81 depth:7
B tree:Action node:82 depth:8
B tree:Action node:83 depth:8
B tree:Action node:84 depth:8
B tree:Action node:85 depth:9
B tree:Action node:86 depth:9
";

        #endregion


        #region Implementation


        class TestWalker : WalkChanceAndActionTrees<WalkChanceAndActionTreesContext> //<WalkChanceAndActionTrees.Context>
        {
            public int[] BeginCount = new int[2];
            public string  Text = "";

            protected override void  OnNodeBegin(WalkChanceAndActionTreesContext[] stack, int depth)
            {
                WalkChanceAndActionTreesContext context = stack[depth];
                TreeKind treeKind = context.TreeKind;
                string line = String.Format("B tree:{0} node:{1} depth:{2}\r\n", treeKind, context.NodeIdx[(int)treeKind], depth);
                Text += line;
                // Console.Write(line);
                BeginCount[(int)treeKind]++;
            }
        }

        #endregion
    }
}
