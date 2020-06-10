/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.algorithms.tree;
using ai.lib.utils;
using System.Reflection;
using ai.lib.algorithms;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A tree of cluster nodes to build a chance abstraction based on clusters of some metric.
    /// </summary>
    public class ClusterTree
    {
        /// <summary>
        /// Method to access children for standard tree algorithms.
        /// </summary>
        public static bool TreeGetChild(ClusterTree tree, IClusterNode n, ref int i, out IClusterNode child)
        {
            if (i >= n.ChildrenCount)
            {
                child = null;
                return false;
            }
            child = n.GetChild(i++);
            return true;
        }

        public ClusterTree()
        {
            Version = new BdsVersion(Assembly.GetExecutingAssembly());
        }

        public IClusterNode Root
        {
            set;
            get;
        }

        public BdsVersion Version
        {
            get;
            private set;
        }

        public void Write(BinaryWriter w)
        {
            // Write version first to allow standard tools work.
            Version.Write(w);
            w.Write(SERIALIZATION_FORMAT_VERSION);
            w.Write(Root.SerializationFormatVersion);
            Type t = Root.GetType();
            string typeName = t.AssemblyQualifiedName;
            w.Write(typeName);
            Root.Write(w);
        }

        public void Write(string fileName)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
            {
                Write(bw);
            }
        }

        /// <summary>
        /// Binary deserialization from reader.
        /// </summary>
        public static ClusterTree Read(BinaryReader r) 
        {
            ClusterTree newTree = new ClusterTree();
            newTree.ReadInternal(r);
            return newTree;
        }

        /// <summary>
        /// Binary deserialization from file.
        /// </summary>
        public static ClusterTree Read(string fileName)
        {
            ClusterTree tree;
            using (BinaryReader br = new BinaryReader(
                File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                tree = ClusterTree.Read(br);
            }
            return tree;
        }


        public class Vis : VisTree<ClusterTree, IClusterNode, int, Vis.Context>
        {
            public class Context : VisTreeContext<IClusterNode, int>
            {
                public string Cards = "";
            }

            public Vis()
            {
                CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));

                ShowExpr.Clear();
                ShowExpr.Add(new ExprFormatter("s[d].Node.ToGvString();{1}"));
            }

            public bool ShowDealPath
            {
                set;
                get;
            }

            protected override bool OnNodeBeginFunc(ClusterTree tree, IClusterNode node, List<Context> stack, int depth)
            {
                Context context = stack[depth];

                if (depth > 0)
                {
                    Context parentContext = stack[depth - 1];
                    context.Cards = parentContext.Cards;
                    if (context.Cards != "")
                    {
                        context.Cards += " ";
                    }
                    context.Cards += (parentContext.ChildrenIterator-1).ToString();
                }
                return base.OnNodeBeginFunc(tree, node, stack, depth);
            }

            protected override void OnTreeBeginFunc(ClusterTree tree, IClusterNode root)
            {
                base.OnTreeBeginFunc(tree, root);
                Output.WriteLine("subgraph[peripheries=0]");
            }

            protected override void VisNode(ClusterTree tree, IClusterNode node, List<Vis.Context> stack, int depth)
            {
                if (depth > 0)
                {
                    Output.WriteLine (string.Format("subgraph cluster{0} {{", stack[depth-1].GvId));
                }
                base.VisNode(tree, node, stack, depth);
                if (depth > 0)
                {
                    Output.WriteLine ("}");
                }
            }

            protected override void CustomizeNodeAttributes(ClusterTree tree, IClusterNode node, List<Context> stack, int depth, 
                NodeAttributeMap attr)
            {
                Context context = stack[depth];
                base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
                string label = attr.label;
                if (ShowDealPath)
                {
                    label = "c: " + context.Cards + ".\\n" + label;
                }
                attr.label = label;
            }
        }

        public void Visualize(string fileName)
        {
            using (TextWriter output = new StreamWriter(fileName))
            {
                var vis = new Vis
                {
                    Output = output
                };
                vis.Walk(this, Root);
            }
        }

        #region Implementation

        private const int SERIALIZATION_FORMAT_VERSION = 3;


        private void ReadInternal(BinaryReader r)
        {
            Version = new BdsVersion();
            Version.Read(r);
            int serFmtVer = r.ReadInt32();
            if(serFmtVer > SERIALIZATION_FORMAT_VERSION)
            {
                throw new ApplicationException(string.Format(
                                                   "Unsupported serialization format: {0}, max supported: {1}",
                                                   serFmtVer, SERIALIZATION_FORMAT_VERSION));
            }

            if (serFmtVer == 1)
            {
                throw new ApplicationException("Serialization format version 1 is not supported");
            }

            int nodeSerFmtVer = 0;
            if(serFmtVer >= 3)
            {
                nodeSerFmtVer = r.ReadInt32();
            }

            IClusterNode root;
            string typeName = r.ReadString();
            ClassFactoryParams p = new ClassFactoryParams(typeName, "");
            root = ClassFactory.CreateInstance<IClusterNode>(p);
            root.Read(r, nodeSerFmtVer);
            Root = root;
        }

        #endregion
    }
}
