/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metabots;
using ai.pkr.metabots.bots;
using System.Text.RegularExpressions;
using ai.lib.utils;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.pkr.metastrategy;
using ai.lib.algorithms.tree;
using System.IO;
using ai.lib.algorithms.random;
using System.Reflection;
using ai.lib.algorithms.numbers;
using System.Globalization;


namespace ai.pkr.bots.patience
{
    /// <summary>
    /// An HE.FL bot playing a static strategy (eq, br, whatever).
    /// <para>Parameters:</para>
    /// <para>StrategyDir (string, required): directory containing files with information about strategy.</para>
    /// <para>RngSeed (string, optional, default: 0): RNG seed (0-use time-based).</para>
    /// <para>CheckBlinds (bool, optional, default: true): check if blinds of the strategy match the blinds of the game.</para>
    /// <para>AmountSearchMethod (enum, optional, default: Equal): method to compare action amount to find a move in the strategy tree 
    /// by a given poker action. Possible values: Equal, Closest</para>
    /// <para>RelProbabIgnoreLevel (double, optional, default: 0.0): ignores moves with probability less than this level. 
    /// This can be used to avoid folds with good cards due to the problems with fictitios play algorithm.</para>
    /// </summary>
    public unsafe class Patience : IPlayer
    {
        #region IPlayer Members

        public void OnCreate(string name, Props creationParameters)
        {
            _name = name;
            log.InfoFormat("{0} OnCreate()", _name);
            _creationParams = creationParameters;
            Initialize();
        }

        public void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters)
        {
            // Ignore gameDef, take it from configuration.
            // This is also ok for AIII.
        }

        public void OnSessionEvent(Props parameters)
        {
        }

        public PlayerInfo OnServerConnect()
        {
            PlayerInfo pi = new PlayerInfo(_name);
            pi.Version = new BdsVersion(Assembly.GetExecutingAssembly());

            pi.Properties = _playerInfoProps;
            return pi;
        }

        public void OnServerDisconnect(string reason)
        {
        }

        public void OnSessionEnd()
        {
        }

        public virtual void OnGameBegin(string gameString)
        {
            _gameRecord = new GameRecord(gameString);
            _pos = _gameRecord.FindPositionByName(_name);
            if (_checkBlinds)
            {
                for (int p = 0; p < _gameRecord.Players.Count; ++p)
                {
                    _curStrNodeIdx = p + 1;
                    StrategyTreeNode stNode = new StrategyTreeNode();
                    _strategies[_pos].GetNode(p + 1, &stNode);
                    double strBlind = stNode.Amount;
                    double grBlind = _gameRecord.Players[p].Blind;
                    if (!FloatingPoint.AreEqual(grBlind, strBlind, AMOUNT_EPSILON))
                    {
                        throw new ApplicationException(
                            String.Format("{0}: blind size mismatch: game: {1}, strategy: {2}",
                                          GetBotStateDiagText(), grBlind, strBlind));
                    }
                }
            }
            if (_gameRecord.Actions.Count != 0)
            {
                throw new ApplicationException(
                    String.Format("{0}: action count must be zero in OnGameBegin, game string '{1}'",
                                  GetBotStateDiagText(), gameString));
            }
            _curStrNodeIdx = _gameRecord.Players.Count;
            _processedActionsCount = 0;
            _lastAbsStrProbab = 1.0;
            _gameState = new GameState(_gameRecord, null);

            log.InfoFormat("{0} OnGameBegin() pos {1}", _name, _pos);
        }

        public void OnGameUpdate(string gameString)
        {
        }

        public virtual PokerAction OnActionRequired(string gameString)
        {
            log.InfoFormat("{0} OnActionRequired", _name);
            _gameRecord = new GameRecord(gameString);
            ProcessActions();

            int chBegin, movesCount;
            _strIndexes[_pos].GetChildrenBeginIdxAndCount(_curStrNodeIdx, out chBegin, out movesCount);
            double[] probabs = new double[movesCount];
            double sumProbab = 0;
            for (int c = 0; c < movesCount; ++c)
            {
                int stNodeIdx = _strIndexes[_pos].GetChildIdx(chBegin + c);
                StrategyTreeNode stNode = new StrategyTreeNode();
                _strategies[_pos].GetNode(stNodeIdx, &stNode);
                // Verify we are in the correct position.
                // This is proven to be very helpful in testing.
                if (!stNode.IsPlayerAction(_pos))
                {
                    throw new ApplicationException(String.Format("{0}: expected strategy child: player action for pos {1}, but was: '{2}'",
                        GetBotStateDiagText(),
                        _pos,
                        stNode.ToStrategicString(null)));
                }

                probabs[c] = stNode.Probab;
                sumProbab += probabs[c];
            }
            try
            {
                // Convert to relative probability and ignore if necessary
                for (int c = 0; c < probabs.Length; ++c)
                {
                    probabs[c] /= sumProbab;
                    if (probabs[c] < _relProbabIgnoreLevel)
                    {
                        probabs[c] = 0;
                    }
                }
                _moveSelector.SetWeights(probabs);
            }
            catch (Exception e)
            {
                // If there are problems with 0-probablities, add more info to it.
                throw new ApplicationException(String.Format("{0} : see inner exception", GetBotStateDiagText()), e);
            }

            int moveIdx = _moveSelector.Next();
            Debug.Assert(probabs[moveIdx] > 0, "0-probabs must not occur");
            int nextStrNode = _strIndexes[_pos].GetChildIdx(chBegin + moveIdx);
            PokerAction move = ConvertStrActionToPokerAction(nextStrNode);

            log.InfoFormat("{0} OnActionRequired() returns {1}", _name, move);
            return move;
        }

        public virtual void OnGameEnd(string gameString)
        {
        }

        #endregion


        #region Diagnostics

        /// <summary>
        /// Returns a descriptive text describing the current state of the bot. 
        /// It contains important parameters that are essential to understand the context of almost every error.
        /// It should be use in exceptions and error traces.
        /// </summary>
        /// <returns></returns>
        string GetBotStateDiagText()
        {
            StrategyTreeNode stNode = new StrategyTreeNode();
            _strategies[_pos].GetNode(_curStrNodeIdx, &stNode);
            return String.Format("Player: {0}, pos: {1}, str. node: {2}({3})",
                _name, _pos, _curStrNodeIdx, stNode.ToStrategicString(null));
        }

        //string GetTraceNameBase(GameState CurGameState)
        //{
        //    string name = string.Format("patience-{0}-{1}-{2:00}-",
        //        CurGameState.Id, CurGameState.Round, _traceCount++);
        //    return Path.Combine(_traceDir, name);
        //}

        #endregion

        #region Implementation

        static Patience()
        {
            _traceDir = Props.Global.Expand("${bds.TraceDir}Patience");

            try
            {
                if (!Directory.Exists(_traceDir))
                    Directory.CreateDirectory(_traceDir);
            }
            catch
            {
                // Ignore errors here (was on ACPC once).
            }
        }

        void Initialize()
        {
            _playerInfoProps = new Props();
            string strDir = _creationParams.Get("StrategyDir");
            _playerInfoProps.Set("StrategyDir", strDir);
            string propsFile = Path.Combine(strDir, "props.xml");
            Props props = XmlSerializerExt.Deserialize<Props>(propsFile);

            int rngSeed = int.Parse(_creationParams.GetDefault("RngSeed", "0"));
            if (rngSeed == 0)
            {
                rngSeed = (int)DateTime.Now.Ticks;
            }

            _checkBlinds = bool.Parse(_creationParams.GetDefault("CheckBlinds", "true"));

            string amountCompareMethodText =  _creationParams.GetDefault("AmountSearchMethod", "Equal");
            if(amountCompareMethodText == "Equal")
            {
                _amountSearchMethod = AmountSearchMethod.Equal;
            }
            else if(amountCompareMethodText == "Closest")
            {
                _amountSearchMethod = AmountSearchMethod.Closest;
            }
            else
            {
                throw new ApplicationException(string.Format("Unknown amount compare method {0}", amountCompareMethodText));
            }

            _relProbabIgnoreLevel = double.Parse(_creationParams.GetDefault("RelProbabIgnoreLevel", "0.0"), CultureInfo.InvariantCulture);
            _playerInfoProps.Set("RelProbabIgnoreLevel", _relProbabIgnoreLevel.ToString());

            // Use MersenneTwister because it is under our control on all platforms and 
            // is probably better than System.Random.
            Random underlyingRng = new MersenneTwister(rngSeed);
            _moveSelector = new DiscreteProbabilityRng(underlyingRng);
            _playerInfoProps.Set("RngSeed", rngSeed.ToString());
            
            _deckDescr = XmlSerializerExt.Deserialize<DeckDescriptor>(props.Get("DeckDescriptor"));

            _chanceAbsrtractions = new IChanceAbstraction[0];

            // Create chance abstractions
            for (int pos = 0; ; pos++)
            {
                string caPropName = String.Format("ChanceAbstraction-{0}", pos);
                string relFileName = props.Get(caPropName);
                if (string.IsNullOrEmpty(relFileName))
                {
                    break;
                }
                Array.Resize(ref _chanceAbsrtractions, _chanceAbsrtractions.Length + 1);

                string absFileName = Path.Combine(strDir, relFileName);
                _chanceAbsrtractions[pos] = ChanceAbstractionHelper.CreateFromPropsFile(absFileName);
                _playerInfoProps.Set(caPropName + ".Name", _chanceAbsrtractions[pos].Name);
            }

            Dictionary<string, int> fileToPos = new Dictionary<string, int>();

            _strategies = new StrategyTree[0];
            _strIndexes = new UFTreeChildrenIndex[0];

            // Load strategies, reuse if file is the same
            for (int pos = 0; ; pos++)
            {
                string strPropName = String.Format("Strategy-{0}", pos);
                string relFileName = props.Get(strPropName);
                if (string.IsNullOrEmpty(relFileName))
                {
                    break;
                }
                Array.Resize(ref _strategies, _strategies.Length + 1);
                Array.Resize(ref _strIndexes, _strIndexes.Length + 1);

                string absFileName = Path.Combine(strDir, relFileName);
                int existingPos;
                if (fileToPos.TryGetValue(absFileName, out existingPos))
                {
                    _strategies[pos] = _strategies[existingPos];
                    _strIndexes[pos] = _strIndexes[existingPos];
                }
                else
                {
                    fileToPos.Add(absFileName, pos);
                    _strategies[pos] = StrategyTree.ReadFDA<StrategyTree>(absFileName);
                    _strIndexes[pos] = new UFTreeChildrenIndex(_strategies[pos], Path.Combine(strDir, "strategy-idx.dat"), false);
                }
                _playerInfoProps.Set(strPropName+".Version", _strategies[pos].Version.ToString());
            }

            // Read blinds
            for (int strPos = 0; strPos < _strategies.Length; ++strPos)
            {
                StringBuilder sb = new StringBuilder();
                for (int playerPos = 0; playerPos < _strategies.Length; ++playerPos)
                {
                    StrategyTreeNode stNode = new StrategyTreeNode();
                    _strategies[strPos].GetNode(playerPos + 1, &stNode);
                    sb.AppendFormat("{0:0.000000 }", stNode.Amount);
                }
                _playerInfoProps.Set("Blinds." + strPos.ToString(), sb.ToString());
            }
        }

        /// <summary>
        /// Process new poker actions received from the server.
        /// </summary>
        private void ProcessActions()
        {
            for (int a = _processedActionsCount; a < _gameRecord.Actions.Count; ++a, _processedActionsCount++)
            {
                PokerAction pa = _gameRecord.Actions[a];
                log.InfoFormat("{0} process {1} of pos {2}", _name, pa.Kind, pa.Position);
                ProcessAction(pa);
                StrategyTreeNode stNode = new StrategyTreeNode();
                _strategies[_pos].GetNode(_curStrNodeIdx, &stNode);
                if (stNode.IsPlayerAction(_pos))
                {
                    _lastAbsStrProbab = stNode.Probab;
                }
            }
        }

        /// <summary>
        /// Processes each poker action received from the server.
        /// Updates the game state and moves to the next node in our strategy.
        /// </summary>
        /// <param name="pa"></param>
        void ProcessAction(PokerAction pa)
        {
            int chBegin, chCount;
            _strIndexes[_pos].GetChildrenBeginIdxAndCount(_curStrNodeIdx, out chBegin, out chCount);
            int nextStrNodeIdx = -1;

            if (pa.IsDealerAction())
            {
                _gameState.UpdateByAction(pa, null);

                if (pa.Position >= 0 && pa.Position != _pos)
                {
                    // This is a deal to an opponent
                    // We can skip it for games without public cards (and it is very unlikely we will have some 
                    // in the future).
                    return;
                }

                int[] hand = _deckDescr.GetIndexes(_gameState.Players[_pos].Hand);
                int abstrCard = _chanceAbsrtractions[_pos].GetAbstractCard(hand, hand.Length);
                for (int c = 0; c < chCount; ++c)
                {
                    int stNodeIdx = _strIndexes[_pos].GetChildIdx(chBegin + c);
                    StrategyTreeNode stNode = new StrategyTreeNode();
                    _strategies[_pos].GetNode(stNodeIdx, &stNode);
                    if (!stNode.IsDealerAction)
                    {
                        throw new ApplicationException(
                            String.Format("{0} : expected strategy child: dealer action but was: '{1}'",
                                        GetBotStateDiagText(), stNode.ToStrategicString(null)));
                    }
                    if (stNode.Card == abstrCard)
                    {
                        nextStrNodeIdx = stNodeIdx;
                        goto searchFinished;
                    }
                }
            }
            else // Player action
            {
                double inPotBefore = _gameState.Players[pa.Position].InPot;
                _gameState.UpdateByAction(pa, null);
                double inPotAfter = _gameState.Players[pa.Position].InPot;
                double actualAmount = inPotAfter - inPotBefore;
                double bestAmount = double.MinValue;

                for (int c = 0; c < chCount; ++c)
                {
                    int stNodeIdx = _strIndexes[_pos].GetChildIdx(chBegin + c);
                    StrategyTreeNode stNode = new StrategyTreeNode();
                    _strategies[_pos].GetNode(stNodeIdx, &stNode);

                    if (!stNode.IsPlayerAction(pa.Position))
                    {
                        throw new ApplicationException(
                            String.Format("{0} : expected strategy child: player action pos {1} but was: '{2}'",
                            GetBotStateDiagText(), pa.Position, stNode.ToStrategicString(null)));
                    }
                    double amount = stNode.Amount;
                    switch (_amountSearchMethod)
                    {
                        case AmountSearchMethod.Equal:
                            if (FloatingPoint.AreEqual(amount, actualAmount, AMOUNT_EPSILON))
                            {
                                nextStrNodeIdx = stNodeIdx;
                                goto searchFinished;
                            }
                            break;
                        case AmountSearchMethod.Closest:
                            if (Math.Abs(amount - actualAmount) < Math.Abs(bestAmount - actualAmount))
                            {
                                bestAmount = amount;
                                nextStrNodeIdx = stNodeIdx;
                            }
                            break;
                    }
                }

            }
            searchFinished:
            if (nextStrNodeIdx == -1)
            {
                throw new ApplicationException(
                    String.Format("{0} : cannot find strategy action for poker action '{1}'",
                                  GetBotStateDiagText(), pa.ToGameString()));
            }
            _curStrNodeIdx = nextStrNodeIdx;
        }

        private PokerAction ConvertStrActionToPokerAction(int nextStNodeIdx)
        {
            StrategyTreeNode nextStNode = new StrategyTreeNode();
            _strategies[_pos].GetNode(nextStNodeIdx, &nextStNode);
            if (!nextStNode.IsPlayerAction(_pos))
            {
                throw new ApplicationException(String.Format("{0} : wrong move: {1}", GetBotStateDiagText(), nextStNode.ToStrategicString(null)));
            }
            double amount = nextStNode.Amount;

            double raiseAmount = _gameState.Players[_pos].Bet + amount - _gameState.Bet;
            if (raiseAmount > 0)
            {
                return PokerAction.r(_pos, raiseAmount);
            }
            if (amount == 0 && _gameState.Players[_pos].Bet < _gameState.Bet)
            {
                return PokerAction.f(_pos);
            }
            return PokerAction.c(_pos);
        }

        #endregion

        #region Configuration data

        enum AmountSearchMethod
        {
            Equal,
            Closest
        };

        private const double AMOUNT_EPSILON = 0.001;

        private string _name;
        Props _creationParams;
        DeckDescriptor _deckDescr;
        private IChanceAbstraction[] _chanceAbsrtractions;
        private DiscreteProbabilityRng _moveSelector;
        private StrategyTree[] _strategies;
        UFTreeChildrenIndex[] _strIndexes;
        /// <summary>
        /// Send to server some infomational properirties. 
        /// </summary>
        Props _playerInfoProps;
        AmountSearchMethod _amountSearchMethod;
        bool _checkBlinds = true;
        double _relProbabIgnoreLevel = 0;

        #endregion

        #region Game data

        private GameRecord _gameRecord;
        private int _pos;
        private GameState _gameState;
        private double _lastAbsStrProbab;
        Int64 _curStrNodeIdx;
        int _processedActionsCount;

        #endregion

        #region Trace data

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string _traceDir;

        #endregion
    }
}
