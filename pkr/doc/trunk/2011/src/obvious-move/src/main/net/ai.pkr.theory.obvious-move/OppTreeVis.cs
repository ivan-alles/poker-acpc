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
    public class OppTreeVis : VisPokerTree<ObviousMoveSolver.TreeNode, ObviousMoveSolver.TreeNode, int, OppTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<ObviousMoveSolver.TreeNode, int>
        {
        }

        public OppTreeVis()
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
            GraphAttributes.label = Solver.GameDef.Name + " opponent tree for hero pos " + Solver.HeroPosition.ToString();
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(tree, root);
        }

        protected override bool OnNodeBeginFunc(ObviousMoveSolver.TreeNode tree, ObviousMoveSolver.TreeNode node,
            List<Context> stack, int depth)
        {
            Context context = stack[depth];
            context.Action = node.Action;
            context.State = node.State;

            // Skip part of the tree.
            /*if (node.State.Players[1-Solver.HeroPosition].PrivateCards == "K")
                return false;*/

            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        protected override void CustomizeNodeAttributes(ObviousMoveSolver.TreeNode tree, ObviousMoveSolver.TreeNode node, List<Context> stack, int depth, ai.lib.algorithms.tree.VisTree<ObviousMoveSolver.TreeNode, ObviousMoveSolver.TreeNode, int, Context>.NodeAttributeMap attr)
        {
            base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
            string rightMargin = "";
            string leftMargin = "     ";
            string label = "<";
            label += rightMargin + String.Format("<BR ALIGN=\"LEFT\"/>Id:{0}", node.Id);
            if (node.State.IsGameOver)
            {
                Dictionary<string, int> cardToVarH = new Dictionary<string, int> { { "J", -1 }, { "Q", -1 }, { "K", -1 }, };
                for (int i = 0; i < node.TerminalVars_h.Count; ++i)
                {
                    cardToVarH[Solver.VarHToCard[node.TerminalVars_h[i]]] = i;
                }
                string[] cardSequence = new string[] {"J", "Q", "K"};

                foreach (string card in cardSequence)
                {
                    string row = rightMargin + "<BR ALIGN=\"LEFT\"/>" + card + ":";
                    int varHIdx = cardToVarH[card];
                    if (varHIdx != -1)
                    {
                        row += string.Format("{0:+#.00;-#.00;0}·{1}", node.TerminalCoeffs_h[varHIdx],
                                             Solver.Vars.GetName(node.TerminalVars_h[varHIdx]));
                    }
                    else
                    {
                        row += "n/a";
                    }
                    label += row + leftMargin;
                }
            }

            label += rightMargin + "<BR ALIGN=\"LEFT\"/>>";
            attr.label = label;
            //attr.width = 1.5;
            attr.margin = "0.1,0.05";
            // Draw unplayed nodes grayed
            if (IsHtmlColor(attr.fillcolor))
            {
                switch (Solver.HeroPosition)
                {
                    case 0:
                        switch (node.Id)
                        {
                            case 10:
                            case 27:
                                GrayNode(attr);
                                break;
                        }
                        break;
                    case 1:
                        switch (node.Id)
                        {
                            case 7:
                            case 26:
                                GrayNode(attr);
                                break;
                        }
                        break;
                }
            }
        }

        static void GrayNode(NodeAttributeMap attr)
        {
            if (IsHtmlColor(attr.fillcolor))
            {
                attr.fillcolor = GradientHtmlColor(attr.fillcolor, Color.FromArgb(255, 200, 200, 200), 0.4);
            }
        }
    }
}
