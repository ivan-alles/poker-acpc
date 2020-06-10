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
    /// Configuration of a local bot.
    /// </summary>
    public class LocalPlayerCfg
    {
        public LocalPlayerCfg()
        {
            CreationParameters = new Props();
            Assembly = "";
        }

        /// <summary>
        /// Optional assembly file containing the bot's class. 
        /// </summary>
        public PropString Assembly
        {
            set;
            get;
        }

        /// <summary>
        /// Full name of the type. If Assembly is unspecified, 
        /// assembly name must follow the type name separated by comma.
        /// If the bot belongs to this assembly, only full type name will do.
        /// </summary>
        public PropString Type
        {
            set;get;
        }

        /// <summary>
        /// Name of the bot. Will be passed to the bot in OnCreate().
        /// The bot must accept this name.
        /// </summary>
        public string Name
        {
            set;
            get;
        }

        /// <summary>
        /// Configuration in user-defined format. Will be passed to the bot in OnCreate().
        /// </summary>
        public Props CreationParameters
        {
            set;get;
        }
    }
}
