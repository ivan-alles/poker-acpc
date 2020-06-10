/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.metastrategy
{
    public interface IStrategicAction
    {
        /// <summary>
        /// Player position. 
        /// For root node of game trees is set by convention to the number of players, this allows to easily 
        /// find out the number of players of the game.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// Returns a game string. Parameters may contain implementation-depeding data allowing special formatting,
        /// for instance card names.
        /// </summary>
        string ToStrategicString(object parameters);
    }
}
