/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace ai.lib.utils
{
    /// <summary>
    /// A unit test helper class.
    /// </summary>
    public class UTHelper
    {
        /// <summary>
        /// Returns test resource directory: ${bds.TestDir}/ut-assembly-name/. Directory
        /// separator is added at the end.
        /// Example: /ai/test/ai.lib.algorithms.2.nunit/.
        /// </summary>
        /// <param name="utAssembly"></param>
        /// <returns></returns>
        public static string GetTestResourceDir(Assembly utAssembly)
        {
            string libName = Path.GetFileNameWithoutExtension(CodeBase.Get(utAssembly));
            string testDir = Path.Combine(Props.Global.Get("bds.TestDir"), libName);
            testDir += Path.DirectorySeparatorChar;
            return testDir;
        }

        /// <summary>
        /// Creates and returns test output directory: ${bds.TestOutputDir}/ut-assembly-name/. Directory
        /// separator is added at the end.
        /// Example: /ai/var/test-output/ai.lib.algorithms.2.nunit/.
        /// </summary>
        public static string MakeAndGetTestOutputDir(Assembly utAssembly)
        {
            string libName = Path.GetFileNameWithoutExtension(CodeBase.Get(utAssembly));
            string testDir = Path.Combine(Props.Global.Get("bds.TestOutputDir"), libName);
            testDir += Path.DirectorySeparatorChar;
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }
            return testDir;
        }

        /// <summary>
        /// Returns test output directory: ${bds.UtOutputDir}/ut-assembly-name/test-name. Directory
        /// separator is added at the end.
        /// Example: /ai/var/test-output/ai.lib.algorithms.2.nunit/SBR_Test/.
        /// </summary>
        public static string MakeAndGetTestOutputDir(Assembly utAssembly, string testName)
        {
            string libName = Path.GetFileNameWithoutExtension(CodeBase.Get(utAssembly));
            string testDir = Path.Combine(Props.Global.Get("bds.TestOutputDir"), libName);
            testDir = Path.Combine(testDir, testName);
            testDir += Path.DirectorySeparatorChar;
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }
            return testDir;
        }
    }
}
