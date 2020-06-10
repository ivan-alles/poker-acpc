/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem;
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
using ai.pkr.stdpoker;
using ai.pkr.holdem.strategy;
using ai.bds.utils;


namespace ai.pkr.bots.neytiri
{
    /// <summary>
    /// A bot that plays he.fl.2 against bots with static strategies (like FellOmen2).
    /// <para>
    /// Parameters:
    /// </para>
    /// <para>Strategy (string, required): path to strategy file</para>
    /// <para>MonteCarloCount (string, optional): comma-separated list of MC 
    /// repetitions counts for each round, for example:<para>
    /// "-1, 5000, 3000, 3000"</para></para>
    /// 
    /// </summary>
    /// <seealso cref="http://de.james-camerons-avatar.wikia.com/wiki/Neytiri"/>
    public class Neytiri: IPlayer
    {
        static Neytiri()
        {
            _traceDir = Dirs.DirByCodebase(Dirs.TraceDir);

            if (!Directory.Exists(_traceDir))
                Directory.CreateDirectory(_traceDir);
        }

        #region IPlayer Members

        public virtual void OnCreate(string name, PropertyMap creationParameters)
        {
            _name = name;
            log.InfoFormat("{0} OnCreate() initializion...", _name);
            _creationParams = new PropertyMap(creationParameters);
        }

        public PlayerInfo OnServerConnect()
        {
            return new PlayerInfo(_name);
        }

        public void OnServerDisconnect(string reason)
        {
        }

        public virtual void OnSessionBegin(string sessionName, GameDefinition gameDef, lib.utils.PropertyMap sessionParameters)
        {
            // Debugger.Break();
            _gameDef = gameDef;
            if(!_isInitialized)
            {
                // Do initialization here instead of in OnCreate() to allow the server start-up faster.
                string sessionSuiteDir = Path.GetDirectoryName(_creationParams["pkr.SessionSuiteXmlFileName"]);
                string strategyPath = PathResolver.Resolve(_creationParams["Strategy"], new string[]{sessionSuiteDir});
                XmlSerializerExt.Deserialize(out _strategy, strategyPath);
                HandStrength.LoadPrecalculationTables();
                string[] mcReps = _creationParams.GetValueDef("MonteCarloCount", "-1, 5000, 3000, 3000").Split(
                    new char[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                _monteCarloRepetitions = new int[mcReps.Length];
                for(int i = 0; i < mcReps.Length; ++i)
                {
                    _monteCarloRepetitions[i] = int.Parse(mcReps[i]);
                }
                _isInitialized = true;
            }
        }

        public void OnSessionEvent(PropertyMap parameters)
        {
        }

        public void OnSessionEnd()
        {
        }

        public virtual void OnGameBegin(string gameString)
        {
            _traceCount = 0;
            UpdateFromGameString(gameString);
            Debug.Assert(_gameState.Players.Length == 2);
            _pos = _gameState.FindPositionByName(_name);
            _strategyPath.Clear();
            _strategyPath.Add(_strategy.Positions[1 - _pos]);
            log.InfoFormat("{0} OnGameBegin() pos {1}", _name, _pos);
            
            _roundWithKnownStrategy = 0; // Preflop strategy is known.
        }

        public void OnGameUpdate(string gameString)
        {
        }

        public virtual PokerAction OnActionRequired(string gameString)
        {
            log.InfoFormat("{0} OnActionRequired", _name);
            UpdateFromGameString(gameString);

            PokerAction result = null;
            ProcessPlayerActions();
            if (_roundWithKnownStrategy != _gameState.Round)
            {
                // New round started, calcualate strategy
                MonteCarloStrategyFinder.DoMonteCarlo(_strategy,
                    _pos, _pocket, _gameState.Round, _gameState.SharedCards,
                    _strategyPath, _monteCarloRepetitions[_gameState.Round]);
                _roundWithKnownStrategy = _gameState.Round;
                TraceState(_gameState);
            }
            result = GetBestAction(_gameState);

            Debug.Assert(result != null);
            log.InfoFormat("{0} OnActionRequired() return {1}", _name, result.Kind);

            return result;
        }

        public virtual void OnGameEnd(string gameString)
        {
        }

        #endregion

        string GetTraceNameBase(GameState CurGameState)
        {
            string name = string.Format("neytiri-{0}-{1}-{2:00}-",
                CurGameState.Id, CurGameState.Round, _traceCount++);
            return Path.Combine(_traceDir, name);
        }

        void TraceState(GameState CurGameState)
        {
            if(!_isTraceActive)
            {
                return;
            }
            string logNameBase = GetTraceNameBase(CurGameState);
            ActionTreeVisualizer v = new ActionTreeVisualizer();
            v.StrategyPath = _strategyPath;
            v.PruneIf = c => c.Node.State.Round > CurGameState.Round;


            if (CurGameState.Round > 0)
            {
                string treePath = "/";
                foreach (ActionTreeNode n in _strategyPath)
                {
                    treePath += n.ActionKind.ToString() + "/";
                }
                v.MatchPath = treePath;
                v.ShowExpr.Add(new VisTreeShowExpr("Node.Value", "\\nV: {1:#.00}"));
            }
            using (TextWriter wr = new StreamWriter(Path.Combine(logNameBase, "str.gv")))
            {
                v.Write(_strategy, _strategy.Positions[1 - _pos], wr);
            }
        }

        int ActionPreference(Ak a)
        {
            switch (a)
            {
                case Ak.c:
                    return 100;
                case Ak.r:
                    return 50;
                case Ak.f:
                    return 0;
            }
            Debug.Assert(false);
            return -1;
        }
        
        private PokerAction GetBestAction(GameState CurGameState)
        {
            Debug.Assert(CurStrategyNode.State.CurrentActor == _pos);
            double maxVal = double.MinValue;
            ActionTreeNode bestNode = null;
            foreach (ActionTreeNode child in CurStrategyNode.Children)
            {
                double childValue = CurGameState.Round == 0 ? child.PreflopValues[(int)_pocketKind] : child.Value;
                if (childValue > maxVal)
                {
                    bestNode = child;
                    maxVal = childValue;
                }
                else if(childValue == maxVal)
                {
                    if(ActionPreference(child.ActionKind) > ActionPreference(bestNode.ActionKind))
                        bestNode = child;
                }
            }

            PokerAction result;
            if (bestNode != null)
            {
                result = new PokerAction {Kind = bestNode.ActionKind};
            }
            else
            {
                Debug.Assert(CurStrategyNode.Children.Count > 0);
                // We have never been here - just call
                result = PokerAction.c(0);
            }
            return result;
        }

        private void ProcessPlayerActions()
        {
            for(int a = _strategyPath.Count - 1; a < _gameRecord.Actions.Count; ++a)
            {
                PokerAction pa = _gameRecord.Actions[a];
                log.InfoFormat("{0} process {1} of pos {2}", _name, pa.Kind, pa.Position);
                ActionTreeNode nextNode = CurStrategyNode.FindChildByAction(pa.Kind);
                Debug.Assert(nextNode.State.LastActor == pa.Position);
                _strategyPath.Add(nextNode);
                if(pa.Kind == Ak.d && pa.Position == _pos)
                {
                    OnDeal(pa.Cards);
                }
            }
        }

        private void OnDeal(string cards)
        {
            _sei.Reset();
            _pocket = StdDeck.Descriptor.GetCardSet(cards);
            _sePocket = _sei.Convert(_pocket);
            _pocketKind = HePockets.CardSetToPocketKind(_sePocket);
            log.InfoFormat("{0} OnDeal() pocket {1} iso {2} kind {3}", _name, _pocket, _sePocket, _pocketKind);
        }

        private ActionTreeNode CurStrategyNode 
        {
            get { return _strategyPath[_strategyPath.Count - 1]; }
        }

        private void UpdateFromGameString(string gameString)
        {
            _gameRecord = new GameRecord(gameString);
            _gameState = new GameState(_gameRecord, _gameDef);
        }

        #region Data

        private string _name;
        private GameState _gameState;
        private GameRecord _gameRecord;
        private ActionTree _strategy;
        private List<ActionTreeNode> _strategyPath = new List<ActionTreeNode>(30);
        private int _pos;
        private CardSet _pocket, _sePocket;
        private HePocketKind _pocketKind;
        NormSuit _sei = new NormSuit();
        private int[] _monteCarloRepetitions;
        private int _roundWithKnownStrategy;
        PropertyMap _creationParams;
        private GameDefinition _gameDef;
        bool _isInitialized = false;

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string _traceDir;
        bool _isTraceActive = false; // Todo: use log4net to set this.
        private int _traceCount = 0;


        #endregion
    }
}
