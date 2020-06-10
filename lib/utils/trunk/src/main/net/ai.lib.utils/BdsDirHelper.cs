/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ai.lib.utils
{
    /// <summary>
    /// A helper class returning names of commonly used directories according to BDS rules.
    /// Is a supplement to Props.Global, which contains the main settings for directories.
    /// </summary>
    public static class BdsDirHelper
    {
        /// <summary>
        /// Adds the short name of the assembly to the BDS data directory.
        /// </summary>
        public static string DataDir(Assembly a)
        {
            return Props.Global.Expand("${bds.DataDir}${0}", AssemblyHelper.GetShortName(a));
        }

        /// <summary>
        /// Returns a directory for Windows 32-bit executables and DLLs-
        /// </summary>
        public static string BinWin32Dir()
        {
            return Props.Global.Expand("${bds.BinDir}${0}", "win32");
        }

        /// <summary>
        /// Returns a directory for Windows 32-bit executables and DLLs-
        /// </summary>
        public static string BinWin64Dir()
        {
            return Props.Global.Expand("${bds.BinDir}${0}", "win64");
        }
    }
}
