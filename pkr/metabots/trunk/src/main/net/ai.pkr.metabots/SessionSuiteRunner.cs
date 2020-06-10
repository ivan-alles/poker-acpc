/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using ai.lib.utils;
using ai.pkr.metagame;
using System.Reflection;
using ai.lib.algorithms.random;
using ai.lib.algorithms;

namespace ai.pkr.metabots
{
    /// <summary>
    /// SessionSuiteRunner is responsible for executing multiple games. Games are united in sessions.
    /// Game rules (GameDefinition) do not change during a sesion. 
    /// 
    /// A typical use case for bot development is to create a SessionSuiteCfg that
    /// executes all necessary tests for a given bot (or multiple bots),
    /// start a SessionSuiteRunner on the server an let it execute the tests.
    /// 
    /// Local bots are created by the runner. Remote bots connect to the server via network.
    /// 
    /// Some configuration parameters (e.g. relative paths) will be resolved during the run. If you need the original 
    /// values, store a copy of configuration.
    /// </summary>
    /// <seealso cref="IPlayer"/>
    public class SessionSuiteRunner
    {
        #region Public constants

        public readonly static string DefaultLogSubDir = "pkrlog";

        #endregion

        #region Public interface

        public SessionSuiteRunner()
        {
            LogDir = Path.Combine(Props.Global.Get("bds.VarDir"), DefaultLogSubDir);
        }

        public SessionSuiteCfg Configuration
        {
            set;
            get;
        }

        public bool IsLoggingEnabled
        {
            set;
            get;
        }

        /// <summary>
        /// Directory for logs. Default value is "${bds.VarDir}/DefaultLogSubDir".
        /// </summary>
        public string LogDir
        {
            set;
            get;
        }

        public string CurrentLogFile
        {
            get;
            private set;
        }

        public List<SocketServerPlayer> RemotePlayers
        {
            set;
            get;
        }

        /// <summary>
        /// Runs sessions. At the beginning verifies that all local and remote players 
        /// for all sessions are available.
        /// </summary>
        public void Run()
        {
            QuitAtNextPossiblePoint = false;
            CreateLocalPlayers();
            AddRemotePlayers();
            Verify();
            CreateGameLog(LogDir);
            for (_sessionIdx = 0; _sessionIdx < Configuration.Sessions.Length && !QuitAtNextPossiblePoint; _sessionIdx++)
            {
                _sessionCfg = Configuration.Sessions[_sessionIdx];
                _gameDef = _sessionCfg.GameDefinition;
                int rngSeed = _sessionCfg.RngSeed == 0 ? (int)DateTime.Now.Ticks : _sessionCfg.RngSeed;
                for (int sessionRep = 0; sessionRep < _sessionCfg.RepeatCount && !QuitAtNextPossiblePoint; ++sessionRep)
                {
                    PlaySession(sessionRep, rngSeed);
                    if(_sessionCfg.RngSeedStep == 0)
                    {
                        rngSeed += (int)DateTime.Now.Ticks;
                    }
                    else
                    {
                        rngSeed += _sessionCfg.RngSeedStep;
                    }
                }
            }
            Close();
        }

        private void PlaySession(int repetition, int rngSeed)
        {
            if (IsLoggingEnabled)
            {
                _gameLog.WriteLine(">OnSessionBegin Name=\'{0}\' Repetition=\'{1}\' RngSeed=\'{2}\'", _sessionCfg.Name, repetition, rngSeed);
                _gameLog.Flush();
            }

            if (OnSessionBegin != null)
                OnSessionBegin(_sessionCfg, repetition);

            // Create a list of session players in the same order as in session configuration.
            _sessionPlayers = new List<PlayerData>();
            foreach (PlayerSessionCfg sessionBotCfg in _sessionCfg.Players)
            {
                PlayerData sessionPlayer = _suitePlayers.FirstOrDefault(delegate(PlayerData pd)
                                                                       {
                                                                           return pd.info.Name ==
                                                                                  sessionBotCfg.Name;
                                                                       });
                if (sessionPlayer == null)
                {
                    throw new ApplicationException(String.Format("Missing player: {0}", sessionBotCfg.Name));
                }
                else
                {
                    _sessionPlayers.Add(sessionPlayer);
                    // Reset stack to the default value at the beginning of each session.
                    // Todo: decide if we need to configure starting stack, if yes add it here.
                    // Starting stacks must be a property of SessionConfiguration, not of a PlayerSessionConfiguration,
                    // because of seat permutations.
                    sessionPlayer.stack = 0;
                    sessionPlayer.iplayer.OnSessionBegin(_sessionCfg.Name, _gameDef,
                                                         sessionBotCfg.SessionParameters);
                }
            }

            switch (_sessionCfg.Kind)
            {
                case SessionKind.RingGame:
                    PlaySimpleSession(rngSeed);
                    break;
                case SessionKind.RingGameWithSeatPermutations:
                    PlayRingGameSessionWithSeatPermutations(rngSeed);
                    break;
            }

            foreach (PlayerData sessionPlayer in _sessionPlayers)
            {
                sessionPlayer.iplayer.OnSessionEnd();
            }

            if (IsLoggingEnabled)
            {
                _gameLog.WriteLine(">OnSessionEnd");
                _gameLog.Flush();
            }

            if (OnSessionEnd != null)
                OnSessionEnd();
        }

        /// <summary>
        /// Stops Run() method at the next possible point, depending on the session type, so that 
        /// the session result remain consistent.
        /// <para>- Ring games: exits at the next game.</para>
        /// <para>- Ring games with seat permuatations: exits at the end of the session.</para>
        /// </summary>
        public bool QuitAtNextPossiblePoint
        {
            set;
            get;
        }

        #endregion

        #region Public events
        /// <summary>
        /// Is called on session begin.
        /// Log meta-data: &lt;OnSessionBegin Name='Session name' Repetition='rep' RngSeed='seed'
        /// </summary>
        /// <param name="sessionCfg">Session configuration</param>
        /// <param name="repetition">Repetition, in range [0, sessionCfg.RepeatCount)</param>
        public delegate void OnSessionBeginHandler(SessionCfg sessionCfg, int repetition);
        /// <summary>
        /// Is called on session end.
        /// Log meta-data: &lt;OnSessionEnd
        /// </summary>
        public delegate void OnSessionEndHandler();
        public delegate void OnGameEndHandler(GameRecord gameRecord);

        public event OnSessionBeginHandler OnSessionBegin;
        public event OnSessionEndHandler OnSessionEnd;
        public event OnGameEndHandler OnGameEnd;

        #endregion

        #region Implementation

        void CreateLocalPlayers()
        {
            string homeNamespace = "ai.pkr.metabots.bots.";
            string homeAssembly = Assembly.GetExecutingAssembly().FullName;
            foreach (LocalPlayerCfg localBotCfg in Configuration.LocalPlayers)
            {
                string type = localBotCfg.Type.Get(Props.Global);
                if(type.StartsWith(homeNamespace) && localBotCfg.Assembly.Get(Props.Global) == "")
                {
                    type += ", " + homeAssembly;
                }
                IPlayer iplayer = ClassFactory.CreateInstance<IPlayer>(type, localBotCfg.Assembly.Get(Props.Global));
                if (iplayer != null)
                {
                    Props creationParams = localBotCfg.CreationParameters;
                    if(Configuration.XmlParams != null && Configuration.XmlParams.XmlFile != null)
                    {
                        creationParams.Set("pkr.SessionSuiteXmlFileName", Configuration.XmlParams.XmlFile);
                    }
                    iplayer.OnCreate(localBotCfg.Name, creationParams);
                    AddLocalPlayer(iplayer, localBotCfg.Name);
                }
            }
        }

        private void AddLocalPlayer(IPlayer player, string expectedName)
        {
            PlayerData playerData = new PlayerData();
            playerData.info = player.OnServerConnect();
            if (playerData.info == null)
            {
                string error = "PlayerInfo cannot be null";
                player.OnServerDisconnect(error);
                throw new ApplicationException(error);
            }

            if (!String.IsNullOrEmpty(expectedName) && playerData.info.Name != expectedName)
            {
                string error = String.Format("Player must accept name '{0}' given in OnCreate().", expectedName);
                player.OnServerDisconnect(error);
                throw new ApplicationException(error);
            }

            playerData.iplayer = player;
            playerData.isLocal = true;
            _suitePlayers.Add(playerData);
        }

        void AddRemotePlayers()
        {
            if (RemotePlayers == null)
                return;
            HashSet<string> expectedNames = Configuration.FindRemotePlayers();
            foreach (SocketServerPlayer sp in RemotePlayers)
            {
                if (!expectedNames.Contains(sp.PlayerInfo.Name))
                    continue;
                AddRemotePlayer(sp);
            }
        }

        private void AddRemotePlayer(SocketServerPlayer player)
        {
            PlayerData playerData = new PlayerData();
            playerData.iplayer = player;
            playerData.info = player.PlayerInfo;
            _suitePlayers.Add(playerData);
        }

        void Verify()
        {
            HashSet<string> availablePlayers = new HashSet<string>();
            foreach(PlayerData pd in _suitePlayers)
            {
                if(availablePlayers.Contains(pd.info.Name))
                {
                    throw new ApplicationException(String.Format("Duplicate player name {0}", pd.info.Name));
                }
                availablePlayers.Add(pd.info.Name);
            }
            HashSet<string> missingPlayers = Configuration.GetPlayers();
            missingPlayers.ExceptWith(availablePlayers);
            if (missingPlayers.Count > 0)
            {
                throw new ApplicationException(String.Format("Missing players: {0}", 
                    string.Join(" ", missingPlayers.ToArray())));
            }
        }

        private void Close()
        {
            if(_gameLog != null)
                _gameLog.Close();

            foreach (PlayerData playerData in _suitePlayers)
            {
                if (playerData.isLocal)
                {
                    playerData.iplayer.OnServerDisconnect("SessionSuite finished - disconnect local players");
                    playerData.iplayer = null;
                }
            }
            _suitePlayers.Clear();
        }


        /// <summary>
        /// Deals cards for the next game. Is used if not replaying from game log.
        /// </summary>
        GameRecord DealCards()
        {
            GameRecord dealRecord = new GameRecord();

            int cardsToDealCount = 0;

            for (int r = 0; r < _gameDef.RoundsCount; ++r)
            {
                cardsToDealCount +=
                    (_gameDef.PrivateCardsCount[r] + _gameDef.PublicCardsCount[r]) * _sessionPlayers.Count
                     + _gameDef.SharedCardsCount[r];
            }

            if (cardsToDealCount > 0)
            {
                _dealer.Shuffle(cardsToDealCount);
                
                int dealt = 0;
                for (int r = 0; r < _gameDef.RoundsCount; ++r)
                {
                    if (_gameDef.PrivateCardsCount[r] > 0)
                    {
                        for (int p = 0; p < _sessionPlayers.Count; ++p)
                        {
                            dealRecord.Actions.Add(PokerAction.d(p, _gameDef.DeckDescr.GetCardNames(
                                                                        _dealer.Sequence, dealt,
                                                                        _gameDef.PrivateCardsCount[r])));
                            dealt += _gameDef.PrivateCardsCount[r];
                        }
                    }

                    if (_gameDef.PublicCardsCount[r] > 0)
                    {
                        for (int p = 0; p < _sessionPlayers.Count; ++p)
                        {
                            dealRecord.Actions.Add(PokerAction.d(p, _gameDef.DeckDescr.GetCardNames(
                                                                        _dealer.Sequence, dealt,
                                                                        _gameDef.PublicCardsCount[r])));
                            dealt += _gameDef.PublicCardsCount[r];
                        }
                    }

                    if (_gameDef.SharedCardsCount[r] > 0)
                    {
                        dealRecord.Actions.Add(PokerAction.d(_gameDef.DeckDescr.GetCardNames(
                                                                 _dealer.Sequence, dealt, _gameDef.SharedCardsCount[r])));
                        dealt += _gameDef.SharedCardsCount[r];
                    }
                }
            }

            return dealRecord;
        }

        /// <summary>
        /// True if replaying an existing game log.
        /// </summary>
        bool IsReplaying()
        {
            return _sessionCfg.ReplayFrom.Get(Props.Global) != "";
        }

        /// <summary>
        /// Creates an underlying RNG for random algorithms.
        /// Now MersenneTwister is used, it is under our control and guarantees the same behaviour
        /// on each plattform.
        /// </summary>
        Random CreateUnderlyingRng(int rngSeed)
        {
            return new MersenneTwister(rngSeed);
        }

        /// <summary>
        /// Plays a ring game session or one subsession of seat-permutation session.
        /// </summary>
        void PlaySimpleSession(int rngSeed)
        {
            _dealer = new SequenceRng(CreateUnderlyingRng(rngSeed), _gameDef.DeckDescr.FullDeckIndexes);
            int gamesCount = _sessionCfg.GamesCount;
            if (IsReplaying())
            {
                _dealLog.Clear();
                GameLogParser gameLogParser = new GameLogParser();
                gameLogParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(gameLogParser_OnGameRecord);
                gameLogParser.OnError += new GameLogParser.OnErrorHandler(gameLogParser_OnError);

                gameLogParser.ParsePath(_sessionCfg.ReplayFrom.Get(Props.Global));
                gamesCount = Math.Min(_sessionCfg.GamesCount, _dealLog.Count);
            }

            for (int gameNumber = 0; gameNumber < gamesCount; gameNumber++)
            {
                if(_sessionCfg.Kind == SessionKind.RingGame && QuitAtNextPossiblePoint)
                {
                    break;
                }

                GameRecord dealRecord;

                if(IsReplaying())
                {
                    dealRecord = _dealLog[gameNumber];
                    for(int a = 0; a < dealRecord.Actions.Count;)
                    {
                        switch(dealRecord.Actions[a].Kind)
                        {
                            case Ak.d:
                                // Keep these actions.
                                ++a;
                                break;
                            default:
                                // Remove these actions
                                dealRecord.Actions.RemoveAt(a);
                                break;
                        }
                    }
                }
                else
                {
                    dealRecord = DealCards();
                    dealRecord.Id = gameNumber.ToString();
                }

                GameRunner game = new GameRunner(_gameDef, _sessionPlayers, dealRecord);
                GameRecord finalGameRecord = game.Run();

                if (IsLoggingEnabled)
                {
                    _gameLog.WriteLine(finalGameRecord.ToString());
                    _gameLog.Flush();
                }

                if (OnGameEnd != null)
                    OnGameEnd(finalGameRecord);

                if (!_sessionCfg.FixButton)
                {
                    // Move button: [0, 1, 2] -> [1, 2, 0]
                    _sessionPlayers.RotateMinCopy(_sessionPlayers.Count - 1);
                }
            }
        }

        void gameLogParser_OnError(GameLogParser source, string error)
        {
            throw new ApplicationException(source.GetDefaultErrorText(error));
        }

        void gameLogParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            _dealLog.Add(gameRecord);
        }

        void PlayRingGameSessionWithSeatPermutations(int rngSeed)
        {
            List<PlayerData> initialPlacement = new List<PlayerData>(_sessionPlayers);
            List<List<int>> permuts = EnumAlgos.GetPermut(_sessionPlayers.Count);
            foreach (List<int> permut in permuts)
            {
                string permutText = "";
                for(int i = 0; i < permut.Count; ++i)
                {
                    _sessionPlayers[i] = initialPlacement[permut[i]];
                    permutText += " " + permut[i].ToString();
                }
                if (IsLoggingEnabled)
                {
                    _gameLog.WriteLine(">SeatPermutation Positions=\'{0}\'", permutText.Substring(1));
                    _gameLog.Flush();
                }

                Props pm = new Props();
                pm.Set("pkr.ForgetAll", "true");

                foreach (PlayerData sessionPlayer in _sessionPlayers)
                {
                    sessionPlayer.iplayer.OnSessionEvent(pm);
                }

                PlaySimpleSession(rngSeed);
            }
        }

        void CreateGameLog(string logsFolder)
        {
            if (IsLoggingEnabled)
            {
                if (logsFolder == "")
                    logsFolder = ".";
                if (!Directory.Exists(logsFolder))
                    Directory.CreateDirectory(logsFolder);
                string path = logsFolder + System.IO.Path.DirectorySeparatorChar +
                              GetLogName();
                if(!Path.IsPathRooted(path))
                {
                    // Convert to absolute path to avoid problems if CD changes.
                    path = Path.Combine(Directory.GetCurrentDirectory(), path);
                }
                _gameLog = new StreamWriter(path, true);
                CurrentLogFile = path;
            }
        }

        private string GetLogName()
        {
            StringBuilder name = new StringBuilder("gl-");
            try
            {
                String machineName = Environment.MachineName;
                String date = DateTime.Now.ToString("yyMMdd-HHmmss");
                String cfgName = Configuration.Name;
                if (cfgName != "")
                    name.Append(cfgName);
                if (machineName != "")
                    name.Append("-" + machineName);
                if (date != "")
                    name.Append("-" + date);
                foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
                    name.Replace(invalidChar, '_');
                name.Replace(' ', '_');
            }
            catch
            {
            }
            name.Append(".log");
            return name.ToString();
        }

        public PlayerInfo [] GetPlayerInfos()
        {
            PlayerInfo [] infos = new PlayerInfo[_suitePlayers.Count];
            for (int i = 0; i < _suitePlayers.Count; ++i)
            {
                infos[i] = _suitePlayers[i].info;
            }
            return infos;
        }

        #endregion

        #region Data members

        private List<PlayerData> _suitePlayers = new List<PlayerData>();
        private int _sessionIdx;
        private SessionCfg _sessionCfg;
        private GameDefinition _gameDef;
        private List<PlayerData> _sessionPlayers;
        private SequenceRng _dealer;
        private List<GameRecord> _dealLog = new List<GameRecord>();
        private StreamWriter _gameLog = null;

        #endregion
    }
}
