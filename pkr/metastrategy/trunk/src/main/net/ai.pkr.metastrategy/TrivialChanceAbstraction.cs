/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Trivial chance abstraction. The abstract card corresponds to the last card of the hand.
    /// For games with a single card dealt in each round the abstracted game equals to the 
    /// original game.
    /// </summary>
    public class TrivialChanceAbstraction: IChanceAbstraction
    {
        public TrivialChanceAbstraction()
        {   
        }

        /// <summary>
        /// Create a class with the properties, can be used for dynamic creation.
        /// <para>Properties: none</para>
        /// </summary>
        public TrivialChanceAbstraction(Props props)
        {
        }

        #region IChanceAbstraction Members

        public string Name
        {
            get 
            {
                return "Trivial-CA"; 
            }
        }

        public int GetAbstractCard(int[] hand, int handLength)
        {
            return hand[handLength-1];
        }

        #endregion

    }
}
