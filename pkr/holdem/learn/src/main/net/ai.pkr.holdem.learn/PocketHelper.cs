/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.core;
using ai.pkr.metagame;

namespace ai.pkr.holdem.learn
{
    public static class PocketHelper
    {

        public static HePocketKind[] GetAllPockets()
        {
            HePocketKind[] result = new HePocketKind[169];
            for(int i = 0; i < 169; ++i)
            {
                result[i] = (HePocketKind)i;
            }
            return result;
        }

        /// <summary>
        /// Returns a probability distribution assuming that only given pockets can be dealt.
        /// The distribution is bases on pockets count only.
        /// For example, for (AA, 72o) the distribution will be (6/18, 12/18).
        /// </summary>
        /// <param name="pockets"></param>
        /// <returns></returns>
        public static double[] GetProbabDistr(HePocketKind[] pockets)
        {
            int n = pockets.Count();
            double[] result = new double[n];
            int totalPockets = 0;
            for (int c = 0; c < pockets.Length; ++c)
            {
                int count = HePocket.KindToRange(pockets[c]).Length;
                result[c] = count;
                totalPockets += count;
            }
            for (int c = 0; c < pockets.Length; ++c)
            {
                result[c] /= totalPockets;
            }
            return result;
        }

        /// <summary>
        /// Returns a probability distribution assuming that only given pockets can be dealt
        /// and one pocket is already dealt.
        /// The distribution is bases on pockets count only.
        /// For example, for (AA, 72o) the distribution will be (6/18, 12/18).
        /// </summary>
        /// <param name="pockets"></param>
        /// <returns></returns>
        public static double[] GetProbabDistr(HePocketKind[] pockets, CardSet dealtPocket)
        {
            int n = pockets.Count();
            double[] result = new double[n];
            int totalPockets = 0;
            for (int c = 0; c < pockets.Length; ++c)
            {
                CardSet[] range = HePocket.KindToRange(pockets[c]);
                int count = 0;
                foreach(CardSet cs in range)
                {
                    if(!cs.IsIntersectingWith(dealtPocket))
                    {
                        count++;
                    }
                }
                result[c] = count;
                totalPockets += count;
            }
            for (int c = 0; c < pockets.Length; ++c)
            {
                result[c] /= totalPockets;
            }
            return result;
        }
    }
}
