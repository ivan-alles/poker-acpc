/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;
using ai.lib.algorithms.numbers;
using ai.pkr.metastrategy;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Verifies a strategy stored in a StrategyTree with absolute probabilities.
    /// Make sure that sum of probabilities of children in nodes where the hero moves equals to 
    /// the probability of a parent node.
    /// <para>
    /// Usage: either create instance, set properties and call Walk() or use static methods.
    /// </para>
    /// <para>
    /// Default format: double values.
    /// </para>
    /// </summary>
    public unsafe class VerifyAbsStrategy : WalkUFTreePP<StrategyTree, VerifyAbsStrategy.Context>
    {
        public class Context : WalkUFTreePPContext
        {
            /// <summary>
            /// Own probability of a node.
            /// </summary>
            internal double Probability;
            internal double SumProbabilityOfChildren;
            /// <summary>
            /// Is set to true for nodes that where the hero makes moves.
            /// </summary>
            internal bool IsVerificationNeeded;
        }

        public const double DEFAULT_EPSILON = 0.0;

        public int HeroPosition
        {
            set;
            get;
        }

        public double Epsilon
        {
            set;
            get;
        }

        /// <summary>
        /// Checks sum(children) == parent only if the sum is non-zero.
        /// This is useful for abstract strategies converted from non-abstract ones. 
        /// They have zero values in nodes that can have 0 chance probability and cannot occur in reality.
        /// Default: false.
        /// </summary>
        public bool NonZeroSumsOnly
        {
            set;
            get;
        }

        public bool IsOK
        {
            protected set;
            get;
        }

        public string ErrorText
        {
            protected set;
            get;
        }

        public VerifyAbsStrategy()
        {
            Epsilon = DEFAULT_EPSILON;
            NonZeroSumsOnly = false;
        }

        /// <summary>
        /// Verifies a strategy.
        /// </summary>
        public static bool Verify(StrategyTree strategy, int heroPos, double epsilon, bool nonZeroSumsOnly, out string errorText)
        {
            VerifyAbsStrategy verifier = new VerifyAbsStrategy
                                             {
                                                 HeroPosition = heroPos,
                                                 Epsilon = epsilon,
                                                 NonZeroSumsOnly = nonZeroSumsOnly
                                             };
            verifier.Walk(strategy);

            errorText = verifier.ErrorText;
            return verifier.IsOK;
        }

        /// <summary>
        /// Verifies a strategy.
        /// </summary>
        public static bool Verify(StrategyTree strategy, int heroPos, double epsilon, out string errorText)
        {
            return Verify(strategy, heroPos, epsilon, false, out errorText);
        }

        /// <summary>
        /// Verify with default epsilon.
        /// </summary>
        public static bool Verify(StrategyTree strategy, int heroPos, out string errorText)
        {
            return Verify(strategy, heroPos, DEFAULT_EPSILON, out errorText);
        }

        public override void Walk(StrategyTree tree)
        {
            IsOK = true;
            ErrorText = "";
            try
            {
                // Start from the last blind.
                base.Walk(tree, tree.PlayersCount);
            }
            catch (VerificationException e)
            {
                IsOK = false;
                ErrorText = e.Text;
            }
        }

        protected override void OnNodeBeginFunc(StrategyTree tree, Context[] stack, int depth)
        {
            Int64 n = stack[depth].NodeIdx;
            stack[depth].SumProbabilityOfChildren = 0;
            stack[depth].IsVerificationNeeded = false;

            int playersCount = tree.PlayersCount;
            if(depth == playersCount)
            {
                stack[depth].Probability = 1.0;
            }
            else
            {
                if (!tree.Nodes[n].IsDealerAction && tree.Nodes[n].Position == HeroPosition)
                {
                    double probab = tree.Nodes[n].Probab;
                    stack[depth].Probability = probab;
                    stack[depth - 1].SumProbabilityOfChildren += probab;
                    stack[depth - 1].IsVerificationNeeded = true;
                }
                else
                {
                    stack[depth].Probability = stack[depth - 1].Probability;
                }
            }
        }

        protected override void OnNodeEndFunc(StrategyTree tree, Context[] stack, int depth)
        {
            if (stack[depth].IsVerificationNeeded)
            {
                Context context = stack[depth];
                if (!NonZeroSumsOnly || context.SumProbabilityOfChildren != 0)
                {
                    if (!FloatingPoint.AreEqual(context.SumProbabilityOfChildren, context.Probability, Epsilon))
                    {
                        throw new VerificationException
                        {
                            Text =
                                string.Format("Node {0}, depth {1}: sum of childen {2} differs from expected {3} (epsilon {4}).",
                                context.NodeIdx, depth, context.SumProbabilityOfChildren, context.Probability, Epsilon)
                        };
                    }
                }
            }
        }

        #region Implementation
        
        class VerificationException : Exception
        {
            public string Text;
        }

        #endregion
    }
}