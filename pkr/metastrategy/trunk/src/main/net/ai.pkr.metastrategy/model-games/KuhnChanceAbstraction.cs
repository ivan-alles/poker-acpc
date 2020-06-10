/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metastrategy.model_games
{
    /// <summary>
    /// Kuhn bucketizer, transforms J->0, Q->1, K->2. Is useful to verify
    /// bucketizing algorithms.
    /// </summary>
    public class KuhnChanceAbstraction: IChanceAbstraction
    {
        public KuhnChanceAbstraction()
        {   
        }

        /// <summary>
        /// Create a class with the properties, can be used for dynamic creation.
        /// <para>Properties: none</para>
        /// </summary>
        public KuhnChanceAbstraction(Props props)
        {
        }

        #region IChanceAbstraction Members

        public string Name
        {
            get 
            { 
                return "Kuhn-CA"; 
            }
        }

        public int GetAbstractCard(int[] hand, int handLength)
        {
            if (handLength != 1)
            {
                throw new ArgumentException("Wrong hand length");
            }
            return hand[0];
        }

        #endregion
    }
}
