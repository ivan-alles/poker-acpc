using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using System.Reflection;
using System.Drawing;

namespace convex_kuhn
{
    public class HeroTreeVis : VisPokerTree<Calculator.TreeNode, Calculator.TreeNode, int, HeroTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<Calculator.TreeNode, int>
        {
        }

        public HeroTreeVis()
        {
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));
        }

        public Calculator Solver
        {
            set;
            get;
        }

        public List<string> FlatStrategy;

        protected override void OnTreeBeginFunc(Calculator.TreeNode tree, Calculator.TreeNode root)
        {
            GraphAttributes.label = Solver.GameDef.Name + " hero tree pos " + Solver.HeroPosition.ToString();
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(tree, root);
        }

        protected override bool OnNodeBeginFunc(Calculator.TreeNode tree, Calculator.TreeNode node,
            List<Context> stack, int depth)
        {
            Context context = stack[depth];
            context.Action = node.Action;
            context.State = node.State;

            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        protected override void CustomizeNodeAttributes(Calculator.TreeNode tree, Calculator.TreeNode node, List<Context> stack, int depth, NodeAttributeMap attr)
        {
            base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
            string label = "";
            int fontSize = 15;
            if (node.State.HasPlayerActed(Solver.HeroPosition))
            {
                label = String.Format("<<FONT FACE = \"ARIAL\" POINT-SIZE=\"{0}\">{1}</FONT>>", fontSize, FlatStrategy[node.Id]);
            }
            attr.label = label;
        }

        static void GrayNode(NodeAttributeMap attr)
        {
            if(IsHtmlColor(attr.fillcolor))
            {
                attr.fillcolor = GradientHtmlColor(attr.fillcolor, Color.FromArgb(255, 200, 200, 200), 0.4);
            }
        }
    }
}
