/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ai.lib.utils
{
    public static class CodeBase
    {
        /// <summary>
        /// Returns a non-uri path of the code base of the assembly.
        /// </summary>
        public static string Get(Assembly assembly)
        {
            string uriPath = assembly.CodeBase;
            string normalPath = Regex.Replace(uriPath, "file:///(?<1>.*)", "${1}");
            return normalPath;
        }

        /// <summary>
        /// Returns a non-uri path of the code base of this assembly (ai.lib.utils).
        /// </summary>
        public static string Get()
        {
            return Get(Assembly.GetExecutingAssembly());
        }

    }
}
