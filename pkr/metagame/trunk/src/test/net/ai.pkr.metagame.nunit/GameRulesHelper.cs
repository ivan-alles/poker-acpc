/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using ai.pkr.metagame;

namespace ai.pkr.metagame.nunit
{
    /// <summary>
    /// Implementation of IGameRules for UTs.
    /// </summary>
    public class GameRulesHelper: IGameRules
    {
        #region IGameRules Members

        virtual public void OnCreate(Props config)
        {
            OnCreateCount++;
            Config = config;
        }


        virtual public void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
        {
            ShowdownCount++;
        }

        #endregion

        #region Data Members

        public Props Config;

        #endregion

        #region Method call counters

        public int OnCreateCount;
        public int InitializeCount;
        public int DealCount;
        public int ShowdownCount;

        #endregion
    }
}
