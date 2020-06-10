/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ai.lib.utils.commandline;
using ai.lib.algorithms;

namespace ai.pkr.metatools.pkrchart
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!Parser.ParseArgumentsWithUsage(args.Slice(1, args.Length - 1), _cmdLine))
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindow mw = new MainWindow();
            mw.SetCommandLine(_cmdLine);
            Application.Run(mw);
        }

        private static CommandLine _cmdLine = new CommandLine();
    }
}
