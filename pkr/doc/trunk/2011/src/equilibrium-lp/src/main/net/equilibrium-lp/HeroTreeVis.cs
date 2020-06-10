using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using System.Reflection;

namespace equilibrium_lp
{
    public class HeroTreeVis : VisPokerTree<EquilibriumSolverLp.TreeNode, EquilibriumSolverLp.TreeNode, int, HeroTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<EquilibriumSolverLp.TreeNode, int>
        {
        }

        public HeroTreeVis()
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

            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        protected override void CustomizeNodeAttributes(EquilibriumSolverLp.TreeNode tree, EquilibriumSolverLp.TreeNode node, List<Context> stack, int depth, ai.lib.algorithms.tree.VisTree<EquilibriumSolverLp.TreeNode, EquilibriumSolverLp.TreeNode, int, Context>.NodeAttributeMap attr)
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
                attr.label = String.Format("<<FONT FACE = \"ARIAL\" COLOR=\"{2}\" POINT-SIZE=\"{0}\">{1}</FONT>>", fontSize, label, color);
            }
            else
            {
                if (node.State.IsPlayerActing(Solver.HeroPosition))
                {
                    attr.label = "1";
                }
                else
                {
                    attr.label = "";
                }
            }

        }
    }
}
