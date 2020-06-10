/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ai.lib.utils.commandline;
using ai.pkr.metagame;
using ai.lib.utils;
using System.Threading;
using System.Diagnostics;
using log4net.Repository;
using log4net;
using log4net.Appender;
using ai.pkr.metabots;
using System.Reflection;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace ai.pkr.metatools.pkrserver
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("root");

        static int Main(string[] args)
        {
            Thread runnerThread = null;
            try
            {
                if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
                {
                    return -1;
                }
                if(_cmdLine.DebuggerLaunch)
                {
                    Debugger.Launch();
                }
                SetUpLog4Net();
                log.Info("Server started.");
                Debug.WriteLine("Server started");

                _cmdLineString = string.Join(" ", args);


                _sessionResult.Name = _SESSION_RESULT_NAME;
                _totalResult.Name = "Total result in all sessions";

                // First create all the configurations to make sure everything is ok before
                // the games start.
                _suiteCfgs = new SessionSuiteCfg[_cmdLine.SessionSuites.Length];
                for (int ss = 0; ss < _cmdLine.SessionSuites.Length; ++ss)
                {
                    string sessionSuiteFile = _cmdLine.SessionSuites[ss];
                    _suiteCfgs[ss] = XmlSerializerExt.Deserialize<SessionSuiteCfg>(sessionSuiteFile);

                    foreach (SessionCfg sc in _suiteCfgs[ss].Sessions)
                    {
                        _sessionsTotal += sc.RepeatCount;
                        _gamesTotal += sc.GetEstimatedGamesCount() * sc.RepeatCount;
                    }
                }

                runnerThread = new Thread(Run);
                _startTime = DateTime.Now;
                runnerThread.Start();

                int cnt = 0;
                for (; ; )
                {
                    if (!runnerThread.IsAlive)
                        break;
                    Thread.Sleep(100);
                    if (_cmdLine.Menu)
                    {
                        ProcessMenu();
                        cnt++;
                        if (!_quit && _quitCounter < _quitPressThreshold && (cnt % 10) == 0)
                        {
                            _quitCounter++;
                        }
                    }
                }

                return _exitCode;
            }
            catch (Exception e)
            {
                log.Fatal("Unhandled exception", e);
                Console.Error.Write(e.ToString());
                if(Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                if (runnerThread != null && runnerThread.IsAlive)
                {
                    runnerThread.Abort();
                }
            }
            return -1;
        }

        /// <summary>
        /// Creates unique log file name based on port.
        /// </summary>
        private static void SetUpLog4Net()
        {
            ILoggerRepository repository = LogManager.GetRepository();
            IAppender[] appenders = repository.GetAppenders();
            foreach (IAppender appender in (from iAppender in appenders
                                            where iAppender is FileAppender
                                            select iAppender))
            {
                FileAppender fileAppender = appender as FileAppender;
                string newFileName = Path.Combine(Path.GetDirectoryName(fileAppender.File),
                                                  Path.GetFileNameWithoutExtension(fileAppender.File)) +
                                                  "-" + _cmdLine.Port.ToString() + Path.GetExtension(fileAppender.File);

                fileAppender.File = newFileName;
                fileAppender.ActivateOptions();
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("Menu (case-sensitive):");
            Console.WriteLine("m: show menu");
            Console.WriteLine("s: show suite status");
            Console.WriteLine("r: show total result");
            Console.WriteLine("l: toggle game log printing to console on/off");
            Console.WriteLine("L: show current log file name");
            Console.WriteLine("c: start pkrchart for the current game log");
            Console.WriteLine("p: show command line paramerters");
            Console.WriteLine("b: show information about players"); // b for "bots"
            Console.WriteLine("C: clear screen");
            Console.WriteLine("Q: quit at the next possible point");
            Console.WriteLine();
        }

        private static void ProcessMenu()
        {
            while(Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch(key.KeyChar)
                {
                    case 'm':
                        ShowMenu();
                        break;
                    case 's':
                        ShowSuiteStatus();
                        break;
                    case 'r':
                        ShowTotalResult();
                        break;
                    case 'l':
                        _showLog = !_showLog;
                        break;
                    case 'L':
                        ShowLogFileName();
                        break;
                    case 'c':
                        StartPkrChart();
                        break;
                    case 'p':
                        ShowCommandLineParams();
                        break;
                    case 'b':
                        ShowBots();
                        break;
                    case 'C':
                        Console.Clear();
                        break;
                    case 'Q':
                        ProcessQuit();
                        break;
                }
            }
        }


        static void Run()
        {
            try
            {
                SocketServer server = new SocketServer("127.0.0.1", _cmdLine.Port);
                _isWaitingForRemotePlayers = true;
                WaitRemotePlayers(server, _suiteCfgs, _cmdLine.RemoteTimeout * 1000);
                _isWaitingForRemotePlayers = false;

                for (int ss = 0; ss < _suiteCfgs.Length && !_quit; ++ss)
                {
                    _suitesCount++;
                    _suiteName = _suiteCfgs[ss].Name;

                    SessionSuiteRunner runner = new SessionSuiteRunner();
                    runner.OnGameEnd += new SessionSuiteRunner.OnGameEndHandler(runner_OnGameEnd);
                    runner.OnSessionBegin += new SessionSuiteRunner.OnSessionBeginHandler(runner_OnSessionBegin);
                    runner.RemotePlayers = _remotePlayers;
                    _isWaitingForRemotePlayers = false;
                    runner.Configuration = _suiteCfgs[ss];
                    runner.IsLoggingEnabled = true;
                    if(!String.IsNullOrEmpty(_cmdLine.LogDir))
                    {
                        runner.LogDir = _cmdLine.LogDir;
                    }
                    lock (_thisLock)
                    {
                        _currentSessionSuiteRunner = runner;
                    }
                    runner.Run();

                    if (!String.IsNullOrEmpty(_cmdLine.OnSessionSuiteEnd))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(_cmdLine.OnSessionSuiteEnd, runner.CurrentLogFile);
                        }
                        catch (Exception e)
                        {
                            log.Error(e.ToString());
                        }
                    }
                }

                foreach(SocketServerPlayer rp in _remotePlayers)
                {
                    if (rp.IsConnected)
                    {
                        rp.OnServerDisconnect("Session suites finished");
                        rp.Disconnect();
                    }
                }
            }
            catch (Exception e)
            {
                _exitCode = -1;
                log.Fatal("Unhandled exception in runner thread", e);
                Console.Error.Write(e.ToString());
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        /// <summary>
        /// Waits for remote players required for session suites.
        /// </summary>
        /// <param name="timeout">Total timeout (ms) for all players</param>
        static void WaitRemotePlayers(SocketServer server, SessionSuiteCfg[] sessionSuiteCfgs, int timeout)
        {
            foreach (SessionSuiteCfg ssc in sessionSuiteCfgs)
            {
                _requiredRemotePlayers.UnionWith(ssc.FindRemotePlayers());
            }


            DateTime startTime = DateTime.Now;

            for (; _requiredRemotePlayers.Count > 0; )
            {
                int waitTime = (int)(DateTime.Now - startTime).TotalMilliseconds;
                if (waitTime > timeout)
                    break;
                SocketServerPlayer socketPlayer = server.Listen(timeout - waitTime);
                if (socketPlayer != null)
                {
                    socketPlayer.PlayerInfo = socketPlayer.OnServerConnect();
                    lock (_thisLock)
                    {
                        _remotePlayers.Add(socketPlayer);
                        if (_requiredRemotePlayers.Contains(socketPlayer.PlayerInfo.Name))
                        {
                            _requiredRemotePlayers.Remove(socketPlayer.PlayerInfo.Name);
                        }
                    }
                }
            }
        }

        static void runner_OnSessionBegin(SessionCfg sessionCfg, int repetition)
        {
            _sessionsCount++;
            _curSessionCfg = sessionCfg;
            _sessionName = sessionCfg.Name;
            _sessionGamesCount = 0;
            _sessionRepetition = repetition;
            _sessionGamesTotal = sessionCfg.GetEstimatedGamesCount();
            if(repetition == 0)
            {
                _sessionResult = new TotalResult();
                _sessionResult.Name = _SESSION_RESULT_NAME;
            }
        }

        static void runner_OnGameEnd(GameRecord gameRecord)
        {
            lock (_thisLock)
            {
                _gamesCount++;
                _sessionGamesCount++;
                _lastGameId = gameRecord.Id;
                _totalResult.Update(gameRecord);
                _sessionResult.Update(gameRecord);
            }

            if(_showLog)
            {
                Console.WriteLine(gameRecord.ToString());
            }
        }

        static void ShowSuiteStatus()
        {
            if (_isWaitingForRemotePlayers)
            {
                lock (_thisLock)
                {
                    Console.Write("Waiting for remote players: ");
                    foreach (string n in _requiredRemotePlayers)
                    {
                        Console.Write(n + " ");
                    }
                    Console.WriteLine();
                    Console.Write("Already connected: ");
                    foreach (SocketServerPlayer p in _remotePlayers)
                    {
                        Console.Write(p.PlayerInfo.Name + " ");
                    }
                }
                Console.WriteLine();
            }
            else
            {
                DateTime curTime = DateTime.Now;
                TimeSpan runTime = curTime - _startTime;
                runTime = TimeSpan.FromSeconds(Math.Round(runTime.TotalSeconds));

                int gamesCount = _gamesCount == 0 ? 1 : _gamesCount; // avoid div by 0.

                TimeSpan restTime = TimeSpan.FromSeconds(runTime.TotalSeconds / gamesCount * (_gamesTotal - gamesCount));
                restTime = TimeSpan.FromSeconds(Math.Round(restTime.TotalSeconds));
                DateTime endTime = curTime + restTime;

                Console.WriteLine("Suite: '{0}', {1} of {2}",
                    _suiteCfgs[_suitesCount - 1].Name, _suitesCount, _suiteCfgs.Length);
                if (_curSessionCfg == null)
                {
                    Console.WriteLine("Sessions are not started yet");
                }
                else
                {
                    Console.WriteLine("Session: '{0}', {1} of {2}. Repetition: index {3}, count {4}",
                                      _sessionName, _sessionsCount, _sessionsTotal,
                                      _sessionRepetition, _curSessionCfg.RepeatCount);
                    Console.WriteLine("Game: {0} of {1}, {2:0.0%}",
                                      _gamesCount, _gamesTotal, (double) _gamesCount/_gamesTotal);
                    Console.WriteLine("Game in current session: id '{0}', {1} of {2}, {3:0.0%}",
                                      _lastGameId, _sessionGamesCount, _sessionGamesTotal,
                                      (double) _sessionGamesCount/_sessionGamesTotal);
                    Console.WriteLine("Started at : {0}, run  time: {1}, {2:0.00} g/s", 
                        _startTime.ToString("dd.MM HH:mm:ss"), runTime, gamesCount / (runTime.TotalSeconds + 0.001)); // Avoid div by 0
                    Console.WriteLine("Est. end at: {0}, rest time: {1}", endTime.ToString("dd.MM HH:mm:ss"), restTime);
                }
            }
            Console.WriteLine();
        }

        static void ShowTotalResult()
        {
            lock (_thisLock)
            {
                _sessionResult.Print(Console.Out);
                Console.WriteLine();
                _totalResult.Print(Console.Out);
                Console.WriteLine();
            }
        }

        private static void StartPkrChart()
        {
            string currentLogFile = null;
            currentLogFile = GetCurrentLogFileName(currentLogFile);
            if(String.IsNullOrEmpty(currentLogFile))
            {
                Console.WriteLine("Log is not available");
                return;
            }
            try
            {
                string codebaseDir = Path.GetDirectoryName(CodeBase.Get());
                string exePath = Path.Combine(codebaseDir, "pkrchart.exe");
                string cmdLine = "\"" + currentLogFile + "\"";
                log.InfoFormat("Starting: {0} {1}", exePath, cmdLine);
                Process.Start(exePath, cmdLine);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static string GetCurrentLogFileName(string currentLogFile)
        {
            lock (_thisLock)
            {
                if (_currentSessionSuiteRunner != null)
                {
                    currentLogFile = _currentSessionSuiteRunner.CurrentLogFile;
                }
            }
            return currentLogFile;
        }

        private static void ShowLogFileName()
        {
            string currentLogFile = null;
            currentLogFile = GetCurrentLogFileName(currentLogFile);
            if (String.IsNullOrEmpty(currentLogFile))
            {
                currentLogFile = "N/A";
            }
            Console.WriteLine("Current log: {0}", currentLogFile);
        }

        private static void ShowCommandLineParams()
        {
            Console.WriteLine(_cmdLineString);
            Console.WriteLine();
        }

        private static void ShowBots()
        {
            PlayerInfo[] infos = null;
            lock (_thisLock)
            {
                if (_currentSessionSuiteRunner != null)
                {
                    infos = _currentSessionSuiteRunner.GetPlayerInfos();
                }
            }
            if(infos == null)
            {
                Console.WriteLine("Player info is not available yet");
                return;
            }
            for (int p = 0; p < infos.Length; ++p)
            {
                Console.WriteLine("Player: {0}", infos[p].Name ?? "N/A");
                Console.WriteLine("Version: {0}", infos[p].Version != null ? infos[p].Version.ToString() : "N/A");
                Console.WriteLine("Propeties:");
                if (infos[p].Properties != null)
                {
                    string [] propNames = infos[p].Properties.Names;
                    for (int i = 0; i < infos[p].Properties.Count; ++i)
                    {
                        Console.WriteLine("{0}: {1}", propNames[i], infos[p].Properties.Get(propNames[i]) ?? "null");
                    }
                }
                Console.WriteLine();
            }
        }

        private static void ProcessQuit()
        {
            if(!_quit && --_quitCounter == 0)
            {
                _quit = true;
                log.Info("Quit set to true on user request.");
                lock (_thisLock)
                {
                    if (_currentSessionSuiteRunner != null)
                    {
                        _currentSessionSuiteRunner.QuitAtNextPossiblePoint = true;
                    }
                }
            }
            if(_quit)
            {
                Console.WriteLine("Program will quit at the next possible point.");
            }
            else
            {
                Console.WriteLine("Press {0} more times to activate quit.", _quitCounter);
            }
        }

        #region Data members

        private static SessionSuiteCfg[] _suiteCfgs;
        static bool _showLog = false;

        private static int _suitesCount = 0;
        private static string _suiteName = "";

        private static int _sessionsCount = 0;
        private static int _sessionsTotal = 0;
        private static string _sessionName = "";

        private static int _gamesCount = 0;
        private static int _gamesTotal = 0;

        private static int _sessionGamesCount = 0;
        private static int _sessionRepetition = 0;
        private static int _sessionGamesTotal = 0;
        private static string _lastGameId = "";

        private static DateTime _startTime;

        static bool _isWaitingForRemotePlayers = false;

        private static SessionCfg _curSessionCfg;
        private static TotalResult _totalResult = new TotalResult();
        private static TotalResult _sessionResult = new TotalResult();
        const string _SESSION_RESULT_NAME = "Result in current session (all repetitions)";

        private static List<SocketServerPlayer> _remotePlayers = new List<SocketServerPlayer>();
        static HashSet<string> _requiredRemotePlayers = new HashSet<string>();

        static CommandLine _cmdLine = new CommandLine();
        static private string _cmdLineString = "";

        private static SessionSuiteRunner _currentSessionSuiteRunner;

        static private Object _thisLock = new Object();

        private const int _quitPressThreshold = 3;
        private static int _quitCounter = _quitPressThreshold;
        static bool _quit = false;
        private static int _exitCode = 0;

        #endregion
    }
}
