/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// A player action for strategy game model. Is equivalent for blinds, raise, call and fold of
    /// regular poker.
    /// </summary>
    public interface IPlayerAction: IStrategicAction
    {
        double Amount { get; }
    }
}
