/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metabots;

namespace ai.pkr.acpc
{
    /// <summary>
    /// Message converter for line-based ACPC protocol.
    /// </summary>
    public interface IAcpcServerMessageConverter
    {
        IPlayer Player
        {
            get;
        }

        /// <summary>
        /// Line terminator. Is handled by the adapter
        /// (cut off in commands and added to responses).
        /// </summary>
        string LineTerminator
        {
            get;
        }

        string HandshakeMessage { get; }

        string OnServerMessage(string message);

        int GameCount
        {
            get;
        }
    }
}
