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
    /// Create chance tree based on the game definition. Decks with duplicates are supported.
    /// The algo is written for small model decks.
    /// </summary>
    public static unsafe class CreateChanceTreeByGameDef
    {
        public static ChanceTree Create(GameDefinition gd)
        {
            // First pass - count nodes.
            GlobalContext gc = new GlobalContext { GameDef = gd };
            GameContext root = new GameContext { GameState = new GameState(gd), Global  = gc};
            ProcessGameContext(root);
            // Create the tree.
            int nodeCount = root.ChildCount + 1;
            ChanceTree ct = new ChanceTree(nodeCount);
            // Clear memory to ensure stable values in unused fields.
            ct.SetNodesMemory(0);
            // Second pass - initialize nodes.
            gc.Tree = ct;
            gc.NodeId = 0;
            gc.TotalCombCountOfLeaves = root.CombCountOfLeaves;
            root.CombCountOfLeaves = 0;
            ProcessGameContext(root);

            ct.Version.Description = String.Format("Chance tree (gamedef: {0})", gd.Name);

            return ct;
        }

        /// <summary>
        /// Global context, such as current node id.
        /// </summary>
        class GlobalContext
        {
            public GameDefinition GameDef;
            public int NodeId;
            public ChanceTree Tree;

            public int TotalCombCountOfLeaves;

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
                DealtCards = other.DealtCards;
                Global = other.Global;
                Depth = other.Depth;
                DealCount = other.DealCount;
            }

            public GlobalContext Global;


            public GameState GameState;

            /// <summary>
            /// Cards dealt so far.
            /// </summary>
            public CardSet DealtCards;

            /// <summary>
            /// Deal for this game context.
            /// </summary>
            public DealT Deal;

            /// <summary>
            /// Number of ways the current cards can be dealt multiplied recursively
            /// by the corresponding numbers of all parents.
            /// </summary>
            public int DealCount = 1;

            /// <summary>
            /// Number of combinations cards can be dealt for all leaves 
            /// groing (also indirectly) from this node.
            /// </summary>
            public int CombCountOfLeaves;

            public int ChildCount;

            public int Depth;
        }

        class DealT
        {

            public DealT()
            {
            }

            public DealT(DealT other)
            {
                CardIdx = other.CardIdx;
                CardName = other.CardName;
                Count = other.Count;
                SharedCardsPlayer = other.SharedCardsPlayer;
            }

            /// <summary>
            /// Card index.
            /// </summary>
            public int CardIdx;

            /// <summary>
            /// Card name corresponding to CardIdx, is used both in code and for debugging.
            /// </summary>
            public string CardName;

            /// <summary>
            /// Number of ways this card can be dealt.
            /// </summary>
            public int Count;

            /// <summary>
            /// If shared cards are dealt, index of the player who receives them.
            /// Otherwise is set to -1.
            /// </summary>
            public int SharedCardsPlayer = -1;
        }

        private static void ProcessGameContext(GameContext context)
        {
            int nodeId = context.Global.NodeId++;
            GameDefinition gd = context.Global.GameDef;
            ChanceTree tree = context.Global.Tree;

            while (!context.GameState.IsGameOver && !context.GameState.IsDealerActing)
            {
                context.GameState.UpdateByAction(PokerAction.c(context.GameState.CurrentActor), gd);
            }

            if (tree != null)
            {
                tree.SetDepth(nodeId, (byte)context.Depth);
                int position = context.GameState.LastActor;
                if(context.Deal != null && context.Deal.SharedCardsPlayer != -1)
                {
                    position = context.Deal.SharedCardsPlayer;
                }
                if (nodeId == 0)
                {
                    position = gd.MinPlayers;
                }
                tree.Nodes[nodeId].Position = position;
                if (context.Deal == null)
                {
                    tree.Nodes[nodeId].Card = -1;
                }
                else
                {
                    // This double-conversion assures that for decks with duplicates
                    // the same index 
                    int stableCardIdx = gd.DeckDescr.GetIndex(gd.DeckDescr.CardNames[context.Deal.CardIdx]);
                    tree.Nodes[nodeId].Card = stableCardIdx;
                }
            }

            if (context.GameState.IsGameOver)
            {
                context.CombCountOfLeaves = context.DealCount;
                if (tree != null)
                {
                    tree.Nodes[nodeId].Probab = (double)context.CombCountOfLeaves / context.Global.TotalCombCountOfLeaves;
                    int playersCount = gd.MinPlayers;
                    uint[] ranks = new uint[playersCount];
                    int[][] hands = new int[playersCount][];
                    for (int p = 0; p < playersCount; ++p)
                    {
                        hands[p] = gd.DeckDescr.GetIndexes(context.GameState.Players[p].Hand);
                    }
                    gd.GameRules.Showdown(gd, hands, ranks);
                    context.GameState.UpdateByShowdown(ranks, gd);
                    double[] potShares = new double[gd.MinPlayers];
                    for (int p = 0; p < playersCount; ++p)
                    {
                        potShares[p] = (context.GameState.Players[p].Result + context.GameState.Players[p].InPot)
                            / context.GameState.Pot;
                    }
                    tree.Nodes[nodeId].SetPotShare(context.GameState.GetActivePlayers(), potShares);
                }
                return;
            }

            // Deal next cards.
            List<DealT> deals = DealCards(context);
            foreach (DealT d in deals)
            {
                GameContext childContext = new GameContext(context);
                childContext.Depth++;
                
                childContext.DealtCards.UnionWith(gd.DeckDescr.CardSets[d.CardIdx]);
                if (d.SharedCardsPlayer == -1)
                {
                    // Non-shared deal - update game state and deal count
                    PokerAction a = PokerAction.d(context.GameState.CurrentActor, d.CardName);
                    childContext.GameState.UpdateByAction(a, gd);
                    childContext.DealCount *= d.Count;
                }
                else if(d.SharedCardsPlayer == gd.MinPlayers - 1)
                {
                    // Shared deal for last player - update game state and deal count once for all.
                    PokerAction a = PokerAction.d(-1, context.Deal.CardName);
                    childContext.GameState.UpdateByAction(a, gd);
                    childContext.DealCount *= d.Count;
                }
                childContext.Deal = d;
                ProcessGameContext(childContext);
                context.CombCountOfLeaves += childContext.CombCountOfLeaves;
                context.ChildCount += childContext.ChildCount + 1;
            }
            if (tree != null)
            {
                tree.Nodes[nodeId].Probab = (double)context.CombCountOfLeaves / context.Global.TotalCombCountOfLeaves;
            }
        }

        static List<DealT> DealCards(GameContext context)
        {
            GameDefinition gd = context.Global.GameDef;
            List<DealT> deals = new List<DealT>();

            // If this a shared card - duplicate the deal for all players
            if (context.Deal != null && context.Deal.SharedCardsPlayer != -1)
            {
                if (context.Deal.SharedCardsPlayer < gd.MinPlayers - 1)
                {
                    DealT copyDeal = new DealT(context.Deal);
                    deals.Add(copyDeal);
                    copyDeal.SharedCardsPlayer++;
                    return deals;
                }
            }

            int playerCardsCount = gd.PrivateCardsCount[context.GameState.NextDealRound] +
                gd.PublicCardsCount[context.GameState.NextDealRound];
            int sharedCardsCount = gd.SharedCardsCount[context.GameState.NextDealRound];

            if (playerCardsCount > 0 && sharedCardsCount > 0)
            {
                throw new ApplicationException("Only deals with either player or shared cards are supported now.");
            }
            int cardsCount = playerCardsCount + sharedCardsCount;
            if (cardsCount != 1)
            {
                throw new ApplicationException("Only one card in round is supported.");
            }

            EnumParams ep = new EnumParams { 
                GameDef = gd, 
                Deals = deals,
                SharedCardsPlayer = sharedCardsCount == 0 ? -1 : 0
            };
            CardEnum.Combin(gd.DeckDescr, cardsCount, CardSet.Empty, context.DealtCards, OnCombin, ep);
            return deals;
        }

        class EnumParams
        {
            public GameDefinition GameDef;
            public Dictionary<string, DealT> uniqueActions = new Dictionary<string, DealT>();
            public List<DealT> Deals;
            public int SharedCardsPlayer;
        }

        static void OnCombin(ref CardSet cards, EnumParams ep)
        {
            DealT deal;
            string cardName = ep.GameDef.DeckDescr.GetCardNames(cards);
            int cardId = ep.GameDef.DeckDescr.GetIndexesAscending(cards)[0];

            if (!ep.uniqueActions.TryGetValue(cardName, out deal))
            {
                // Add new action
                deal = new DealT { CardIdx = cardId, CardName = cardName, SharedCardsPlayer = ep.SharedCardsPlayer };
                ep.Deals.Add(deal);
                ep.uniqueActions[cardName] = deal;
            }
            deal.Count++;
        }
    }
}
