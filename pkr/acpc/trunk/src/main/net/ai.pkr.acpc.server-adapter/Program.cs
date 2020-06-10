/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using ai.pkr.metabots;
using System.Reflection;
using ai.lib.utils;
using System.IO;
using System.Diagnostics;

namespace ai.pkr.acpc.server_adapter
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return 1;
            }
            if (_cmdLine.DebuggerLaunch)
            {
                Debugger.Launch();
            }

            return RunAcpcSession();
        }

        private static int RunAcpcSession()
        {
            if(_cmdLine.Verbose)
            {
                Console.Out.WriteLine("Starting ACPC session in directory {0}", Directory.GetCurrentDirectory());
            }

            string [] addressParts = _cmdLine.ServerAddress.Get(Props.Global).Split(
                new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);

            string hostName;
            int port;
            if(addressParts.Length != 2 || !int.TryParse(addressParts[1], out port))
            {
                Console.Error.WriteLine("Wrong server address ");
                return -1;
            }
            hostName = addressParts[0];

            Acpc11ServerMessageConverter converter = new Acpc11ServerMessageConverter();

            IPlayer player = CreatePlayer();

            converter.Player = player;
            converter.PlayerName = BotName;

            AcpcServerAdapter adapter = new AcpcServerAdapter();
            adapter.MessageConverter = converter;
            adapter.IsVerbose = _cmdLine.Verbose;
            adapter.IsTrafficVerbose = _cmdLine.VerboseTraffic;

            if (!adapter.Connect(hostName, port, _cmdLine.ConnectTimeout))
            {
                return -1;
            }

            adapter.Run();

            player.OnSessionEnd();

            if(_cmdLine.Verbose)
            {
                Console.WriteLine("Games played: {0}", converter.GameCount);
            }

            return 0;
        }

        static IPlayer CreatePlayer()
        {
            ClassFactoryParams cfp = new ClassFactoryParams(_cmdLine.BotClass.Get(Props.Global));
            IPlayer iplayer = ClassFactory.CreateInstance<IPlayer>(cfp);
            if (iplayer != null)
            {
                Props creationParams =
                    XmlSerializerExt.Deserialize<Props>(_cmdLine.CreationParametersFileName.Get(Props.Global));
                iplayer.OnCreate(BotName, creationParams);

                PlayerInfo pi = iplayer.OnServerConnect();

                if(_cmdLine.Verbose)
                {
                    Console.WriteLine("Player.OnServerConnect() returned:");
                    Console.WriteLine("Name: {0}", pi.Name);
                    Console.WriteLine("Version: {0}", pi.Version);
                }
                iplayer.OnSessionBegin(SessionName, null, null);
            }
            return iplayer;
        }

        static readonly string BotName = "PkrBot";
        static readonly string SessionName = "AcpcSession";
        static CommandLine _cmdLine = new CommandLine();
    }
}
