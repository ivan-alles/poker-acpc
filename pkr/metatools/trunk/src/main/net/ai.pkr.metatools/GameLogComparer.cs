/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.metatools
{
    // Compares 2 poker game logs.
    public static class GameLogComparer
    {
        /// <summary>
        /// Compares two logs and returns true if they are equal. 
        /// If there is a parsing error, throws an ApplicationException().
        /// </summary>
        /// <param name="hint">Contains a hint text about comparison result, e.g. "Games differ: ..."</param>
        /// <param name="count">Compare up to count logs. Set to int.MaxValue to compare all. Set to a negative value to
        /// compare up to the minimal size of the two logs.</param>
        public static bool Compare(string logFileName1, string logFileName2, out string hint, int count)
        {
            GameLogParser parser = new GameLogParser();

            ParsedLogs[] logs = new ParsedLogs[2] {new ParsedLogs(), new ParsedLogs()};


            for (int curLog = 0; curLog < 2; curLog++)
            {
                parser.OnError += new GameLogParser.OnErrorHandler(logs[curLog].OnError);
                parser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logs[curLog].OnGameRecord);
                parser.ParseFile(curLog == 0 ? logFileName1 : logFileName2);
                parser.OnError -= new GameLogParser.OnErrorHandler(logs[curLog].OnError);
                parser.OnGameRecord -= new GameLogParser.OnGameRecordHandler(logs[curLog].OnGameRecord);
            }

            if (count < 0)
                count = Math.Min(logs[0].Games.Count, logs[1].Games.Count);

            int count0 = Math.Min(logs[0].Games.Count, count);
            int count1 = Math.Min(logs[1].Games.Count, count);

            if (count0 != count1)
            {
                hint = String.Format("Log sizes differ: {0} != {1}", count0, count1);
                return false;
            }

            string[] gameStrings = new string[2];

            for (int g = 0; g < count0; ++g)
            {
                for (int l = 0; l < 2; l++)
                {
                    gameStrings[l] = logs[l].Games[g].ToString();
                }
                if (gameStrings[0] != gameStrings[1])
                {
                    hint = String.Format("Games differ:\n{0}\n{1}", gameStrings[0], gameStrings[1]);
                    return false;
                }
            }
            hint = "Logs are equal";
            return true;
        }

        class ParsedLogs
        {
            public List<GameRecord> Games = new List<GameRecord>(10000);
            
            public void OnGameRecord(GameLogParser source, GameRecord gameRecord)
            {
                Games.Add(gameRecord);
            }

            public void OnError(GameLogParser source, string error)
            {
                throw new ApplicationException(source.GetDefaultErrorText(error));
            }
        }
    }
}
