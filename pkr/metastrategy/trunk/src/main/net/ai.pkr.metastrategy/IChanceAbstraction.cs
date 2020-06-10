/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Chance abstraction. 
    /// There is no information about position. To use different abstractions for each positon, multiple 
    /// instances of this interface must be created. This allows to easily combine different 
    /// abstraction from existing code.
    /// </summary>
    public interface IChanceAbstraction
    {
        /// <summary>
        /// Some identifying name.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Convert a real-life card to an abstract card. 
        /// </summary>
        /// <param name="hand">Card indexes of the hand</param>
        /// <param name="handLength">Length of the hand. 
        /// Usage of this parameter allows to reuse hand[] array for all rounds.</param>
        /// <returns>Abstract card index, must be non-negative.</returns>
        int GetAbstractCard(int [] hand, int handLength);
    }
}
