/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// A helper class to build unit tests.
    /// We cannot use UTHelper here because it is not tested yet and 
    /// functionality it uses is untested too, therefore we need a special class for 
    /// this library.
    /// </summary>
    static class UTHelperPrivate
    {
        public static string GetTestResourcesPath()
        {
            string subdirName = Path.GetFileNameWithoutExtension(CodeBase.Get(Assembly.GetExecutingAssembly()));
            string testResourcesPath = Path.Combine(Path.GetDirectoryName(CodeBase.Get()), "..");
            if (EnvironmentExt.IsUnix() && !Path.IsPathRooted(testResourcesPath))
            {
                // Fix for mono under linux, it returns a path from root, but without '/' at the
                // beginning.
                testResourcesPath = '/' + testResourcesPath;
            }
            testResourcesPath = Path.Combine(testResourcesPath, "test");
            testResourcesPath = Path.Combine(testResourcesPath, subdirName);
            return testResourcesPath;
        }

        public static string MakeAndGetTestOutputDir(string testName)
        {
            string testDir = Path.Combine("test-output", testName) + Path.DirectorySeparatorChar;
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }
            return testDir;
        }
    }
}
