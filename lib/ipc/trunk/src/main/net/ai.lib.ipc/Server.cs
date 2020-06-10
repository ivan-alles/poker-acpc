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

namespace ai.lib.ipc
{
    /// <summary>
    /// Todo: is not ready yet, just started.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Creates a server on the specified local interface.
        /// </summary>
        /// <param name="address">port</param>
        public Server(string address)
        {
        }

        public void Start(string address)
        {
            IPAddress ipAddress = IPAddress.Any;
            int port = int.Parse(address);
            _listener = new TcpListener(ipAddress, port);

            //_thread = new Thread(ThreadFunc);
            //_thread.Start();

            IAsyncResult ar = _listener.BeginAcceptSocket(new AsyncCallback(DoAcceptSocketCallback), null);
        }

        void ThreadFunc(object param)
        {
        }

        // Process the client connection.
        public void DoAcceptSocketCallback(IAsyncResult ar)
        {
            Socket clientSocket = _listener.EndAcceptSocket(ar);
        }

        private TcpListener _listener;
        private Thread _thread;
    }
}
