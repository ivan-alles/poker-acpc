/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metabots;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ai.pkr.acpc
{
    /// <summary>
    /// Adapter for ACPC server.
    /// Assumes that the server protocol is line-based, one command/response per line.
    /// </summary>
    public class AcpcServerAdapter
    {
        #region Public API

        public AcpcServerAdapter()
        {
        }

        public IAcpcServerMessageConverter MessageConverter
        {
            set;
            get;
        }

        public bool IsVerbose
        {
            set;
            get;
        }

        public bool IsTrafficVerbose
        {
            set;
            get;
        }

        public bool Connect(string hostName, int port, int timeOutSec)
        {
            if (IsVerbose)
            {
                Console.WriteLine("Connecting to {0}:{1} ...", hostName, port);
            }
            DateTime startTime = DateTime.Now;
            for (; ; )
            {
                try
                {
                    _tc = new TcpClient();
                    _tc.Connect(hostName, port);
                    if(IsVerbose)
                    {
                        Console.WriteLine("Connected to {0}:{1}", hostName, port);
                    }
                    return true;
                }
                catch(SocketException )
                {
                }
                Thread.Sleep(100);
                double time = (DateTime.Now - startTime).TotalSeconds;
                if(time > timeOutSec)
                {
                    break;
                }
            }
            if (IsVerbose)
            {
                Console.WriteLine("Cannot connect to {0}:{1}", hostName, port);
            }
            return false;
        }

        void Write(string message)
        {
            if (IsTrafficVerbose)
            {
                Console.WriteLine("c->s:{0}", message);
            }
            message += MessageConverter.LineTerminator;
            byte[] bytes = new byte[message.Length];
            for (int i = 0; i < message.Length; ++i)
            {
                bytes[i] = (byte) message[i];
            }
            _tc.GetStream().Write(bytes, 0, bytes.Length);
            _tc.GetStream().Flush();
        }

        public void Run()
        {
            try
            {
                Write(MessageConverter.HandshakeMessage);

                for (;;)
                {
                    // This should be compatible with line ending of ACPC '\r\n'
                    string command = ReadLine();
                    if(command == null)
                    {
                        break;
                    }

                    string response = MessageConverter.OnServerMessage(command);

                    if (response != null)
                    {
                        Write(response);
                    }
                }
            }
            catch(SocketException e)
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Disconnect on socket exception: {0}", e.ToString());
                    return;
                }
            }
            if (IsVerbose)
            {
                Console.WriteLine("Disconnect: server closed");
            }
        }

        string ReadLine()
        {
            string line = "";

            for (;;)
            {
                int b = _tc.GetStream().ReadByte();
                if (b == -1)
                {
                    return null;
                }
                char c = (char) b;
                line += c;
                if (line.EndsWith(MessageConverter.LineTerminator))
                {
                    line = line.Substring(0, line.Length - MessageConverter.LineTerminator.Length);
                    if (IsTrafficVerbose)
                    {
                        Console.WriteLine("s->c:{0}", line);
                    }
                    return line;
                }
            }
        }

        public void Disconnect()
        {
            if (_tc != null)
            {
                _tc.Close();
                _tc = null;
            }
        }

        public bool IsConnected
        {
            get { return _tc != null; }
        }

        #endregion

        #region Implementation

        private TcpClient _tc;


        #endregion
    }
}
