/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.InteropServices;
using ai.lib.utils;
using System.IO;
using System.Reflection;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class UFTree_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public unsafe void Test_Simple()
        {
            TestTree tree = new TestTree(5);

            tree.SetDepth(0, 0);
            tree.Nodes[0].Id = 0;

            tree.SetDepth(1, 1);
            tree.Nodes[1].Id = 1;

            tree.SetDepth(2, 2);
            tree.Nodes[2].Id = 2;

            tree.SetDepth(3, 2);
            tree.Nodes[3].Id = 3;

            tree.SetDepth(4, 1);
            tree.Nodes[4].Id = 4;
        }

        [Test]
        public unsafe void Test_ReadWrite()
        {
            int nodesCount = 1 + 4 + 4*4 + 4*4*4;
            TestTree tree = new TestTree(nodesCount);
            int idx = 0;
            CreateTestTree(tree, ref idx, 0, 3, 4);
            tree.Version.Major = 4;
            tree.Version.Minor = 2;
            tree.UserData = 1234567;

            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            tree.Write(w);
            byte[] buffer = ms.ToArray();
            ms = new MemoryStream(buffer);
            BinaryReader r = new BinaryReader(ms);
            TestTree tree1 = TestTree.Read<TestTree>(r);

            Assert.AreEqual(tree.Version, tree1.Version);
            Assert.AreEqual(tree.UserData, tree1.UserData);
            Assert.AreEqual(tree.NodesCount, tree1.NodesCount);
            for (int i = 0; i < tree.NodesCount; ++i)
            {
                Assert.AreEqual(tree.GetDepth(i), tree1.GetDepth(i));
                Assert.AreEqual(tree.Nodes[i].Id, tree1.Nodes[i].Id);
            }
        }

        [Test]
        public unsafe void Test_ReadWriteFDA()
        {
            int nodesCount = 1 + 4 + 4 * 4 + 4 * 4 * 4;
            TestTree tree = new TestTree(nodesCount);
            int idx = 0;
            CreateTestTree(tree, ref idx, 0, 3, 4);
            tree.Version.Major = 4;
            tree.Version.Minor = 2;
            tree.UserData = 1234567;
            string fileName = Path.Combine(_outDir, "uftree.dat");
            tree.Write(fileName);
            TestTree tree1 = TestTree.ReadFDA<TestTree>(fileName);

            Assert.AreEqual(tree.Version, tree1.Version);
            // Do not check the user data, it is not supported by FDA
            Assert.AreEqual(tree.NodesCount, tree1.NodesCount);
            for (int i = 0; i < tree.NodesCount; ++i)
            {
                Assert.AreEqual(tree.GetDepth(i), tree1.GetDepth(i), i.ToString());
                TestNode n, n1;
                tree.GetNode(i, (byte*)&n);
                tree1.GetNode(i, (byte*)&n1);
                Assert.AreEqual(n.Id, n1.Id, i.ToString());
            }
        }

        #endregion

        #region Benchmarks


        #endregion

        #region Implementation

        struct TestNode 
        {
            public int Id;
        }

        unsafe class TestTree: UFTree
        {
            public TestTree()
            {
            }

            public TestTree(Int64 nodesCount): base(nodesCount, Marshal.SizeOf(typeof(TestNode)))
            {
                SetNodesMemory(0);
                AfterRead();
            }

            protected override void WriteUserData(BinaryWriter w)
            {
                w.Write(UserData);
            }

            protected override void ReadUserData(BinaryReader r)
            {
                UserData = r.ReadInt32();
            }

            protected override void AfterRead()
            {
                if (_nodesPtr != null)
                {
                    _nodes = (TestNode*) _nodesPtr.Ptr.ToPointer();
                }
            }

            public unsafe TestNode * Nodes
            {
                get { return _nodes; }
            }

            public int UserData
            {
                set;
                get;
            }

            TestNode* _nodes;
        }

        unsafe void CreateTestTree(TestTree tree, ref int nodeIdx, int curDepth, int depthLimit, int childCount)
        {
            if (curDepth > depthLimit)
            {
                return;
            }
            tree.SetDepth(nodeIdx, (byte)curDepth);
            tree.Nodes[nodeIdx].Id = nodeIdx++;
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestTree(tree, ref nodeIdx, curDepth + 1, depthLimit, childCount);
            }
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "tree/UFTree_Test");

        #endregion
    }
}
