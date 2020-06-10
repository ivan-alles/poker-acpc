/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.Reflection;
using System.IO;
using ai.pkr.metagame;
using ai.pkr.metastrategy.nunit;
using ai.pkr.metastrategy;
using ai.lib.algorithms.tree;
using ai.pkr.metastrategy.vis;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for VerifyAbsStrategy. 
    /// </summary>
    [TestFixture]
    public unsafe class VerifyAbsStrategy_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            for (_heroPos = 0; _heroPos < gd.MinPlayers; ++_heroPos)
            {
                StrategyTree st = CreateValidStrategy(gd);

                string fileName = Path.Combine(_outDir, string.Format("{0}-{1}.gv", gd.Name, _heroPos));
                VisStrategyTree.Show(st, fileName);

                string errorText;
                Assert.IsTrue(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));

                // Now make some errors.
                if (_heroPos == 0)
                {
                    st.Nodes[18].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    string expTextBegin = string.Format("Node {0},", 12);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));
                    st.Nodes[18].Probab -= 0.1;

                    st.Nodes[17].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    expTextBegin = string.Format("Node {0},", 15);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));

                }
                else
                {
                    st.Nodes[15].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    string expTextBegin = string.Format("Node {0},", 13);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));
                }
            }
        }

        [Test]
        public void Test_Leduc()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));

            for (_heroPos = 0; _heroPos < gd.MinPlayers; ++_heroPos)
            {
                StrategyTree st = CreateValidStrategy(gd);

                string fileName = Path.Combine(_outDir, string.Format("{0}-{1}.gv", gd.Name, _heroPos));
                VisStrategyTree.Show(st, fileName);

                string errorText;
                Assert.IsTrue(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));

                // Now make some errors.
                if (_heroPos == 0)
                {
                    st.Nodes[339].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    string expTextBegin = string.Format("Node {0},", 342);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));
                    st.Nodes[339].Probab -= 0.1;

                    st.Nodes[348].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    expTextBegin = string.Format("Node {0},", 345);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));

                }
                else
                {
                    st.Nodes[435].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    string expTextBegin = string.Format("Node {0},", 439);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));
                    st.Nodes[435].Probab -= 0.1;

                    st.Nodes[432].Probab += 0.1;
                    Assert.IsFalse(VerifyAbsStrategy.Verify(st, _heroPos, out errorText));
                    expTextBegin = string.Format("Node {0},", 429);
                    Assert.AreEqual(expTextBegin, errorText.Substring(0, expTextBegin.Length));

                }

            }
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class Context : WalkUFTreePPContext
        {
            public double ParentProbab = 1;
        }

        private StrategyTree CreateValidStrategy(GameDefinition gd)
        {
            StrategyTree st = TreeHelper.CreateStrategyTree(gd, _heroPos);
            _childrenCount = new int[st.NodesCount];
            WalkUFTreePP<StrategyTree, Context> wt = new WalkUFTreePP<StrategyTree, Context>();
            wt.OnNodeBegin = OnNodeBegin;
            _run = 0;
            wt.Walk(st);

            _run = 1;
            wt.Walk(st);
            return st;
        }

        void OnNodeBegin(StrategyTree tree, Context[] stack, int depth)
        {
            Context context = stack[depth];
            Int64 n = context.NodeIdx;
            if (depth > 0)
            {
                context.ParentProbab = stack[depth - 1].ParentProbab;
            }
            if (tree.Nodes[n].IsPlayerAction(_heroPos))
            {
                if (_run == 0)
                {
                    // Count children here.
                    _childrenCount[stack[depth - 1].NodeIdx] += 1;
                }
                else
                {
                    tree.Nodes[n].Probab = context.ParentProbab / _childrenCount[stack[depth - 1].NodeIdx];
                    context.ParentProbab = tree.Nodes[n].Probab;
                }
            }
        }

        int _run;
        int _heroPos;
        int []_childrenCount;

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/VerifyAbstStrategyTree_Test");


        #endregion


    }
}
