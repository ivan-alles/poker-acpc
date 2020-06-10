/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using ai.lib.utils;

namespace ai.pkr.metabots
{
    [XmlRootAttribute("SessionSuiteCfg", Namespace = "ai.pkr.metabots.SessionSuiteCfg.xsd")]
    public class SessionSuiteCfg
    {
        public SessionSuiteCfg()
        {
            LocalPlayers = new LocalPlayerCfg[0];
        }

        public string Name
        {
            set;
            get;
        }

        public LocalPlayerCfg[] LocalPlayers
        {
            set; get;
        }

        public SessionCfg[] Sessions
        {
            set;get;
        }

        /// <summary>
        /// Finds names of remote players that play in sessions.
        /// </summary>
        /// <returns></returns>
        public HashSet<string> FindRemotePlayers()
        {
            HashSet<string> remotePlayerNames = new HashSet<string>();

            foreach (SessionCfg sc in Sessions)
            {
                foreach (PlayerSessionCfg psc in sc.Players)
                {
                    LocalPlayerCfg localPlayer = LocalPlayers.FirstOrDefault(
                        delegate(LocalPlayerCfg lpc) { return lpc.Name == psc.Name; });
                    if (localPlayer != null)
                        continue; // This is a local player.
                    if (!remotePlayerNames.Contains(psc.Name))
                        remotePlayerNames.Add(psc.Name);
                }
            }
            return remotePlayerNames;
        }

        /// <summary>
        /// Returns a hashset containing names of the players that take part in sessions.
        /// If a local player is unused, it is not included.
        /// </summary>
        public HashSet<string> GetPlayers()
        {
            HashSet<string> playerNames = new HashSet<string>();

            foreach (SessionCfg sc in Sessions)
            {
                foreach (PlayerSessionCfg psc in sc.Players)
                {
                    if (!playerNames.Contains(psc.Name))
                        playerNames.Add(psc.Name);
                }
            }
            return playerNames;
        }

        #region Serialization

        public void ConstructFromXml(ConstructFromXmlParams parameters)
        {
            XmlParams = parameters;
            if (Sessions != null)
            {
                foreach (SessionCfg sc in Sessions)
                {
                    sc.ConstructFromXml(parameters);
                }
            }
        }

        /// <summary>
        /// Parameters passed to ConstructFromXml, for resolving references later on.
        /// </summary>
        [XmlIgnore]
        public ConstructFromXmlParams XmlParams
        {
            get;
            set;
        }

        #endregion

    }
}
