/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.IO;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metatools.pkrlogstat
{
    internal class GameLimitException: Exception
    {
    }

    class Program
    {
        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return 1;
            }

            char[] sep = new char[] {'='};
            foreach(string param in _cmdLine.ReportParameters)
            {
                string[] nameVal = param.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if(nameVal.Length != 2)
                {
                    Console.Error.WriteLine("Wrong report parameter: {0}", param);
                    return 1;
                }
                _reportParameters.Set(nameVal[0], nameVal[1]);
            }

            if(!String.IsNullOrEmpty(_cmdLine.Output))
            {
                _output = new StreamWriter(_cmdLine.Output);
            }

            DateTime start = DateTime.Now;

            GameLogParser logParser = new GameLogParser{Verbose = _cmdLine.Verbose, IncludeFiles = _cmdLine.IncludeFiles};
            logParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logParser_OnGameRecord);
            logParser.OnMetaData += new GameLogParser.OnMetaDataHandler(logParser_OnMetaData);

            if (_cmdLine.TotalResult)
            {
                _totalResult = CreateGameLogReport("Total result");
            }
            if (_cmdLine.SessionResult)
            {
                _sessionResult = CreateGameLogReport("Unnamed session");
            }
            if (_isHelpShown)
            {
                return 0;
            }
            _sessionGamesCount = 0;

            if (_cmdLine.CountGames || _cmdLine.TotalResult || _cmdLine.SessionResult)
            {
                foreach (string path in _cmdLine.InputPaths)
                {
                    try
                    {
                        logParser.ParsePath(path);
                    }
                    catch (GameLimitException )
                    {
                    }
                }
            }

            if (_cmdLine.SessionResult && _sessionGamesCount > 0)
            {
                // There was an incomplete session.
                _sessionResult.Print(_output);
            }

            if (_cmdLine.TotalResult)
            {
                _totalResult.Print(_output);
            }

            // Print game count after session result, so that results are not interrupted.
            if (_cmdLine.CountGames)
            {
                _output.WriteLine("{0} games in {1} files, {2} errors", logParser.GamesCount, logParser.FilesCount, logParser.ErrorCount);
            }

            double sec = (DateTime.Now - start).TotalSeconds;

            if (_cmdLine.Time)
            {
                _output.WriteLine("Run time {0:0.0} seconds", sec);
            }

            _output.Flush();

            return 0;
        }

        static IGameLogReport CreateGameLogReport(string name)
        {
            IGameLogReport rep;
            if (String.IsNullOrEmpty(_cmdLine.ReportClass))
            {
                rep = new TotalResult();
            }
            else
            {
                ClassFactoryParams cfp = new ClassFactoryParams(_cmdLine.ReportClass);
                rep = ClassFactory.CreateInstance<IGameLogReport>(cfp);
            }
            rep.Name = name;
            rep.Configure(_reportParameters);

            // Show help only once
            if (_cmdLine.ShowReportHelp && !_isHelpShown)
            {
                rep.ShowHelp(_output);
                _isHelpShown = true;
            }

            return rep;
        }

        static void  logParser_OnMetaData(GameLogParser source, string metaData)
        {
            GameLogMetaData md = GameLogMetaData.Parse(metaData);
            if(md == null)
            {
                source.ErrorCount++;
                Console.Error.WriteLine(source.GetDefaultErrorText("Unknown metadata: " + metaData));
            }
            if(md.Name == "OnSessionBegin")
            {
                if(!md.Properties.ContainsKey("Repetition") || int.Parse(md.Properties["Repetition"]) == 0)
                {
                    // The very first repetition.
                    if (_cmdLine.SessionResult && _sessionGamesCount > 0)
                    {
                        _sessionResult.Print(_output);
                    }
                    if (_cmdLine.SessionResult)
                    {
                        _sessionResult =
                            CreateGameLogReport(md.Properties.ContainsKey("Name")
                                                    ? md.Properties["Name"]
                                                    : "Unnamed session");
                    }
                    _sessionGamesCount = 0;
                }
            }
        }

        static void logParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            if (source.GamesCount > _cmdLine.GameLimit)
            {
                throw new GameLimitException();
            }
            if (_sessionResult != null)
            {
                _sessionResult.Update(gameRecord);
            }
            if (_totalResult != null)
            {
                _totalResult.Update(gameRecord);
            }
            _sessionGamesCount++;
        }

        #region Data

        static CommandLine _cmdLine = new CommandLine();
        private static IGameLogReport _totalResult;
        private static IGameLogReport _sessionResult;
        static private int _sessionGamesCount = 0;
        private static Props _reportParameters = new Props();
        private static TextWriter _output = Console.Out;
        private static bool _isHelpShown = false;

        #endregion
    }
}
