using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using System.Reflection;

namespace equilibrium_lp
{
    public class OppTreeVis : VisPokerTree<EquilibriumSolverLp.TreeNode, EquilibriumSolverLp.TreeNode, int, OppTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<EquilibriumSolverLp.TreeNode, int>
        {
        }

        public OppTreeVis()
        {
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));
        }

        public EquilibriumSolverLp Solver
        {
            set;
            get;
        }

        protected override bool OnNodeBeginFunc(EquilibriumSolverLp.TreeNode tree, EquilibriumSolverLp.TreeNode node,
            List<Context> stack, int depth)
        {
            Context context = stack[depth];
            context.Action = node.Action;
            context.State = node.State;

            // Skip part of the tree.
            if (node.State.Players[1-Solver.HeroPosition].PrivateCards == "K")
                return false;

            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        protected override void CustomizeNodeAttributes(EquilibriumSolverLp.TreeNode tree, EquilibriumSolverLp.TreeNode node, List<Context> stack, int depth, ai.lib.algorithms.tree.VisTree<EquilibriumSolverLp.TreeNode, EquilibriumSolverLp.TreeNode, int, Context>.NodeAttributeMap attr)
        {
            base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
            string rightMargin = "        ";
            string var = "v" + node.Id.ToString();
            string label = "<<BR ALIGN=\"LEFT\"/>" + var + "=";
            if (node.State.IsGameOver)
            {
                label += rightMargin + "<BR ALIGN=\"LEFT\"/>";
                for (int i = 0; i < node.TerminalVars_h.Count; ++i)
                {
                    if (i > 0 || node.TerminalCoeffs_h[i] < 0)
                    {
                        label += node.TerminalCoeffs_h[i] >= 0 ? "+" : "-";
                    }
                    label += string.Format("{0:#.00}·{1}", Math.Abs(node.TerminalCoeffs_h[i]),
                                           Solver.Vars.GetName(node.TerminalVars_h[i]));
                }
            }
            else
            {
                if (node.State.IsPlayerActing(1 - Solver.HeroPosition))
                {
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        if (c > 0)
                        {
                            label += "+";
                        }
                        label += "r"+ node.Children[c].Id.ToString();
                    }
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        label += rightMargin + "<BR ALIGN=\"LEFT\"/>";
                        label += var + "≤" + Solver.Vars.GetName(node.Children[c].Var_v);
                    }
                }
                else
                {
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        if (c > 0)
                        {
                            label += "+";
                        }
                        label += Solver.Vars.GetName(node.Children[c].Var_v);
                    }
                }
            }
            label += rightMargin + ">";
            attr.label = label;
            //attr.width = 1.5;
            attr.margin = "0.15,0.1";
        }
    }
}
