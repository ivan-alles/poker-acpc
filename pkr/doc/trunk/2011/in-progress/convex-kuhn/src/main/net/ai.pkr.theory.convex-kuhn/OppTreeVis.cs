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
    public class OppTreeVis : VisPokerTree<Calculator.TreeNode, Calculator.TreeNode, int, OppTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<Calculator.TreeNode, int>
        {
        }

        public OppTreeVis()
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
            GraphAttributes.label = Solver.GameDef.Name + " opponent tree for hero pos " + Solver.HeroPosition.ToString();
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(tree, root);
        }

        protected override bool OnNodeBeginFunc(Calculator.TreeNode tree, Calculator.TreeNode node,
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

        protected override void CustomizeNodeAttributes(Calculator.TreeNode tree, Calculator.TreeNode node, List<Context> stack, int depth, ai.lib.algorithms.tree.VisTree<Calculator.TreeNode, Calculator.TreeNode, int, Context>.NodeAttributeMap attr)
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
                        string varName = Solver.Vars.GetName(node.TerminalVars_h[varHIdx]);
                        int varNodeId = int.Parse(varName.Substring(1));
                        string varValue = FlatStrategy[varNodeId];
                        if(varValue.StartsWith("1 - "))
                        {
                            varValue = "(" + varValue + ")";
                        }
                        string text = "";
                        if(varValue == "1")
                        {
                            text = string.Format("{0:+#.00;-#.00;0}", node.TerminalCoeffs_h[varHIdx]);
                        }
                        else if (varValue == "0")
                        {
                            text = "0";
                        }
                        else
                        {
                            text = string.Format("{0:+#.00;-#.00;0}·{1}", node.TerminalCoeffs_h[varHIdx],
                                             varValue);
                        }

                        row += text;
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
        }

    }
}
