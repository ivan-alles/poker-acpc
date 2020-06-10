/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.pkr.metagame;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Create an action tree from game definition.
    /// </summary>
    public static unsafe class CreateActionTreeByGameDef
    {
        public static ActionTree Create(GameDefinition gd)
        {
            // First pass - count nodes.
            GlobalContext gc = new GlobalContext { GameDef = gd };
            GameContext root = new GameContext { GameState = new GameState(gd), Global  = gc};
            ProcessGameContext(root);
            // Create the tree, adding actions for blinds 
            int nodeCount = root.ChildCount + 1 + gd.MinPlayers;
            ActionTree tree = new ActionTree(nodeCount);
            // Second pass - initialize nodes.
            gc.Tree = tree;

            // Start from the last blind node
            gc.NodeId = gd.MinPlayers;
            root.Depth = gd.MinPlayers;

            ProcessGameContext(root);

            // Now fill the root and blind nodes (overwriting the last one, because it is wrong).

            gc.NodeId = 0;
            root.Depth = 0;

            // Set data for root.
            tree.SetDepth(gc.NodeId, (byte)root.Depth++);
            tree.Nodes[gc.NodeId].Position = gd.MinPlayers;
            tree.Nodes[gc.NodeId].Amount = 0;
            tree.Nodes[gc.NodeId].ActivePlayers = root.GameState.GetActivePlayers();
            tree.Nodes[gc.NodeId].Round = -1;

            gc.NodeId++;

            // Set data for blinds 
            for (int p = 0; p < gd.MinPlayers; ++p)
            {
                tree.SetDepth(gc.NodeId, (byte)root.Depth++);
                tree.Nodes[gc.NodeId].Position = p;
                tree.Nodes[gc.NodeId].Amount = root.GameState.Players[p].InPot;
                tree.Nodes[gc.NodeId].ActivePlayers = root.GameState.GetActivePlayers();
                tree.Nodes[gc.NodeId].Round = -1;
                gc.NodeId++;
            }

            tree.Version.Description = String.Format("Action tree (gamedef: {0})", gd.Name);

            return tree;
        }

        /// <summary>
        /// Global context, such as current node id.
        /// </summary>
        class GlobalContext
        {
            public GameDefinition GameDef;
            public int NodeId;
            public ActionTree Tree;
        }
        
        /// <summary>
        /// Information about game in the current node: state, dealt cards, etc.
        /// Is build incrementall by recursive function calls.
        /// </summary>
        class GameContext
        {
            public GameContext()
            {
            }

            public GameContext(GameContext other)
            {
                GameState = new GameState(other.GameState);
                Global = other.Global;
                Depth = other.Depth;
            }

            public GlobalContext Global;

            public GameState GameState;

            public int ChildCount;

            public double Amount;

            public int Depth;
        }

        private static void ProcessGameContext(GameContext context)
        {
            int nodeId = context.Global.NodeId++;
            GameDefinition gd = context.Global.GameDef;
            ActionTree tree = context.Global.Tree;

            // Store it before we possible change the state to skip dealer actions.
            int lastActor = context.GameState.LastActor;
            int round = context.GameState.Round;

            while (!context.GameState.IsGameOver && context.GameState.IsDealerActing)
            {
                List<Ak> kinds = context.GameState.GetAllowedActions(gd);
                foreach(Ak kind in kinds)
                {
                    PokerAction action = new PokerAction 
                    { 
                        Kind = kind, 
                        Position = context.GameState.CurrentActor
                    };
                    context.GameState.UpdateByAction(action, gd);
                }
            }

            if (tree != null)
            {
                tree.SetDepth(nodeId, (byte)context.Depth);
                tree.Nodes[nodeId].Position = lastActor;
                tree.Nodes[nodeId].Amount = context.Amount;
                tree.Nodes[nodeId].ActivePlayers = context.GameState.GetActivePlayers();
                tree.Nodes[nodeId].Round = round;
            }

            if (context.GameState.IsGameOver)
            {
                return;
            }

            // Deal next cards.
            List<Ak> actions = context.GameState.GetAllowedActions(gd);
            foreach (Ak actionKind in actions)
            {
                GameContext childContext = new GameContext(context);
                childContext.Depth++;
                int position = context.GameState.CurrentActor;
                PokerAction a = new PokerAction(actionKind, position, 0, "");
                if (a.Kind == Ak.r)
                {
                    a.Amount = gd.BetStructure[context.GameState.Round];
                }
                childContext.GameState.UpdateByAction(a, gd);
                childContext.Amount = childContext.GameState.Players[position].InPot -
                    context.GameState.Players[position].InPot;
                ProcessGameContext(childContext);
                context.ChildCount += childContext.ChildCount + 1;
            }
        }
    }
}
