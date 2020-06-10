/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace ai.pkr.metabots
{
    public class SocketServer
    {
        /// <summary>
        /// Creates a server on the specified local interface.
        /// </summary>
        /// <param name="localAddress">IP address.</param>
        /// <param name="port">Port</param>
        public SocketServer(string localAddress, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(localAddress);
            _listener = new TcpListener(ipAddress, port);
        }

        public SocketServerPlayer Listen(int timeout)
        {
            DateTime startTime = DateTime.Now;
            _listener.Start(11);
            while (!_listener.Pending())
            {
                if ((DateTime.Now - startTime) > TimeSpan.FromMilliseconds(timeout))
                {
                    return null;
                }
                Thread.Sleep(50);
            }
            TcpClient tc = _listener.AcceptTcpClient();
            Debug.WriteLine("Socket player connected");
            return new SocketServerPlayer(tc);
        }

        private TcpListener _listener;
    }
}
