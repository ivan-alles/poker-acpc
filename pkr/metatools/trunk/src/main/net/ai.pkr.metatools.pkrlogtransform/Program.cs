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
using System.Globalization;

namespace ai.pkr.metatools.pkrlogtransform
{
    internal class GameLimitException : Exception
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
            _transformer = new TransformGameRecords();

            _outputName = _cmdLine.Output;
            if (String.IsNullOrEmpty(_outputName))
            {
                _outputName = Path.Combine(Path.GetDirectoryName(_cmdLine.InputFile), Path.GetFileNameWithoutExtension(_cmdLine.InputFile));
                _outputName += "-tr" + Path.GetExtension(_cmdLine.InputFile);
            }

            _output = new StreamWriter(_outputName);

            if (!string.IsNullOrEmpty(_cmdLine.RenameEq))
            {
                    
                string []parts = _cmdLine.RenameEq.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                _transformer.RenameEqName = parts[0];
                _transformer.RenameEqNewName = parts[1];
            }
            if (!string.IsNullOrEmpty(_cmdLine.RenameNeq))
            {
                string[] parts = _cmdLine.RenameNeq.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                _transformer.RenameNeqName = parts[0];
                _transformer.RenameNeqNewName = parts[1];
            }

            _transformer.FinalizeGames = _cmdLine.FinalizeGames;
            if(_cmdLine.RenumerateGames)
            {
                _transformer.RenumerateGames = true;
                _transformer.GameCount = 0;
            }
            _transformer.HideOpponentCards = _cmdLine.HideOpponentCards;
            _transformer.NormalizeCards = _cmdLine.NormalizeCards;
            _transformer.ResetStacks = _cmdLine.ResetStacks;
            _transformer.ResetResults = _cmdLine.ResetResults;
            if (string.IsNullOrEmpty(_cmdLine.NormalizeStakes))
            {
                _transformer.NormalizeStakes = -1.0;
            }
            else
            {
                _transformer.NormalizeStakes = double.Parse(_cmdLine.NormalizeStakes, CultureInfo.InvariantCulture);
            }
            _transformer.HeroName = _cmdLine.HeroName;
            _transformer.RemoveNoHeroMoves = _cmdLine.RemoveNoHeroMoves;
            _transformer.RemoveNoShowdown = _cmdLine.RemoveNoShowdown;

            GameLogParser logParser = new GameLogParser { Verbose = _cmdLine.Verbose };
            logParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logParser_OnGameRecord);
            logParser.OnMetaData += new GameLogParser.OnMetaDataHandler(logParser_OnMetaData);
            try
            {
                logParser.ParseFile(_cmdLine.InputFile);
            }
            catch (GameLimitException)
            {
            }

            _output.Flush();

            _output.Close();

            return 0;
        }

        static void logParser_OnMetaData(GameLogParser source, string metaData)
        {
            //GameLogMetaData md = GameLogMetaData.Parse(metaData);
            if (!_cmdLine.RemoveMetadata)
            {
                _output.WriteLine(metaData);
            }
        }

        static void logParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            if (source.GamesCount > _cmdLine.GameLimit)
            {
                throw new GameLimitException();
            }

            if (!_transformer.Transform(gameRecord))
            {
                return;
            }
            _output.WriteLine(gameRecord.ToGameString());
        }

        #region Data

        static CommandLine _cmdLine = new CommandLine();
        private static string _outputName = null;
        private static TextWriter _output;
        static TransformGameRecords _transformer;


        #endregion
    }
}
