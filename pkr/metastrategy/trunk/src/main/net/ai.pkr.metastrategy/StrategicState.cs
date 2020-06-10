/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.lib.algorithms.numbers;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Game state for strategy algos. It is very similar to GameState but is much
    /// simpler. It only accumulates the state informations from known strategic 
    /// actions or tree nodes. The intended usage is to traverse an existing game tree and feed the 
    /// state with actions from the tree. It is not used to get information about 
    /// possible following actions.
    /// </summary>
    public class StrategicState
    {
        public StrategicState(int playersCount)
        {
            InPot = new double[playersCount];
            Hands = new int[playersCount][];

            for (int p = 0; p < InPot.Length; ++p)
            {
                Hands[p] = new int[0];
                ActivePlayers |= (UInt16)(1 << p);
            }
        }

        public int PlayersCount
        {
            get 
            {
                return InPot.Length;
            }
        }

        public double[] InPot
        {
            private set;
            get;
        }

        public int[][] Hands
        {
            private set;
            get;
        }

        public UInt16 ActivePlayers
        {
            get;
            set;
        }

        public double Pot
        {
            set;
            get;
        }

        /// <summary>
        /// Update by a strategic action.
        /// </summary>
        public StrategicState GetNextState(IStrategicAction action)
        {
            StrategicState next = CreateNextState();
            IPlayerAction pa = action as IPlayerAction;
            if (pa != null)
            {
                UpdateAmount(next, pa.Position, pa.Amount);
                IActionTreeNode an = pa as IActionTreeNode;
                if(an != null)
                {
                    next.ActivePlayers = an.ActivePlayers;
                }
            }
            else
            {
                IDealerAction da = action as IDealerAction;
                UpdateHand(next, da.Position, da.Card);
            }

            return next;
        }

        /// <summary>
        /// Update by a strategy tree node. Cannot update active players.
        /// </summary>
        public StrategicState GetNextState(IStrategyTreeNode n)
        {
            StrategicState next = CreateNextState();
            if (!n.IsDealerAction)
            {
                UpdateAmount(next, n.Position, n.Amount);
            }
            else
            {
                UpdateHand(next, n.Position, n.Card);
            }
            return next;
        }

        /// <summary>
        /// Returns true, if the specified player has in pot strictly more than any other players.
        /// This is an equvivalent of a raise.
        /// </summary>
        public bool HasStrictMaxInPot(int position)
        {
            for (int p = 0; p < InPot.Length; ++p)
            {
                if (p == position) continue;
                if (InPot[position] <= InPot[p])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true, if the specified player has in pot more or equal than any other players.
        /// This is an equvivalent of a raise, check or call.
        /// </summary>
        public bool HasMaxOrEqualInPot(int position)
        {
            for (int p = 0; p < InPot.Length; ++p)
            {
                if (p == position) continue;
                if (InPot[position] < InPot[p])
                {
                    return false;
                }
            }
            return true;
        }


        private StrategicState CreateNextState()
        {
            StrategicState next = new StrategicState(InPot.Length);
            for (int p = 0; p < InPot.Length; ++p)
            {
                next.InPot[p] = InPot[p];
                next.Hands[p] = Hands[p].ShallowCopy();
            }
            next.Pot = Pot;
            next.ActivePlayers = ActivePlayers;
            return next;
        }

        private void UpdateAmount(StrategicState next, int position, double amount)
        {
            next.Pot += amount;
            next.InPot[position] += amount;
        }

        private void UpdateHand(StrategicState next, int position, int card)
        {
            int handSize = next.Hands[position].Length;
            Array.Resize(ref next.Hands[position],  handSize + 1);
            next.Hands[position][handSize] = card;
        }
    }
}
