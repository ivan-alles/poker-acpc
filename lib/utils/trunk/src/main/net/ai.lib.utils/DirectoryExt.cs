/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ai.lib.utils
{
    /// <summary>
    /// Helpers to work with directories.
    /// </summary>
    public static class DirectoryExt
    {
        /// <summary>
        /// Copy directory structure recursively. This functionality is missing in .NET Directory class. 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void Copy(string src, string dst)
        {
            String[] Files;
            if (dst[dst.Length - 1] != Path.DirectorySeparatorChar)
                dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
            Files = Directory.GetFileSystemEntries(src);
            foreach (string Element in Files)
            {
                // Sub directories
                if (Directory.Exists(Element))
                    Copy(Element, dst + Path.GetFileName(Element));
                // Files in directory
                else
                    File.Copy(Element, dst + Path.GetFileName(Element), true);
            }
        }

        /// <summary>
        /// Deletes a directory. If it does not exists, no exception is thrown (this is what usually needed).
        /// </summary>
        /// <param name="name"></param>
        public static void Delete(string name)
        {
            if (Directory.Exists(name))
            {
                Directory.Delete(name, true);
            }
        }

    }
}
