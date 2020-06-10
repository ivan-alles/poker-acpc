/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.lib.algorithms.numbers;

namespace ai.pkr.metastrategy
{
    public static class ActivePlayers
    {
        /// <summary>
        /// Returns an array containing all possible bit masks with active players.
        /// </summary>
        /// <param name="totalCount">Total number of players</param>
        /// <param name="minCount">Inclusive minimal number of active players</param>
        /// <param name="maxCount">Inclusive maximal number of active players</param>
        /// <returns></returns>
        public static UInt16[] Get(int totalCount, int minCount, int maxCount)
        {
            if (totalCount < minCount || totalCount < maxCount)
            {
                throw new ArgumentOutOfRangeException("Total count must be >= than min and max counts");
            }
            int count = 0;
            for (int i = minCount; i <= maxCount; ++i)
            {
                count += (int)EnumAlgos.CountCombin(totalCount, i);
            }

            UInt16[] result = new ushort[count];

            // Use a slow but simple algorithm
            UInt16 maxMask = (UInt16)((1 << totalCount) - 1);
            count = 0;
            for (UInt16 mask = 0; mask <= maxMask; ++mask)
            {
                int bitCount = CountBits.Count(mask);
                if(minCount <= bitCount && bitCount <= maxCount)
                {
                    result[count++] = mask;
                }
            }
            return result;
        }
    }
}
