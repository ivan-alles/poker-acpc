/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using ai.lib.utils.commandline;
using ai.lib.utils;
using ai.pkr.metabots;
using ai.pkr.metagame;

namespace ai.pkr.bifaces.agentgui_exe
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string [] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                MessageBox.Show("Wrong command line");
                return 1;
            }
            if (_cmdLine.DebuggerLaunch)
            {
                Debugger.Launch();
            }

            if (Debugger.IsAttached)
            {
                Run();
            }
            else
            {
                try
                {
                    Run();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }

            return 0;
        }

        private static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindow mw = new MainWindow();
            mw.Player = CreatePlayer();
            mw.GameDef = XmlSerializerExt.Deserialize<GameDefinition>(_cmdLine.GameDef);
            Application.Run(mw);
        }

        static IPlayer CreatePlayer()
        {
            ClassFactoryParams cfp = new ClassFactoryParams(_cmdLine.BotClass.Get(Props.Global));
            IPlayer iplayer = ClassFactory.CreateInstance<IPlayer>(cfp);
            if (iplayer != null)
            {
                Props creationParams =
                    XmlSerializerExt.Deserialize<Props>(_cmdLine.CreationParametersFileName.Get(Props.Global));
                iplayer.OnCreate(_botName, creationParams);
            }
            return iplayer;
        }

        static readonly string _botName = "A";
        static CommandLine _cmdLine = new CommandLine();
    }
}
