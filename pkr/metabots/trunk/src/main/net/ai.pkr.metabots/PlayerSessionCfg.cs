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
    /// Session-wide configuration parameters of a bot.
    /// </summary>
    [Serializable]
    public class PlayerSessionCfg
    {
        public PlayerSessionCfg()
        {
            Name = "";
            SessionParameters = new Props();
        }


        /// <summary>
        /// Name of the bot. The room will wait for this bot to connect.
        /// </summary>
        public string Name
        {
            set;
            get;
        }

        /// <summary>
        /// Session parameters, will be passed to the bot when the session begins.
        /// </summary>
        public Props SessionParameters
        {
            set;
            get;
        }
    }
}
