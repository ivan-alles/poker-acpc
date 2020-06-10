/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;

namespace ai.pkr.metabots
{
    /// <summary>
    /// Information about a player. A player sends it to the server on connection.
    /// </summary>
    public class PlayerInfo
    {
        public PlayerInfo()
        {
        }

        public PlayerInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name of the bot.
        /// </summary>
        public string Name
        {
            set;
            get;
        }

        /// <summary>
        /// Version of the assembly of the bot.
        /// </summary>
        public BdsVersion Version
        {
            set;
            get;
        }

        /// <summary>
        /// The bot can send data about itself for informational purposes,
        /// e.g. name of local configuration file or number of entries in a player database.
        /// </summary>
        public Props Properties
        {
            set;
            get;
        }
    }
}
