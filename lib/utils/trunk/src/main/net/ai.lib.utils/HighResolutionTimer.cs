/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace ai.lib.utils
{
    /// <summary>
    /// This class is used to make high resolution timing measurements
    /// <remarks>
    /// Works only on windows platform.
    /// </remarks>
    /// </summary>
    public class HighResolutionTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <exclude/>
        private double startTime;
        /// <exclude/>
        private long freq;

        /// <summary>
        /// Constructor
        /// </summary>
        public HighResolutionTimer()
        {
            startTime = 0.0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported 
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Returns current time value as a double. This value is only
        /// useful for makeing relative time measurements. The relationship of
        /// this value to system time is unknown.
        /// </summary>
        public double CurrentTime
        {
            get {
                long time;
                QueryPerformanceCounter(out time);
                return ((double) time)/((double) freq);
            }
        }

        /// <summary>
        /// Used to mark the start of a time duration measurement
        /// </summary>
        public void Start()
        {
            startTime = CurrentTime;
        }

       /// <summary>
       /// Retreives time since the last time Start() was called.
       /// </summary>
        public double Duration
        {
            get
            {
                return (CurrentTime - startTime);
            }
        }
    }
}

