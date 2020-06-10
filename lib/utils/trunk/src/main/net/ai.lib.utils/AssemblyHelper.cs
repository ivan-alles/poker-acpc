/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ai.lib.utils
{
    /// <summary>
    /// Functions to help working with assemblies.
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// Returns short name of the assembly.
        /// </summary>
        public static string GetShortName(Assembly assembly)
        {
            AssemblyName n = new AssemblyName(assembly.FullName);
            return n.Name;
        }
    }
}
