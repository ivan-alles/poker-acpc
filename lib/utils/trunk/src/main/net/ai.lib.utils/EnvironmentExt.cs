/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.utils
{
    /// <summary>
    /// Helper functions concerning environment.
    /// </summary>
    public static class EnvironmentExt
    {
        public static bool IsUnix()
        {
            // Taken from: http://www.mono-project.com/FAQ:_Technical
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 6) || (p == 128))
            {
                return true;
            }
            return false;
        }
    }
}
