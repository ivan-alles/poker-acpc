/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.utils
{
    /// <summary>
    /// Redirects .NET Debug and Trace to log4net.
    /// </summary>
    public class Log4NetTraceListener : System.Diagnostics.TraceListener
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Write(string message)
        {
            logger.Debug(message);
        }
        public override void WriteLine(string message)
        {
            logger.Debug(message);
        }
    }
}
