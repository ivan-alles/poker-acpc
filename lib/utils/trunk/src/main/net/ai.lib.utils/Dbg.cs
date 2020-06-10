/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace ai.lib.utils
{
    public static class Dbg
    {
        /// <summary>
        /// Waits for debugger to attach.
        /// </summary>
        /// <param name="timeMs">Time in ms to wait. Specify any negative number to wait endless.</param>
        public static void WaitDbgAttach(int timeMs)
        {
            DateTime start = DateTime.Now;
            while (!Debugger.IsAttached)
            {
                if (timeMs >= 0)
                {
                    int runTime = (int)((DateTime.Now - start).TotalMilliseconds);
                    if (runTime > timeMs)
                        return;
                }
                Thread.Sleep(100);
            }
        }
    }
}
