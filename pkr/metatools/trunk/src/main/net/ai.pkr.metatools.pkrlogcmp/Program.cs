/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.utils.commandline;

namespace ai.pkr.metatools.pkrlogcmp
{
    /// <summary>
    /// Command line. 
    /// </summary>
    class CommandLine : StandardCmdLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "log",
        HelpText = "Game log file")]
        public string[] Logs = null;

        #endregion

        #region Options
        [Argument(ArgumentType.AtMostOnce , LongName = "count", ShortName = "c",
        HelpText = "Number of games to compare or min for the shortest log.")]
        public string count = null;
        #endregion
    }

    static class Program
    {
        private const int ExitCodeError = 2;
        private const int ExitCodeUnequal = 1;
        private const int ExitCodeEqual = 0;

        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return ExitCodeError;
            }

            if (_cmdLine.Logs.Length != 2)
            {
                Console.Error.WriteLine("Can compare 2 logs but specified {0}", _cmdLine.Logs.Length);
                return ExitCodeError;
            }

            int count = int.MaxValue;
            if (_cmdLine.count != null)
            {
                if (_cmdLine.count == "min")
                    count = -1;
                else
                    count = int.Parse(_cmdLine.count);
            }

            string hint;
            bool result;
            try
            {
                result = GameLogComparer.Compare(_cmdLine.Logs[0], _cmdLine.Logs[1], out hint, count);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return ExitCodeError;
            }

            Console.WriteLine(hint);

            return result ? ExitCodeEqual : ExitCodeUnequal;
        }

        private static CommandLine _cmdLine = new CommandLine();
    }
}
