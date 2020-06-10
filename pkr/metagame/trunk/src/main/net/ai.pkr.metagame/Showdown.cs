/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ai.pkr.metagame
{
    /// <summary>
    /// A class to calculate game result using typical pot-splitting rules 
    /// based on hand ranks.
    /// </summary>
    public static class Showdown
    {

        /// <summary>
        /// Calculate game result for Hi games.
        /// Result for each player will be added to result array.
        /// <remarks>There in no guarantee that the content of inpot and ranks is unchanged.</remarks>
        /// </summary>
        public static void CalcualteHi(double[] inpot, UInt32[] ranks, double[] result, double rake)
        {
            // Todo: rake
            if (rake != 0)
                throw new NotImplementedException("rake is not supported yet");
            //
            // Algorithm to calculate the pot share.
            //
            //        #   
            //    #   #
            //    #   # #
            //    #   # #
            //    #   # #   #
            //  # #   # #   #
            //  # #   # #   #
            //  # # # # # # #
            // ----------------
            //  3 2 0 4 4 0 9  ranks
            //  
            // 1. Find max. rank (maxRank), number of players with this rank (winnersCount) and 
            //    the min. inpot among these players (minInPot).
            // 3. Subtract from each player max(minInPot, inpot[i]) and add this value to sidepot.
            // 4. Split sidepot between winners
            // 5. If inpot[i] becomes 0, set ranks[i] to 0.
            // 6. If there is no players with money, exit.

            // result = potshare - inpot, so subtract inpot first.
            for (int p = 0; p < inpot.Length; ++p)
            {
                result[p] -= inpot[p];
            }

            for(;;)
            {
                UInt32 maxRank = 0;
                int winnersCount = 0;
                double minInPot = 0;
                for(int p = 0; p < inpot.Length; ++p)
                {
                    if(ranks[p] > maxRank)
                    {
                        maxRank = ranks[p];
                        winnersCount = 1;
                        minInPot = inpot[p];
                    }
                    else if(ranks[p] == maxRank)
                    {
                        winnersCount ++;
                        if(inpot[p] < minInPot)
                            minInPot = inpot[p];
                    }
                }
                double sidePot = 0;
                for (int p = 0; p < inpot.Length; ++p)
                {
                    double part = Math.Min(inpot[p], minInPot);
                    inpot[p] -= part;
                    sidePot += part;
                }
                sidePot /= winnersCount;
                int moneylessCount = 0;
                for (int p = 0; p < inpot.Length; ++p)
                {
                    if (ranks[p] == maxRank)
                        result[p] += sidePot;
                    if (inpot[p] == 0)
                    {
                        ranks[p] = 0;
                        moneylessCount++;
                    }
                }
                if (moneylessCount == inpot.Length)
                    break;
            }
        }

    }
}