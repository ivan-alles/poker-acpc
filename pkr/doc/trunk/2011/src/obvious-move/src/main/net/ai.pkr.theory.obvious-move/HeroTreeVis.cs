using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using System.Reflection;
using System.Drawing;

namespace equilibrium_lp
{
    public class HeroTreeVis : VisPokerTree<ObviousMoveSolver.TreeNode, ObviousMoveSolver.TreeNode, int, HeroTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<ObviousMoveSolver.TreeNode, int>
        {
        }

        public HeroTreeVis()
        {
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));
        }

        public ObviousMoveSolver Solver
        {
            set;
            get;
        }

        protected override void OnTreeBeginFunc(ObviousMoveSolver.TreeNode tree, ObviousMoveSolver.TreeNode root)
        {
            GraphAttributes.label = Solver.GameDef.Name + " hero tree pos " + Solver.HeroPosition.ToString();
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(tree, root);
        }

        protected override bool OnNodeBeginFunc(ObviousMoveSolver.TreeNode tree, ObviousMoveSolver.TreeNode node,
            List<Context> stack, int depth)
        {
            Context context = stack[depth];
            context.Action = node.Action;
            context.State = node.State;

            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        protected override void CustomizeNodeAttributes(ObviousMoveSolver.TreeNode tree, ObviousMoveSolver.TreeNode node, List<Context> stack, int depth, NodeAttributeMap attr)
        {
            base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
            string label = "";
            int fontSize = 15;
            string color = "#A0A0A0";
            if (node.Var_h >= 0)
            {
                label = Solver.Vars.GetName(node.Var_h);
                if (node.State.HasPlayerActed(Solver.HeroPosition))
                {
                    color = "#000000";
                }
                label = String.Format("<<FONT FACE = \"ARIAL\" COLOR=\"{2}\" POINT-SIZE=\"{0}\">{1}</FONT>", fontSize, label, color);

            }
            else
            {
                label = "<";
            }
            switch (Solver.HeroPosition)
            {
                case 0:
                    switch (node.Id)
                    {
                        case 5: label += String.Format("<BR/>{0}", "0.1"); break;
                        case 12: label += String.Format("<BR/>{0}", "0.3"); break;
                        case 25: label += String.Format("<BR/>{0}", "0.2"); break;
                    }
                    break;
                case 1:
                    switch (node.Id)
                    {
                        case 8: label += String.Format("<BR/>{0}", "1.1"); break;
                        case 12: label += String.Format("<BR/>{0}", "1.3"); break;
                        case 26: label += String.Format("<BR/>{0}", "1.2"); break;
                    }
                    break;
            }
            label += ">";
            attr.label = label;

            // Draw unplayed nodes grayed
            if (IsHtmlColor(attr.fillcolor))
            {
                switch (Solver.HeroPosition)
                {
                    case 0:
                        switch (node.Id)
                        {
                            case 7:
                            case 26:
                                GrayNode(attr); 
                                break;
                        }
                        break;
                    case 1:
                        switch (node.Id)
                        {
                            case 10:
                            case 27:
                                GrayNode(attr); 
                                break;
                        }
                        break;
                }
            }
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
