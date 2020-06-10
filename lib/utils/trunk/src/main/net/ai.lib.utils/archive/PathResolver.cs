/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ai.lib.utils
{
    // Conserved for possible future use, see also PathResolver_Test.
#if false
    /// <summary>
    /// Resolves a path.
    /// 
    /// Algorithm:
    /// 1. If path is null or empty, return path.
    /// 2. Substitute variables. Environment variables has prefix "env.", for instance "${env.PATH}".
    /// 3. If the given path is absolute, return this path.
    /// 4. If the given path is relative, merge is with paths from searchPaths and see, if the file exists.
    ///    The first found path is returned.
    /// 5. If no file found, the original path is returned.
    /// </summary>
    [Obsolete("Use Props, PropString instead")]
    public static class PathResolver
    {
        static Dictionary<string, string> _envVariables = null;

        public static string Resolve(string path, string [] searchPaths)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }

            if(_envVariables == null)
            {
                UpdateEnvironmentVariables();
            }

            path = TextVariables.Substitute(path, _envVariables);

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            if (searchPaths != null)
            {
                for (int i = 0; i < searchPaths.Length; ++i)
                {
                    string mergedPath = Path.Combine(searchPaths[i], path);
                    if (File.Exists(mergedPath) || Directory.Exists(mergedPath))
                    {
                        return mergedPath;
                    }
                }
            }
            return path;
        }

        /// <summary>
        /// Re-reads environment variables. Is called automatically when first needed.
        /// Call it to explicitely update the variables.
        /// </summary>
        public static void UpdateEnvironmentVariables()
        {
            _envVariables = new Dictionary<string, string>();
            IDictionary envDict = Environment.GetEnvironmentVariables();
            foreach (object key in envDict.Keys)
            {
                object value = envDict[key];
                _envVariables["env." + key.ToString()] = value.ToString();
            }
        }

        /// <summary>
        /// Returns a non-uri path of the code base of the assembly.
        /// </summary>
        [Obsolete("Use CodeBase class")]
        public static string GetCodeBase(Assembly assembly)
        {
            string uriPath = assembly.CodeBase;
            string normalPath = Regex.Replace(uriPath, "file:///(?<1>.*)", "${1}");
            return normalPath;
        }

        /// <summary>
        /// Returns a non-uri path of the code base of this assembly (ai.lib.utils).
        /// </summary>
        [Obsolete("Use CodeBase class")]
        public static string GetCodeBase()
        {
            return GetCodeBase(Assembly.GetExecutingAssembly());
        }

    }
#endif
}
