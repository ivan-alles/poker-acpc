/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.metastrategy
{
    public interface IDealerAction: IStrategicAction
    {
        /// <summary>
        /// Card index. One card is enough for model games and
        /// real-game use abstraction anyway. 
        /// </summary>
        int Card { get; }
    }
}
