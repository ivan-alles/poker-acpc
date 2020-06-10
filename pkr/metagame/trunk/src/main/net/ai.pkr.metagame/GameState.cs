/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace ai.pkr.metagame
{
    /// <summary>
    /// The state of the game.
    /// 
    /// It is intended for the following use cases:
    /// 1. Playing a game with a given game definition, by both server and player.
    /// 2. Strategy development, e.g. this class can help building strategy trees and 
    ///    keep information about game state in a game tree node.
    /// 3. Analyzing game logs, even without knowing exact game definition 
    ///    (e.g. we can calculate the pot size and amount in pot of each player knowing the game string).
    ///    This class can analyze game strings that are measured in unnormalized units (b0 != 1).
    /// 
    /// Game state is a result of game actions, but it does not contain the history of actions.
    /// If you need this history, keep it elsewhere.
    /// 
    /// Game state can update itself by actions of the player or dealer, 
    /// but it does not validate the actions (e.g. raise limit for the round expired).
    /// It it the task of the dealer (server).
    /// </summary>
    [Serializable]
    public class GameState
    {
        #region Constants

        public const int DealerPosition = -1;
        public const int InvalidPosition = int.MinValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor (e.g. for serialization).
        /// </summary>
        public GameState()
        {
        }

        /// <summary>
        /// Create from game string. 
        /// GameDefinition is optional and allows to set some properties (e.g. CurrentActor) correctly.
        /// </summary>
        public GameState(string gameString, GameDefinition gameDef)
        {
            string error;
            GameRecord gameRecord = GameRecord.Parse(gameString, out error);
            if(gameRecord == null)
                throw new ApplicationException("Cannot create GameState: " + error);
            InitializeFromGameRecord(gameRecord, gameDef);
        }

        /// <summary>
        /// Create from game record. 
        /// GameDefinition is optional and allows to set some properties (e.g. CurrentActor) correctly.
        /// </summary>
        public GameState(GameRecord gameRecord, GameDefinition gameDef)
        {
            InitializeFromGameRecord(gameRecord, gameDef);
        }

        /// <summary>
        /// Create a new game state base on game definition. 
        /// Stacks of the players are set to 0, names to "".
        /// Puts blinds automatically (so that players will have negative stacks). 
        /// </summary>
        public GameState(GameDefinition gameDef, int playersCount)
        {
            Initialize(gameDef, playersCount);
            PutBlinds(gameDef);
            SetNextActor(gameDef);
        }

        /// <summary>
        /// Create a new game state base on game definition, players count is set to gameDef.MinPlayers. 
        /// Stacks of the players are set to 0, names to "".
        /// Puts blinds automatically (so that players will have negative stacks). 
        /// </summary>
        public GameState(GameDefinition gameDef): this(gameDef, gameDef.MinPlayers)
        {
        }

        public GameState(GameState other)
        {
            Id = other.Id;
            IsGameOver = other.IsGameOver;
            Round = other.Round;
            Pot = other.Pot;
            Bet = other.Bet;
            BetCount = other.BetCount;
            LastActor = other.LastActor;
            IsDealerActing = other.IsDealerActing;
            CurrentActor = other.CurrentActor;
            DealsCount = other.DealsCount;
            FoldedCount = other.FoldedCount;

            Players = new PlayerState[other.Players.Length];
            for (int p = 0; p < other.Players.Length; ++p)
            {
                Players[p] = new PlayerState(other.Players[p]);
            }
        }


        #endregion

        #region Properties

        public string Id
        {
            set;
            get;
        }

        public bool IsGameOver
        {
            get;
            set;
        }

        /// <summary>
        /// Current round [0..gameDef.RoundsCount), e.g. for standard HE [0..4).
        /// At the beginning is -1, and is incremented with the first dealer action in a row.
        /// </summary>
        public int Round
        {
            set;
            get;
        }

        /// <summary>
        /// Current round index for next deal. Differs from current round just before the first deal in a round.
        /// </summary>
        public int NextDealRound
        {
            get 
            {
                return DealsCount == 0 ? Round + 1 : Round;
            }
        }

        public double Pot
        {
            set;
            get;
        }

        /// <summary>
        /// Current bet size. If there are non-zero blinds/antes, they count for one bet.
        /// </summary>
        public double Bet
        {
            set;
            get;
        }

        /// <summary>
        /// Number of bets (raises) for the current round.
        /// If there are non-zero blinds/antes, they count for one bet.
        /// </summary>
        public int BetCount
        {
            set;
            get;
        }

        /// <summary>
        /// Postion of the player who acted last for actions r, c, f, d, p or DealerPosition for s.
        /// At the beginning of the game is set to InvalidPosition. 
        ///<para>See also <seealso cref="CurrentActor"/>, <seealso cref="HasPlayerActed"/>.</para>
        /// </summary>
        public int LastActor
        {
            set;
            get;
        }

        /// <summary>
        /// If current action is a deal action (d, p or s). 
        /// Influences the meaning of the value of CurrentActor.
        /// If game definition is not used, is always false.
        /// </summary>
        public bool IsDealerActing
        {
            set;
            get;
        }

        /// <summary>
        /// Position of the player who is acting for actions r, c, f, d, p or DealerPosition for s.
        /// If the dealer is acting (IsDealerActing) for d or p, returns the position of the player 
        /// who receives the cards.
        /// <para>If game definition is not used, is set to InvalidPosition.</para>
        /// <remarks>This two-fold semantics (position of player being dealt or position of player acting)
        /// is chosen because this is the same semantic as in game logs, PokerAction class and LastActor property.
        /// The same semantics is used to visualize poker trees too.</remarks>
        /// <para>See also <seealso cref="IsPlayerActing"/>.</para>
        /// </summary>
        public int CurrentActor
        {
            set;
            get;
        }

        /// <summary>
        /// Number of deals in a row in the current round. Reset to 0 after any player action.
        /// </summary>
        public int DealsCount
        {
            set;
            get;
        }

        public int FoldedCount
        {
            set;
            get;
        }

        /// <summary>
        /// If game is over, shows if showdown is required (at least 2 players are contesting the pot). 
        /// If game is not over, returns false.
        /// The state can be updated by showdown results by calling UpdateByShowdown().
        /// </summary>
        [XmlIgnore]
        public bool IsShowdownRequired
        {
            get
            {
                return IsGameOver && FoldedCount < Players.Length - 1;
            }
        }

        public PlayerState[] Players
        {
            get;
            set;
        }

        #endregion

        #region Methods
        public void UpdateByAction(PokerAction action, GameDefinition gameDef)
        {
            if (action.Kind == Ak.b)
            {
                // Ignore this, can occur in strategy algorithms in the beginning of the game.
                return;
            }

            Debug.Assert(action.Position <= Players.Length);

            LastActor = action.Position;
            switch(action.Kind)
            {
                case Ak.d:
                    if (DealsCount == 0)
                    {
                        OnFirstDeal(gameDef);
                    }
                    DealsCount++;
                    if (action.Position >= 0)
                    {
                        Players[action.Position].Hand = AddCards(Players[action.Position].Hand, action.Cards);
                    }
                    else
                    {
                        // Shared cards
                        foreach (PlayerState player in Players)
                        {
                            if (!player.IsFolded)
                            {
                                player.Hand = AddCards(player.Hand, action.Cards);
                            }
                        }
                        LastActor = GameState.DealerPosition;
                    }
                    break;
                case Ak.f:
                    ProcessFold(action.Position);
                    break;
                case Ak.c:
                    ProcessCall(action.Position, action.Amount, gameDef);
                    break;
                case Ak.r:
                    ProcessRaise(action.Position, action.Amount);
                    break;
                default:
                    throw new ApplicationException("Unknown action: " + action.ToGameString());
            }
            SetNextActor(gameDef);
        }

        public PlayerState FindPlayerByName(string name)
        {
            for(int p = 0; p < Players.Length; ++p)
            {
                if (Players[p].Name == name)
                    return Players[p];
            }
            return null;
        }

        public int FindPositionByName(string name)
        {
            for (int p = 0; p < Players.Length; ++p)
            {
                if (Players[p].Name == name)
                    return p;
            }
            return InvalidPosition;
        }

        public double GetMinBet(GameDefinition gameDef)
        {
            // Todo: implement for other limit kinds
            return gameDef.BetStructure[Round];
        }

        public double GetMaxBet(GameDefinition gameDef)
        {
            // Todo: implement for other limit kinds
            return gameDef.BetStructure[Round];
        }

        /// <summary>
        /// List of possible actions from current position.
        /// <para>
        /// If the dealer acts, by convention the order of deals is: d, p, s. </para>
        /// <para>
        /// If the player acts, by convention the order of actions is: f, c, r. </para>
        /// The list will contain only one of these actions at a time. If an action is not available, it is skipped, and 
        /// the order of other actions is preserved. For example, if no fold is possible, the list is "c, r". 
        /// </summary>
        // Todo: decide what to do with raise in no-limit, there are multiple raises possible.
        public List<Ak> GetAllowedActions(GameDefinition gameDef)
        {
            List<Ak> result = new List<Ak>();

            if (IsGameOver)
                return result;

            if (IsDealerActing)
            {
                int activePlayers = 0;
                for (int p = 0; p < Players.Length; ++p)
                {
                    if (Players[p].CanActInGame)
                        activePlayers++;
                }
                int round = NextDealRound;
                int deals = gameDef.PrivateCardsCount[round] > 0 ? activePlayers : 0;
                bool cardsToDealExist = false;
                if (DealsCount < deals)
                {
                    cardsToDealExist = true;
                }
                else
                {
                    deals += gameDef.PublicCardsCount[round] > 0 ? activePlayers : 0;
                    if (DealsCount < deals)
                    {
                        cardsToDealExist = true;
                    }
                    else
                    {
                        deals += gameDef.SharedCardsCount[round] > 0 ? 1 : 0;
                        if (DealsCount < deals)
                        {
                            cardsToDealExist = true;
                        }
                    }
                }
                if (cardsToDealExist)
                {
                    result.Add(Ak.d);
                }
            }
            else
            {
                if (Players[CurrentActor].Bet < Bet) // Otherwise there is no sense to fold anyway.
                    result.Add(Ak.f);

                if (Players[CurrentActor].CanActInCurrentRound)
                {
                    result.Add(Ak.c);
                    if(BetCount < gameDef.BetsCountLimits[Round])
                    {
                        result.Add(Ak.r);
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Returns true if the current actor is the player at the given position (but not the dealer).
        /// </summary>
        public bool IsPlayerActing(int pos)
        {
            return !IsDealerActing && CurrentActor == pos;
        }

        /// <summary>
        /// Returns true if the last actor was the player at the given position (but not the dealer).
        /// </summary>
        public bool HasPlayerActed(int pos)
        {
            return DealsCount == 0 && LastActor == pos;
        }

        /// <summary>
        /// Returns a bitmask, where each bit corresponds to a player position. A bit is set 
        /// if the player is contesting the pot (non-folder).
        /// </summary>
        /// <returns></returns>
        public UInt16 GetActivePlayers()
        {
            UInt16 mask = 0;
            for (int p = 0; p < Players.Length; ++p)
            {
                mask |= (UInt16)((Players[p].IsFolded ? 0 : 1) << p);
            }
            return mask;
        }

        /// <summary>
        /// Updates player results and stacks by showdown result.
        /// gameDef is needed to determine the pot splitting rules (hi, hi/lo).
        /// </summary>
        public void UpdateByShowdown(UInt32[] ranks, GameDefinition gameDef)
        {
            UInt32[] ranksCopy = new UInt32[ranks.Length];
            double[] inpot = new double[Players.Length];
            double[] result = new double[Players.Length];
            for (int p = 0; p < Players.Length; ++p)
            {
                ranksCopy[p] = ranks[p];
                inpot[p] = Players[p].InPot;
            }
            Showdown.CalcualteHi(inpot, ranksCopy, result, 0);
            for (int p = 0; p < Players.Length; ++p)
            {
                Players[p].Result = result[p];
                Players[p].Stack += result[p] + Players[p].InPot;
            }
        }

        /// <summary>
        /// If at the end of the game an active player has an unmatched bet, it will be removed from Pot and player's InPot.
        /// </summary>
        public void RemoveUnmatchedBet()
        {
            if(!IsGameOver)
            {
                throw new ApplicationException("Game must be over");
            }
            int topInPotPlayerIdx = -1;
            double topInPot = double.MinValue;
            for (int p = 0; p < Players.Length; ++p)
            {
                if (Players[p].InPot > topInPot)
                {
                    topInPotPlayerIdx = p;
                    topInPot = Players[p].InPot;
                }
            }
            Debug.Assert(!Players[topInPotPlayerIdx].IsFolded);
            double secondLargestInPot = double.MinValue;
            for (int p = 0; p < Players.Length; ++p)
            {
                if (Players[p].InPot > secondLargestInPot && p != topInPotPlayerIdx)
                {
                    secondLargestInPot = Players[p].InPot;
                }
            }
            // Return unmatched bet
            double unmatched = topInPot - secondLargestInPot;
            Players[topInPotPlayerIdx].InPot -= unmatched;
            Pot -= unmatched;
        }

        public override string ToString()
        {
            return String.Format("R: {0} P: {1}{2}", Round, Pot, IsGameOver ? "." : ";");
        }
        
        public override bool Equals(object obj)
        {
            GameState o = obj as GameState;
            return Equals(o);
        }

        public bool Equals(GameState o)
        {
            if (o == null)
                return false;
            bool own =
               Id == o.Id &&
               IsGameOver == o.IsGameOver &&
               Round == o.Round &&
               Pot == o.Pot &&
               Bet == o.Bet &&
               BetCount == o.BetCount &&
               LastActor == o.LastActor &&
               IsDealerActing == o.IsDealerActing &&
               CurrentActor == o.CurrentActor &&
               DealsCount == o.DealsCount &&
               FoldedCount == o.FoldedCount &&
               Players.Length == o.Players.Length;
            if(!own)
                return false;
            for (int p = 0; p < Players.Length; ++p)
            {
                if(!Players[p].Equals(o.Players[p]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Players.Length ^ (int) Pot ^ Round ^ BetCount ^ (int) Bet ^ (LastActor + 1);
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Basic initialization, before game starts and before the blinds are posted.
        /// </summary>
        private void Initialize(GameDefinition gameDef, int playersCount)
        {
            Players = new PlayerState[playersCount];
            for (int p = 0; p < Players.Length; ++p)
            {
                Players[p] = new PlayerState();
                Players[p].CanActInCurrentRound = false;
            }
            Id = "";
            Round = -1; 
            LastActor = InvalidPosition;
            // Do not set BetCount, either it is set by PutBlinds() or in InitializeFromGameRecord()
        }

        private void InitializeFromGameRecord(GameRecord gameRecord, GameDefinition gameDef)
        {
            Initialize(gameDef, gameRecord.Players.Count);

            Id = gameRecord.Id;

            Bet = 0;
            for (int p = 0; p < Players.Length; ++p)
            {
                Players[p].Name = gameRecord.Players[p].Name;
                Players[p].Stack = gameRecord.Players[p].Stack;
                Players[p].PutAmountInPot(gameRecord.Players[p].Blind);
                Pot += gameRecord.Players[p].Blind;
                if (Bet < gameRecord.Players[p].Blind)
                    Bet = gameRecord.Players[p].Blind;
            }

            if (Bet > 0)
            {
                BetCount = 1;
            }


            // Set initial actor 
            CurrentActor = DealerPosition;
            IsDealerActing = true;

            for (int a = 0; a < gameRecord.Actions.Count; ++a)
            {
                UpdateByAction(gameRecord.Actions[a], gameDef);
            }

            IsGameOver = gameRecord.IsGameOver;

            for (int p = 0; p < Players.Length; ++p)
            {
                Players[p].Result = gameRecord.Players[p].Result;
            }

            if (IsGameOver)
            {
                for (int p = 0; p < Players.Length; ++p)
                {
                    Players[p].Stack = gameRecord.Players[p].Stack + gameRecord.Players[p].Result;
                }
            }
        }

        private void ProcessRaise(int position, double amount)
        {
            Debug.Assert(position >= 0);
            DealsCount = 0;
            Bet += amount;
            Pot += Players[position].ReachAmountInPot(Bet);
            BetCount++;
            Players[position].CanActInCurrentRound = false;
            for(int i = 1; i < Players.Length; ++i)
            {
                Players[(position + i) % Players.Length].CanActInCurrentRound = true;
            }
        }

        private void ProcessCall(int position, double amount, GameDefinition gameDef)
        {
            Debug.Assert(amount <= 0, "Amount of call cannot be positive");
            DealsCount = 0;
            double toReach = Bet + amount;
            Pot += Players[position].ReachAmountInPot(toReach);
            Players[position].CanActInCurrentRound = false;
        }

        private void ProcessFold(int position)
        {
            DealsCount = 0;
            if (!Players[position].IsFolded)
            {
                Players[position].IsFolded = true;
                FoldedCount++;
                if(FoldedCount == Players.Length - 1)
                {
                    IsGameOver = true;
                    double winnerResult = 0;
                    int winnerPos = InvalidPosition;
                    for(int p = 0; p < Players.Length; ++p)
                    {
                        if(!Players[p].IsFolded)
                        {
                            winnerPos = p;
                        }
                        else
                        {
                            winnerResult += Players[p].InPot;
                            Players[p].Result = -Players[p].InPot;
                        }
                    }
                    Players[winnerPos].Result = winnerResult;
                    Players[winnerPos].Stack += winnerResult + Players[winnerPos].InPot;
                }
            }
        }

        private void StartNextRound(GameDefinition gameDef)
        {
            Round++;
            // Do not clear blinds.
            if (Round > 0)
            {
                Bet = 0;
                BetCount = 0;
                foreach (PlayerState p in Players)
                {
                    p.Bet = 0;
                }
            }
        }

        private void OnFirstDeal(GameDefinition gameDef)
        {
            StartNextRound(gameDef);
            foreach (PlayerState p in Players)
            {
                p.CanActInCurrentRound = true;
            }
        }

        /// <summary>
        /// Set next actor.
        /// </summary>
        private void SetNextActor(GameDefinition gameDef)
        {
            if (IsGameOver)
            {
                CurrentActor = InvalidPosition;
                return;
            }

            // Reset dealer here once, will be set to true later if necessary.
            IsDealerActing = false;

            int activePlayers = 0;
            int activePlayersInRound = 0;
            for (int p = 0; p < Players.Length; ++p)
            {
                if (Players[p].CanActInGame)
                    activePlayers++;
                if (Players[p].CanActInCurrentRound)
                    activePlayersInRound++;
            }

            if (activePlayersInRound == 0 && DealsCount == 0)
            {
                // All players have acted, but the dealer have not yet - begin of new round.
                if (gameDef != null && gameDef.RoundsCount == Round + 1)
                {
                    IsGameOver = true;
                    CurrentActor = InvalidPosition;
                    return;
                }
                if (gameDef == null)
                {
                    // All player have acted -> dealer is acting next.
                    IsDealerActing = true;
                    // We do not know the player to receive cards (can be a player or -1).
                    CurrentActor = InvalidPosition;
                    return;
                }
            }

            if (gameDef == null)
            {
                // As we do not always know the number of players without gamedef, just
                // set idicate that no info is available and return.
                CurrentActor = InvalidPosition;
                return;
            }

            // If all players have acted or dealer is acting, first see,
            // if there are more cards to deal.
            if (DealsCount > 0 || activePlayersInRound == 0)
            {
                int dealRound = NextDealRound;
                int deals = gameDef.PrivateCardsCount[dealRound] > 0 ? activePlayers : 0;
                if (DealsCount < deals)
                {
                    // Next action: d
                    FindNextPlayerToDeal();
                    return;
                }
                else
                {
                    deals += gameDef.PublicCardsCount[dealRound] > 0 ? activePlayers : 0;
                    if (DealsCount < deals)
                    {
                        // Next action: p
                        FindNextPlayerToDeal();
                        return;
                    }
                    else
                    {
                        deals += gameDef.SharedCardsCount[dealRound] > 0 ? 1 : 0;
                        if (DealsCount < deals)
                        {
                            // Next action: s
                            CurrentActor = GameState.DealerPosition;
                            IsDealerActing = true;
                            return;
                        }
                    }
                }
            }

            // Otherwise a player is acting.

            // DealsCount is reset after 1st player action.
            Debug.Assert(DealsCount > 0 || LastActor >= 0);
            int startActor = DealsCount > 0 ? gameDef.GetFirstActor(Round, Players.Length) : LastActor + 1;

            for(int p = 0; p < Players.Length; ++p)
            {
                int actor = (p + startActor) % Players.Length;
                if (Players[actor].CanActInCurrentRound)
                {
                    CurrentActor = actor;
                    return;
                }
            }
            Debug.Assert(false); // Should never come here.
            CurrentActor = InvalidPosition;
        }

        private void FindNextPlayerToDeal()
        {
            int count = 0;
            for (int p = 0; p < Players.Length; ++p)
            {
                if (Players[p].CanActInGame)
                {
                    if (++count == DealsCount + 1)
                    {
                        IsDealerActing = true;
                        CurrentActor = p;
                        return;
                    }
                }
            }
        }


        private void PutBlinds(GameDefinition gameDef)
        {
            for (int pos = 0; pos < Players.Length; ++pos)
            {
                Players[pos].PutAmountInPot(gameDef.BlindStructure[pos]);
                Pot += gameDef.BlindStructure[pos];
                Bet = Math.Max(Players[pos].InPot, Bet);
            }
            BetCount = gameDef.GetBlindsBetsCount();
        }

        string AddCards(string origCards, string newCards)
        {
            if (string.IsNullOrEmpty(origCards))
                return newCards;
            return origCards + " " + newCards;
        }


        #endregion

    }
}