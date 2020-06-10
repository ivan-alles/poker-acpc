/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using System.IO;
using System.Reflection;
using System.Xml;
using ai.lib.utils;
using System.Globalization;
using System.Text.RegularExpressions;
using ai.lib.algorithms;
using ai.pkr.metastrategy;


namespace ai.pkr.theory.exploitation
{
    [TestFixture]
    public class MoveInfluence
    {
        #region Tests

        [Test]
        public void Test_GenerateFiles()
        {
            string libDir = Path.GetFileNameWithoutExtension(CodeBase.Get(Assembly.GetExecutingAssembly()));
            string dataDir = Props.Global.Get("bds.DataDir");
            string workingDir = Directory.GetCurrentDirectory() + @"..\..\..\..\";

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Path.Combine(dataDir, "ai.pkr.metastrategy.kuhn.gamedef.1.xml"));

            using (TextWriter tw = new StreamWriter(Path.Combine(workingDir, "p1.gv")))
            {
                GenTree tree = new GenTree { GameDef = gd, Kind = GenTree.TreeKind.PlayerTree,
                    HeroPosition = 1 };
                DeckGenNode root = new DeckGenNode(tree, gd.MinPlayers);

                Vis vis = new Vis { Output = tw };

                SetVisAttributes(vis);
                //vis.ShowExpr.Add(new ExprFormatter("s[d].Node.Id", "id:{1}"));
                vis.Walk(tree, root);
            }

            using (TextWriter tw = new StreamWriter(Path.Combine(workingDir, "g.gv")))
            {
                GenTree tree = new GenTree
                {
                    GameDef = gd,
                    Kind = GenTree.TreeKind.GameTree,
                    HeroPosition = 0
                };
                DeckGenNode root = new DeckGenNode(tree, gd.MinPlayers);

                Vis vis = new Vis { Output = tw };

                SetVisAttributes(vis);
                //vis.ShowExpr.Add(new ExprFormatter("s[d].Node.Id", "id:{1}"));
                vis.Walk(tree, root);
            }
        }

        private void SetVisAttributes(VisPokerGenTree vis)
        {
            vis.GraphAttributes.Map["fontname"] = "arial";
            vis.GraphAttributes.fontsize = 10;
            vis.NodeAttributes.fontsize = 10;
            vis.EdgeAttributes.fontsize = 10;
            vis.MergePrivateDeals = true;
            vis.NodeAttributes.width = 0.3;
            vis.NodeAttributes.height = 0.2;
        }

        #endregion


        #region Implementation

        class Vis : VisPokerGenTree
        {
            protected override void CustomizeNodeAttributes(GenTree tree, GenNode node, List<VisPokerTreeContext<GenNode, int>> stack, int depth, ai.lib.algorithms.tree.VisTree<GenTree, GenNode, int, VisPokerTreeContext<GenNode, int>>.NodeAttributeMap attr)
            {
                base.CustomizeNodeAttributes(tree, node, stack, depth, attr);

                if (node.State.Players[1].PrivateCards == "K")
                {
                    for(int d  = depth; d > 0; --d)
                    {
                        if(stack[d].Node.Action.Kind == Ak.c && stack[d].Node.Action.Position == 0)
                        {
                            attr.penwidth = 3;
                            attr.color = "#0000FF";
                            break;
                        }
                    }
                }
                if(tree.Kind == GenTree.TreeKind.PlayerTree && node.Id == 21)
                {
                    attr.label = "n";
                }
            }

            protected override void CustomizeEdgeAttributes(GenTree tree, GenNode node, GenNode parent, List<VisPokerTreeContext<GenNode, int>> stack, int depth, ai.lib.algorithms.tree.VisTree<GenTree, GenNode, int, VisPokerTreeContext<GenNode, int>>.EdgeAttributeMap attr)
            {
                base.CustomizeEdgeAttributes(tree, node, parent, stack, depth, attr);
                if (attr.label.Contains("d"))
                {
                    attr.label = attr.label + "             ";
                }
                if (attr.label.Contains("1d{K}"))
                {
                    attr.label = String.Format("<<FONT POINT-SIZE=\"15\">{0}</FONT>>", attr.label);
                }
            }
        }


        #endregion
    }
}
