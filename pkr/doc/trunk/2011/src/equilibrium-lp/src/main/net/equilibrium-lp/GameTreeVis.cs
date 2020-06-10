using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using System.Reflection;

namespace equilibrium_lp
{
    public class GameTreeVis : VisPokerTree<EquilibriumSolverLp.TreeNode, EquilibriumSolverLp.TreeNode, int, GameTreeVis.Context>
    {
        public class Context : VisPokerTreeContext<EquilibriumSolverLp.TreeNode, int>
        {
        }

        public GameTreeVis()
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
            if (node.State.Players[Solver.HeroPosition].PrivateCards == "K")
                return false;

            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        protected override void CustomizeNodeAttributes(EquilibriumSolverLp.TreeNode tree, EquilibriumSolverLp.TreeNode node, List<Context> stack, int depth, ai.lib.algorithms.tree.VisTree<EquilibriumSolverLp.TreeNode, EquilibriumSolverLp.TreeNode, int, Context>.NodeAttributeMap attr)
        {
            base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
            string label = "";
            if (node.State.IsGameOver)
            {
                label = string.Format("{0:0.00}·{1}     ", node.TerminalCoeffs_h[0], Solver.Vars.GetName(node.TerminalVars_h[0]));
            }
            attr.label = label;
        }
    }
}
