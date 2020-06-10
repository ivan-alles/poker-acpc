/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.pkr.metagame
{
    /// <summary>
    /// Game actions.
    /// <remarks>
    /// <para>
    /// These constants are widely used throughout all the sources.
    /// Besides, the same actions are used in other documents, for instance game logs and 
    /// tree images. Therefore very short but still expressive names are chosen.
    /// </para>
    /// <para>These values are also used in files that may contain a huge number of entries.
    /// Therefore the encoding that allows the most compact bit representation was chosen.
    /// It is guaranteed that any value can be encoded using at most 3 bits.
    /// </para>
    /// </remarks>
    /// </summary>
    public enum Ak
    {

        /// <summary>Blinds, antes (can be grouped together without influence on strategy)
        /// or other forced bets.</summary>
        b = 0,
        /// <summary>Deal</summary>
        d = 1,
        /// <summary>Fold</summary>
        f = 2,
        /// <summary>Call</summary>
        c = 3,
        /// <summary>Raise</summary>
        r = 4
    }
}
